using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class HUD : MonoBehaviour
{
	public GameObject vehiclePointer;
	public GameObject recallPrompt;
	public GameObject crosshair;
	public GameObject objectivePointer;
	public GameObject spearAquirePanel;
	public GameObject lootAquirePanel;
	public GameObject loadingPanel;
	public GameObject throwingChargePanel;
	public GameObject throwingChargeBar;
	public Text vehicleDistanceText;
	public Text objectiveDistanceText;
	public Text framerateText;
	public AudioMixer masterMixer;
	public Slider masterVolumeSlider;
	public Slider sensitivitySlider;
	public List<GameObject> abilityAquirePool;

	private ItemBar itemBar;
	private EquippedInfo info;
	private Animator animator;
	private GameSystem game;
	private SmoothMouseLook mouseLook;
	private Camera cam;
	private Vehicle vehicle;
	private Character player;
	private Objective objectif;
	private Vector3 vehicleScreenPosition;
	private Vector3 objectiveScreenPosition;
	private float lastFrameTime;
	private bool bHoldRecallPrompt = false;
	private bool bHintShowing = false;
	private bool bCursorInit = false;
	private IEnumerator spearTimeoutCoroutine;
	private IEnumerator abilityTimeoutCoroutine;

	void Start()
	{
		itemBar = GetComponentInChildren<ItemBar>();
		animator = spearAquirePanel.GetComponent<Animator>();
		spearAquirePanel.SetActive(false);
		info = GetComponent<EquippedInfo>();

		objectivePointer.SetActive(false);
		vehiclePointer.SetActive(false);
		recallPrompt.SetActive(true);
		lastFrameTime = Time.time;
		throwingChargePanel.SetActive(false);
		/// loadingPanel.SetActive(false); // move this to Menus
	}

	private void Update()
	{
		if (!bCursorInit)
		{
			Cursor.lockState = CursorLockMode.Locked;
			bCursorInit = true;
		}
	}

	public void SetToolInfo(string name, string value)
	{
		if (info != null)
		{
			info.SetToolName(name);
			info.SetToolReserve(value);
		}
	}

	public GameObject GetTool(int id)
	{
		GameObject result = null;
		if (itemBar.GetItem(id - 1) != null)
			result = itemBar.GetItem(id - 1);
		return result;
	}

	public void SetThrowingChargeActive(bool value)
	{
		throwingChargePanel.SetActive(value);
	}

	public void SetThrowingChargeValue(float value)
	{
		throwingChargeBar.GetComponent<RectTransform>().sizeDelta = new Vector3(10f, value * 20f, 1f);
	}

	public void SetVehiclePointerActive(Vehicle vh, bool value)
	{
		vehicle = vh;
		vehiclePointer.SetActive(value);
		if (value)
		{
			vehicleScreenPosition = WorldToScreen(vh.transform.position);
			vehiclePointer.transform.position = vehicleScreenPosition;
		}
	}

	public void SetRecallPromptActive(bool value)
	{
		if (recallPrompt.activeInHierarchy != value)
		{
			recallPrompt.SetActive(value);
			bHoldRecallPrompt = value;
		}
	}

	Vector3 WorldToScreen(Vector3 worldPosition)
	{
		Vector3 toWorldPos = (worldPosition - cam.transform.position);
		Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);

		// Special case if vehicle is more than 1000u away
		if (toWorldPos.magnitude >= 1000f)
		{
			float beyondRange = (toWorldPos.magnitude - 1000f) + 10f;
			Vector3 retroPosition = toWorldPos.normalized * -beyondRange;
			Vector3 clampedWorldPos = worldPosition + retroPosition;
			screenPos = cam.WorldToScreenPoint(clampedWorldPos);
		}

		// Behind camera case
		Vector3 toPointer = toWorldPos.normalized;
		float dotToPointer = Vector3.Dot(cam.transform.forward, toPointer);
		if (dotToPointer < 0f)
		{
			if (!recallPrompt.activeInHierarchy)
			{
				recallPrompt.SetActive(true);
			}

			if (screenPos.x < Screen.width / 2)
			{
				screenPos.x = Screen.width - 150f;
			}
			else
			{
				screenPos.x = 150f;
			}

			if (toPointer.y > 0)
			{
				screenPos.y = Screen.height - 150f;
			}
			else if (toPointer.y < 0)
			{
				screenPos.y = 150f;
			}
		}

		screenPos.x = Mathf.Clamp(screenPos.x, 150f, Screen.width - 150f);
		screenPos.y = Mathf.Clamp(screenPos.y, 300f, Screen.height - 150f);

		return screenPos;
	}

	public void PlayAnimation(string value)
	{
		if (value == "GetSpear")
		{
			spearAquirePanel.SetActive(true);
			spearAquirePanel.GetComponent<AudioSource>().Play();
			spearTimeoutCoroutine = TimeoutSpearAnim(1f, spearAquirePanel);
			StartCoroutine(spearTimeoutCoroutine);
		}

		animator.Play(value);
	}

	public void SetSpearScore(int value)
	{
		spearAquirePanel.SetActive(true);
		Text spearScoreText = spearAquirePanel.GetComponentInChildren<Text>();
		spearScoreText.text = "+" + value;
	}

	public void AbilityLevel(string abilityName, float value)
	{
		foreach (GameObject go in abilityAquirePool)
		{
			if (!go.activeInHierarchy)
			{
				go.GetComponentInChildren<Text>().text = abilityName + " +" + value;
				go.SetActive(true);
				abilityTimeoutCoroutine = TimeoutAbilityAnim(1f, go);
				StartCoroutine(abilityTimeoutCoroutine);
				break;
			}
		}
	}

	private IEnumerator TimeoutAbilityAnim(float value, GameObject target)
	{
		yield return new WaitForSeconds(value);
		target.SetActive(false);
	}

	private IEnumerator TimeoutSpearAnim(float value, GameObject target)
	{
		yield return new WaitForSeconds(value);
		target.SetActive(false);
	}
}
