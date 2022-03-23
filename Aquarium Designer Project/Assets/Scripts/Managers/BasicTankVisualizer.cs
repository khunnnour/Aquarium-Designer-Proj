using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicTankVisualizer : MonoBehaviour
{
	public static BasicTankVisualizer Instance;

	[Header("Object References")] public GameObject tankObj;

	[Header("Object References")]
	Vector3 rotRange = Vector3.one * 20f;

	[Header("Tank Spec Fields")] public Dropdown materialField;
	public InputField glassThickField;
	public InputField tankDimX;
	public InputField tankDimY;
	public InputField tankDimZ;
	public Transform tankSidePanelObj;

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

	private const float INCH_2_M = 0.0254f; // in/m
	private const float MAT_THICKNESS = (3f / 8f) * INCH_2_M; // m    

	public Vector3 TankDimensions() { return _tankDimensions; }

	private void Awake()
	{
		Application.targetFrameRate = -1;
		Instance = this;
	}

	private void Start()
	{
		UpdateTank();
	}

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

			Debug.Log("rotating " + diff);

			tankObj.transform.Rotate(Vector3.up, -diff.x, Space.World);
			tankObj.transform.Rotate(Vector3.right, diff.y, Space.World);
			//
			//// clamp angles
			//Vector3 newEuler = tankObj.transform.rotation.eulerAngles;
			//newEuler.x = Mathf.Max(Mathf.Min(newEuler.x, _origEulers.x + rotRange.x), _origEulers.x - rotRange.x);
			//newEuler.y = Mathf.Max(Mathf.Min(newEuler.y, _origEulers.y + rotRange.y), _origEulers.y - rotRange.y);
			//newEuler.z = Mathf.Max(Mathf.Min(newEuler.z, _origEulers.z + rotRange.z), _origEulers.z - rotRange.z);
			//
			//tankObj.transform.rotation = Quaternion.Euler(newEuler);
		}

		_mouseLastPos = currMousePos;
	}

	public void UpdateTank()
	{
		/* - get the new parameters - */
		// get thickness
		if (float.TryParse(glassThickField.text, out _glassThickness))
		{
			_glassThickness *= INCH_2_M;
		}
		else
		{
			_glassThickness = 0.25f * INCH_2_M;
			glassThickField.text = (0.25f).ToString("F2");
		}

		// get tank dimensions
		if (float.TryParse(tankDimX.text, out _tankDimensions.x))
		{
			_tankDimensions.x *= INCH_2_M;
		}
		else
		{
			_tankDimensions.x = 20.25f * INCH_2_M;
			tankDimX.text = (20.25f).ToString("F2");
		}

		if (float.TryParse(tankDimY.text, out _tankDimensions.y))
		{
			_tankDimensions.y *= INCH_2_M;
		}
		else
		{
			_tankDimensions.y = 12.5f * INCH_2_M;
			tankDimY.text = (12.5f).ToString("F2");
		}

		if (float.TryParse(tankDimZ.text, out _tankDimensions.z))
		{
			_tankDimensions.z *= INCH_2_M;
		}
		else
		{
			_tankDimensions.z = 10.50f * INCH_2_M;
			tankDimZ.text = (10.50f).ToString("F2");
		}

		/* - update the tank object - */
		// update the base --
		// set position
		tankObj.transform.GetChild(0).transform.localPosition = Vector3.down * 0.5f * (_tankDimensions.y + _glassThickness);
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
		float dist = _tankDimensions.x / INCH_2_M * 0.033f + 0.25f;
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
		if (safeFactor < 3.0f) tankSafetyFactor.color = ColorBad;
		else if (safeFactor <= 3.5f) tankSafetyFactor.color = ColorWarn;
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

		// update front panel(s)
		Vector2 newSize = frontP * in_2_px_scale;
		Vector2 newpos = new Vector2(0f, (newSize.y + botPxDim.y) * 0.5f + PX_SPACING);
		tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(1).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;
		newpos *= -1;
		tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(2).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;

		// update side panel(s)
		newSize = new Vector2(sideP.y, sideP.x) * in_2_px_scale;
		newpos = new Vector2((newSize.x + botPxDim.x) * 0.5f + PX_SPACING, 0);
		tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(3).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;
		newpos *= -1;
		tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(4).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;
	}

	public void TransitionToFullVisualizer()
	{
		PlayerPrefInterface.SetTankSpecs(_tankDimensions, _glassThickness);

		SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
	}
}
