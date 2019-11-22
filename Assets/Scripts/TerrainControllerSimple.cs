using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainControllerSimple : MonoBehaviour {

    [SerializeField]
    private GameObject terrainTilePrefab = null;
	[SerializeField]
	private GameObject terrainLandmarkPrefab = null;
	[SerializeField]
	private int landmarkDensity = 5;
	[SerializeField]
	private float landmarkSpread = 100000f;
	[SerializeField]
    private Vector3 terrainSize = new Vector3(20, 1, 20);
	[SerializeField]
	private bool bUpdateTiles = false;
	[SerializeField]
	private int structureDensity = 15;
	[SerializeField]
    private Gradient gradient;
	[SerializeField]
	private float noiseScale = 3, cellSize = 1;
	[SerializeField]
    private int radiusToRender = 5;
    [SerializeField]
    private Transform[] gameTransforms;
    [SerializeField]
    private Transform playerTransform;

    private Vector2 startOffset;
    private Dictionary<Vector2, GameObject> terrainTiles = new Dictionary<Vector2, GameObject>();
    private Vector2[] previousCenterTiles;
    private List<GameObject> previousTileObjects = new List<GameObject>();
	private GameSystem game;

	private bool bLoaded = false;
	private bool bLandShaped = false;
	
	public void SetPlayer(Transform value)
	{
		playerTransform = value;
		if (!bLoaded)
		{
			InitialLoad();
		}
	}
	
	private void Start()
	{
		game = FindObjectOfType<GameSystem>();
        if (playerTransform != null)
		{
			InitialLoad();
		}
    }
	
	public void InitialLoad() {
		SpawnLandmarks();
		DestroyTerrain();

		//choose a place on perlin noise (which loops after 256)
		startOffset = new Vector2(Random.Range(0f, 256f), Random.Range(0f, 256f));

		bLoaded = true;
    }
	
	private void Update() {
        //save the tile the player is on
        Vector2 playerTile = TileFromPosition(playerTransform.position);
        //save the tiles of all tracked objects in gameTransforms (including the player)
        List<Vector2> centerTiles = new List<Vector2>();
        centerTiles.Add(playerTile);
        foreach (Transform t in gameTransforms)
            centerTiles.Add(TileFromPosition(t.position));

        //if no tiles exist yet or tiles should change
        if (previousCenterTiles == null || (bUpdateTiles && HaveTilesChanged(centerTiles))) {
            List<GameObject> tileObjects = new List<GameObject>();
            //activate new tiles
            foreach (Vector2 tile in centerTiles) {
                bool isPlayerTile = tile == playerTile;
                int radius = isPlayerTile ? radiusToRender : 1;
                for (int i = -radius; i <= radius; i++)
                    for (int j = -radius; j <= radius; j++)
                        ActivateOrCreateTile((int)tile.x + i, (int)tile.y + j, tileObjects);
            }
            //deactivate old tiles
            foreach (GameObject g in previousTileObjects)
                if (!tileObjects.Contains(g))
                    g.SetActive(false);

            previousTileObjects = new List<GameObject>(tileObjects);
        }

        previousCenterTiles = centerTiles.ToArray();

		// Spawning objects
		if (!bLandShaped &&
			(previousTileObjects.Count >= (radiusToRender * radiusToRender)))
		{
			int numObjs = previousTileObjects.Count;
			for (int i = 0; i < numObjs; i++)
			{
				GameObject terrain = previousTileObjects[i].gameObject;
				if (!terrain.GetComponent<GenerateMeshSimple>().bGarnished
					&& (Vector3.Distance(terrain.transform.position, Vector3.zero) >= 1000f))
				{
					GarnishTile(terrain, terrain.transform.position);
				}
			}

			bLandShaped = true;
			if (game != null)
			{
				game.SetStartPosition();
			}
		}
	}

	//Helper methods below
	
	private void SpawnLandmarks()
	{
		for (int i = 0; i < landmarkDensity; i++)
		{
			Vector3 spawnPosition = (Random.insideUnitSphere * landmarkSpread);
			spawnPosition.y = playerTransform.position.y;
			GameObject landmark = Instantiate(terrainLandmarkPrefab, spawnPosition, Quaternion.identity);
			TerrainLandmark lm = landmark.GetComponent<TerrainLandmark>();
		}
	}
	
	private void ActivateOrCreateTile(int xIndex, int yIndex, List<GameObject> tileObjects) {
		if (!terrainTiles.ContainsKey(new Vector2(xIndex, yIndex))) {
            tileObjects.Add(CreateTile(xIndex, yIndex));
		} else {
            GameObject t = terrainTiles[new Vector2(xIndex, yIndex)];
			tileObjects.Add(t);
            if (!t.activeSelf)
                t.SetActive(true);
			///GarnishTile(t, t.transform.position);
        }
    }
	
	private void GarnishTile(GameObject tile, Vector3 location)
	{
		// Add objects
		int randomNumber = Mathf.FloorToInt(Random.Range(0f, structureDensity));
		if (randomNumber > 0)
		{
			for (int i = 0; i < randomNumber; i++)
			{
				ObjectSpawner spawner = FindObjectOfType<ObjectSpawner>();
				if (spawner != null)
				{
					spawner.SpawnObjectNearby(location, terrainSize.x, false);
				}
			}
		}

		tile.GetComponent<GenerateMeshSimple>().bGarnished = true;
	}
	
	private GameObject CreateTile(int xIndex, int yIndex) {
		Vector3 tilePosition = new Vector3(terrainSize.x * xIndex, terrainSize.y + transform.position.y, terrainSize.z * yIndex);
		GameObject terrain = Instantiate(
            terrainTilePrefab,
			tilePosition,
            Quaternion.identity
        );
        terrain.name = TrimEnd(terrain.name, "(Clone)") + " [" + xIndex + " , " + yIndex + "]";

		terrainTiles.Add(new Vector2(xIndex, yIndex), terrain);

		GenerateMeshSimple gm = terrain.GetComponent<GenerateMeshSimple>();
		gm.TerrainSize = terrainSize;
        gm.Gradient = gradient;
        gm.NoiseScale = noiseScale;
        gm.CellSize = cellSize;
        gm.NoiseOffset = NoiseOffset(xIndex, yIndex);
        gm.Generate();

		GarnishTile(terrain, terrain.transform.position);

		return terrain;
    }
	
	private Vector2 NoiseOffset(int xIndex, int yIndex) {
        Vector2 noiseOffset = new Vector2(
            (xIndex * noiseScale + startOffset.x) % 256,
            (yIndex * noiseScale + startOffset.y) % 256
        );
        //account for negatives (ex. -1 % 256 = -1). needs to loop around to 255
        if (noiseOffset.x < 0)
            noiseOffset = new Vector2(noiseOffset.x + 256, noiseOffset.y);
        if (noiseOffset.y < 0)
            noiseOffset = new Vector2(noiseOffset.x, noiseOffset.y + 256);
        return noiseOffset;
    }
	
	private Vector2 TileFromPosition(Vector3 position) {
        return new Vector2(Mathf.FloorToInt(position.x / terrainSize.x + .5f), Mathf.FloorToInt(position.z / terrainSize.z + .5f));
    }
	
	private bool HaveTilesChanged(List<Vector2> centerTiles) {
        if (previousCenterTiles.Length != centerTiles.Count)
            return true;
        for (int i = 0; i < previousCenterTiles.Length; i++)
            if (previousCenterTiles[i] != centerTiles[i])
                return true;
        return false;
    }
	
	public void DestroyTerrain() {
        foreach (KeyValuePair<Vector2, GameObject> kv in terrainTiles)
            Destroy(kv.Value);
        terrainTiles.Clear();
    }

	private static string TrimEnd(string str, string end) {
        if (str.EndsWith(end))
            return str.Substring(0, str.LastIndexOf(end));
        return str;
    }

}