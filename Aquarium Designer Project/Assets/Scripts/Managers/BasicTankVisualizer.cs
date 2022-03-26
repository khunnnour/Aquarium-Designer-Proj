using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicTankVisualizer : MonoBehaviour
{
	public static BasicTankVisualizer Instance;

	[Header("Object References")] public GameObject tankObj;

	[Header("Tank Rotate Variables")]
	public Slider xSlider;
	public Vector2 xRotRange = Vector2.one * 20f;
	public Slider ySlider;
	public Vector2 yRotRange = Vector2.one * 20f;

	[Header("Tank Spec Fields")] public Dropdown materialField;
	public InputField glassThickField;
	public InputField tankDimX;
	public InputField tankDimY;
	public InputField tankDimZ;
	public Transform tankSidePanelObj;
	public Toggle tankPanelLayoutToggle;

	[Header("Text Output Fields")]
	public Text tankCapacityText;
	public Text tankSafetyFactor;
	public Text tankDeflection;
	public Text glassAreaText;
	public Text glassWeightText;

	//public LineRenderer[] lines;

	// parameters from user
	private float _glassThickness;
	private Vector3 _tankDimensions;
	private Vector3 _mouseLastPos;

	private SheetMaterial _constructionMat;

	public Vector3 TankDimensions() { return _tankDimensions; }

	private void Awake()
	{
		Application.targetFrameRate = -1;
		Instance = this;
	}

	private void Start()
	{
		_tankDimensions = PlayerPrefInterface.GetTankDimensions();
		tankDimX.text = (_tankDimensions.x / TankSpecCalculator.INCH_2_M).ToString();
		tankDimY.text = (_tankDimensions.y / TankSpecCalculator.INCH_2_M).ToString();
		tankDimZ.text = (_tankDimensions.z / TankSpecCalculator.INCH_2_M).ToString();

		_glassThickness = PlayerPrefInterface.GetGlassThickness() / TankSpecCalculator.INCH_2_M;
		glassThickField.text = _glassThickness.ToString();

		UpdateTank();
		UpdateTankRotation();
	}

	/*
	private void Update()
	{
		// Get input
		Vector3 currMousePos = Input.mousePosition;
		Vector3 diff = currMousePos - _mouseLastPos;

		// make [-1, 1]
		diff.x /= Screen.width;
		diff.y /= Screen.height;

		// if right mouse is pressed, then orbit
		if (Input.GetMouseButton(1))
		{
			diff *= 60f;

			tankObj.transform.Rotate(tankObj.transform.up,		-diff.x,	Space.World);
			tankObj.transform.Rotate(tankObj.transform.right,	 diff.y,	Space.World);
		}

		_mouseLastPos = currMousePos;
	}
	*/

	public void UpdateTankRotation()
	{
		float xRotate = xSlider.value * (xRotRange.y - xRotRange.x) + xRotRange.x;
		float yRotate = ySlider.value * (yRotRange.y - yRotRange.x) + yRotRange.x;
		Quaternion newRotate = Quaternion.Euler(xRotate, yRotate, 0);
		tankObj.transform.rotation = newRotate;
	}

	public void UpdateTank()
	{
		/* - get the new parameters - */
		// get thickness
		if (float.TryParse(glassThickField.text, out _glassThickness))
		{
			_glassThickness *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			_glassThickness = 0.25f * TankSpecCalculator.INCH_2_M;
			glassThickField.text = (0.25f).ToString("F2");
		}

		// get tank dimensions
		if (float.TryParse(tankDimX.text, out _tankDimensions.x))
		{
			_tankDimensions.x *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			_tankDimensions.x = 20.25f * TankSpecCalculator.INCH_2_M;
			tankDimX.text = (20.25f).ToString("F2");
		}

		if (float.TryParse(tankDimY.text, out _tankDimensions.y))
		{
			_tankDimensions.y *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			_tankDimensions.y = 12.5f * TankSpecCalculator.INCH_2_M;
			tankDimY.text = (12.5f).ToString("F2");
		}

		if (float.TryParse(tankDimZ.text, out _tankDimensions.z))
		{
			_tankDimensions.z *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			_tankDimensions.z = 10.50f * TankSpecCalculator.INCH_2_M;
			tankDimZ.text = (10.50f).ToString("F2");
		}

		/* - update the tank object - */
		// update the base --
		// set position
		tankObj.transform.GetChild(0).transform.localPosition = Vector3.down * 0.5f * _tankDimensions.y;
		// set scale
		tankObj.transform.GetChild(0).transform.localScale =
			new Vector3(_tankDimensions.x, _glassThickness, _tankDimensions.z);

		// update front/back sides
		Vector3 newpos = new Vector3(0, 0, 0.5f * (_tankDimensions.z - _glassThickness));
		tankObj.transform.GetChild(1).transform.localPosition = newpos;
		newpos.z *= -1;
		tankObj.transform.GetChild(2).transform.localPosition = newpos;

		Vector3 newscale = new Vector3(
			_tankDimensions.x,
			_tankDimensions.y - _glassThickness,
			_glassThickness
		);
		tankObj.transform.GetChild(1).transform.localScale = newscale;
		tankObj.transform.GetChild(2).transform.localScale = newscale;

		// update left/right sides
		newpos = new Vector3((_tankDimensions.x - _glassThickness) * 0.5f, 0, 0);
		tankObj.transform.GetChild(3).transform.localPosition = newpos;
		newpos.x *= -1;
		tankObj.transform.GetChild(4).transform.localPosition = newpos;

		newscale = new Vector3(
			_glassThickness,
			_tankDimensions.y - _glassThickness,
			_tankDimensions.z - _glassThickness * 2f
		);
		tankObj.transform.GetChild(3).transform.localScale = newscale;
		tankObj.transform.GetChild(4).transform.localScale = newscale;

		// now reposition the whole tank
		float dist = _tankDimensions.x / TankSpecCalculator.INCH_2_M * 0.033f + 0.25f;
		Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(0.475f * (Screen.width + tankSidePanelObj.GetComponent<RectTransform>().rect.height), 0.5f * Screen.height, dist));
		tankObj.transform.position = pos;


		// update the info text
		UpdateInfoText();
	}

	// updates the labels for the main panel labels
	private static readonly Color ColorBad = new Color(0.8902f, 0.0431f, 0.0431f);
	private static readonly Color ColorNorm = new Color(0.0784f, 0.0784f, 0.0784f);
	private static readonly Color ColorWarn = new Color(0.9216f, 0.5686f, 0.0196f);

	public void UpdateInfoText()
	{
		// calculate total internal capacity --
		// internal volume in m^3
		float intVol = (_tankDimensions.x - 2 * _glassThickness) *
					   (_tankDimensions.z - 2 * _glassThickness) *
					   (_tankDimensions.y - _glassThickness);
		// capacity in liters
		float literCap = intVol * 1000f;
		// capacity in gallons
		float gallonCap = literCap * 0.2641720524f;
		// update the text
		tankCapacityText.text = "~" + gallonCap.ToString("F1") + " gal (" + literCap.ToString("F1") + " L)";

		_constructionMat = (SheetMaterial)materialField.value;

		// -- update safety factor -- //
		float safeFactor = TankSpecCalculator.CalculateSafetyFactor(_tankDimensions, _glassThickness, _constructionMat);
		tankSafetyFactor.text = "~" + safeFactor.ToString("F2");
		// set text color based on value
		if (safeFactor < 2.5f) tankSafetyFactor.color = ColorBad;
		else if (safeFactor < 3.5f) tankSafetyFactor.color = ColorWarn;
		else tankSafetyFactor.color = ColorNorm;

		// update deflection --
		float defl = TankSpecCalculator.CalculateDeflection(_tankDimensions, _glassThickness, _constructionMat);
		tankDeflection.text = "~" + defl.ToString("F2") + " mm";
		// set text color based on value
		if (defl >= 3f) tankDeflection.color = ColorBad;
		else if (defl >= 1f) tankDeflection.color = ColorWarn;
		else tankDeflection.color = ColorNorm;


		// retrieve dimensions of sides
		TankSpecCalculator.CalculateSidePanelDimensions(_tankDimensions, _glassThickness, out Vector2 botP, out Vector2 frontP, out Vector2 sideP);


		float sA = (botP.x * botP.y + 2f * (frontP.x * frontP.y + sideP.x * sideP.y)); // calculate surface area
		glassAreaText.text = sA.ToString("F1") + " in2 (" + (sA * TankSpecCalculator.IN2_TO_M2).ToString("F1") + " m2)";

		float tankWeight = TankSpecCalculator.CalculateTankWeight(sA * TankSpecCalculator.IN2_TO_M2, _glassThickness, _constructionMat);
		glassWeightText.text = (tankWeight * TankSpecCalculator.KG_TO_LB).ToString("F1") + " lbs (" + tankWeight.ToString("F1") + " kg)";


		// -- update glass panel dimension UI -- //
		tankSidePanelObj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tankSidePanelObj.GetComponent<RectTransform>().rect.height);

		Vector2 sideRectSize = tankSidePanelObj.GetComponent<RectTransform>().rect.size;
		float sideRectAR = sideRectSize.x / sideRectSize.y;

		Vector2 tankSize = new Vector2(botP.x + 2f * frontP.y, botP.y + 2f * frontP.y);
		float tankAR = tankSize.x / tankSize.y;

		//Debug.Log(sideRectAR.ToString("F2")+ " | " + tankAR.ToString("F2"));

		const float PX_SPACING = 7.0f;
		float in_2_px_scale = 1 > tankAR ?
								(sideRectSize.y - 4f * PX_SPACING) / (botP.y + 2f * frontP.y) :
								(sideRectSize.x - 4f * PX_SPACING) / (botP.x + 2f * frontP.y);

		// update the bottom panel
		Vector2 botPxDim = botP * in_2_px_scale;
		tankSidePanelObj.GetChild(0).GetComponent<RectTransform>().sizeDelta = botPxDim;
		tankSidePanelObj.GetChild(0).GetChild(0).GetComponent<Text>().text = botP.x + " x " + botP.y;

		// update front panel(s) size
		Vector2 frontPanelSize = frontP * in_2_px_scale;
		tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().sizeDelta = frontPanelSize;
		tankSidePanelObj.GetChild(1).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;
		tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().sizeDelta = frontPanelSize;
		tankSidePanelObj.GetChild(2).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;

		// update side panel(s) size
		Vector2 sidePanelSize = new Vector2(sideP.y, sideP.x) * in_2_px_scale;
		tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().sizeDelta = sidePanelSize;
		tankSidePanelObj.GetChild(3).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;
		tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().sizeDelta = sidePanelSize;
		tankSidePanelObj.GetChild(4).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;

		// update the positions
		if (tankPanelLayoutToggle.isOn)
		{
			// place/scale sheet panel
			tankSidePanelObj.GetChild(5).gameObject.SetActive(true);

			Vector2 fullSheetSize = new Vector2(
				Mathf.Ceil(2f * botP.x > botP.x + 2f * frontP.y ? 2f * botP.x : botP.x + 2f * frontP.y),
				Mathf.Ceil(frontP.y + botP.y)
				) * in_2_px_scale;
			tankSidePanelObj.GetChild(5).GetComponent<RectTransform>().sizeDelta = fullSheetSize;
			tankSidePanelObj.GetChild(5).GetChild(0).GetComponent<Text>().text = fullSheetSize.x / in_2_px_scale + " x " + fullSheetSize.y / in_2_px_scale;


			// if panel is toggled then orient inside of rectangle
			// place the base panel position
			Vector2 basePanelPos = new Vector2(
				0.5f * (botPxDim.x - fullSheetSize.x),
				0.5f * (fullSheetSize.y - botPxDim.y) - frontPanelSize.y
				);
			tankSidePanelObj.GetChild(0).GetComponent<RectTransform>().anchoredPosition = basePanelPos;

			// place the front/back panels' positions
			Vector2 frontPanelPos = new Vector2(
				0.5f * (botPxDim.x - fullSheetSize.x),
				0.5f * (fullSheetSize.y - frontPanelSize.y)
				);
			tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().anchoredPosition = frontPanelPos;
			frontPanelPos.x = 0.5f * (3f * botPxDim.x - fullSheetSize.x);
			tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().anchoredPosition = frontPanelPos;

			// place the side panels' positions
			Vector2 sidePanelPos = new Vector2(
				0.5f * (frontPanelSize.y - fullSheetSize.x) + frontPanelSize.x,
				0.5f * (fullSheetSize.y - sidePanelSize.y) - sidePanelSize.x
				);
			tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().anchoredPosition = sidePanelPos;
			sidePanelPos.x = 0.5f * (3f * frontPanelSize.y - fullSheetSize.x) + frontPanelSize.x;
			tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().anchoredPosition = sidePanelPos;

		}
		else
		{
			// otherwise format like open box
			// place the base panel position
			tankSidePanelObj.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

			// place the front/back panels' positions
			Vector2 frontPanelPos = new Vector2(0f, (frontPanelSize.y + botPxDim.y) * 0.5f + PX_SPACING);
			tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().anchoredPosition = frontPanelPos;
			frontPanelPos *= -1;
			tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().anchoredPosition = frontPanelPos;

			// place the side panels' positions
			Vector2 sidePanelPos = new Vector2((sidePanelSize.x + botPxDim.x) * 0.5f + PX_SPACING, 0);
			tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().anchoredPosition = sidePanelPos;
			sidePanelPos *= -1;
			tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().anchoredPosition = sidePanelPos;

			// hide the sheet panel
			tankSidePanelObj.GetChild(5).gameObject.SetActive(false);
		}
	}

	public void TransitionToFullVisualizer()
	{
		PlayerPrefInterface.SetTankSpecs(_tankDimensions, _glassThickness);

		SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
	}
}
