using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct SubstrateProperties
{
	public float density;
	public float porosity;
}

public class DesignerManager : MonoBehaviour
{
	public static DesignerManager Instance;

	[Header("Object References")] public GameObject tankObj;
	public GameObject decorObj;

	[Header("Decoration Fields")] public InputField capThickField;
	public InputField waterOffsetField;
	public Text waterAmtText;
	public Dropdown substrateDropdown;
	public Text substrateAmtText;

	// parameters from user
	private float _glassThickness;
	private float _capThickness = 0.03f;
	private float _waterOffset = 1f;
	private Vector3 _tankDimensions;

	// other stuff
	private CameraController _camController;
	private Hashtable _substrates;


	public void SetSubstrates(Hashtable subs)
	{
		_substrates = subs;
	}

	private const float MAT_THICKNESS = (3f / 8f) * TankSpecCalculator.INCH_2_M;

	private void Awake()
	{
		Application.targetFrameRate = -1;
		Instance = this;
	}

	private void Start()
	{
		_camController = Camera.main.GetComponent<CameraController>();

		_tankDimensions = PlayerPrefInterface.GetTankDimensions();
		_glassThickness = PlayerPrefInterface.GetGlassThickness();

		_capThickness = PlayerPrefInterface.GetSubstrateThickness() / TankSpecCalculator.INCH_2_M;
		capThickField.text = _capThickness.ToString();
		
		_waterOffset = PlayerPrefInterface.GetWaterOffset() / TankSpecCalculator.INCH_2_M;
		waterOffsetField.text = _waterOffset.ToString();


		UpdateTank();
	}

	private void UpdateTank()
	{
		/* - update the tank object - */
		// neoprene mat --
		// set position
		Vector3 matVertOffset = Vector3.up * MAT_THICKNESS;
		tankObj.transform.GetChild(0).transform.localPosition = matVertOffset * 0.5f;
		// set scale
		tankObj.transform.GetChild(0).transform.localScale =
			new Vector3(_tankDimensions.x, MAT_THICKNESS, _tankDimensions.z);
		// update the base --
		// set position
		tankObj.transform.GetChild(1).transform.localPosition = Vector3.up * _glassThickness * 0.5f + matVertOffset;
		// set scale
		tankObj.transform.GetChild(1).transform.localScale =
			new Vector3(_tankDimensions.x, _glassThickness, _tankDimensions.z);

		// update front/back sides
		Vector3 newpos = new Vector3(0, (_tankDimensions.y + _glassThickness) * 0.5f,
			(_tankDimensions.z - _glassThickness) * 0.5f) + matVertOffset;
		tankObj.transform.GetChild(2).transform.localPosition = newpos;
		newpos.z *= -1;
		tankObj.transform.GetChild(3).transform.localPosition = newpos;

		Vector3 newscale = new Vector3(
			_tankDimensions.x,
			_tankDimensions.y - _glassThickness,
			_glassThickness
		);
		tankObj.transform.GetChild(2).transform.localScale = newscale;
		tankObj.transform.GetChild(3).transform.localScale = newscale;

		// update left/right sides
		newpos = new Vector3((_tankDimensions.x - _glassThickness) * 0.5f, (_tankDimensions.y + _glassThickness) * 0.5f,
			0) + matVertOffset;
		tankObj.transform.GetChild(4).transform.localPosition = newpos;
		newpos.x *= -1;
		tankObj.transform.GetChild(5).transform.localPosition = newpos;

		newscale = new Vector3(
			_glassThickness,
			_tankDimensions.y - _glassThickness,
			_tankDimensions.z - _glassThickness * 2f
		);
		tankObj.transform.GetChild(4).transform.localScale = newscale;
		tankObj.transform.GetChild(5).transform.localScale = newscale;

		// update the decorations accordingly
		UpdateInternals();

		// updates camera distance
		//UpdateCamPos();
		_camController.PlaceCamera();
	}

	public void UpdateInfoText()
	{
		// get substrate properties from hashtable
		SubstrateProperties p = (SubstrateProperties)_substrates[substrateDropdown.captionText.text];

		/* -  update substrate weight estimate - */
		// get volume of substrate
		float substrateVol = decorObj.transform.GetChild(0).localScale.x *
							 decorObj.transform.GetChild(0).localScale.y *
							 decorObj.transform.GetChild(0).localScale.z;
		// get weight of substrate
		float substrateWeight = substrateVol * p.density;
		// update substrate text
		substrateAmtText.text = "~" + substrateWeight.ToString("F1") + " lbs (" + (substrateWeight / TankSpecCalculator.KG_TO_LB).ToString("F1") + " L)";

		/* - update water capacity estimate - */
		// internal volume in m^3 of water column
		float watVol = (_tankDimensions.x - 2 * _glassThickness) *
					   (_tankDimensions.z - 2 * _glassThickness) *
					   (_tankDimensions.y - _glassThickness - _waterOffset - _capThickness);
		// volume of water w/in substrate
		watVol += substrateVol * p.porosity;

		// capacity in liters
		float watVolL = watVol * TankSpecCalculator.WATER_DENSITY;
		// capacity in gallons
		float watVolG = watVolL * TankSpecCalculator.L_TO_GAL;
		// update the text
		waterAmtText.text = "~" + watVolG.ToString("F1") + " gal (" + watVolL.ToString("F1") + " L)";
	}

	public void UpdateInternals()
	{
		// get thickness of cap substrate
		if (float.TryParse(capThickField.text, out _capThickness))
		{
			_capThickness *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			// if invalid then use a default value
			_capThickness = 1.5f * TankSpecCalculator.INCH_2_M;
			capThickField.text = (_capThickness / TankSpecCalculator.INCH_2_M).ToString("F1");
		}

		// get water offset
		if (float.TryParse(waterOffsetField.text, out _waterOffset))
		{
			_waterOffset *= TankSpecCalculator.INCH_2_M;
		}
		else
		{
			// if invalid then use a default value
			_waterOffset = 1f * TankSpecCalculator.INCH_2_M;
			waterOffsetField.text = (_waterOffset / TankSpecCalculator.INCH_2_M).ToString("F1");
		}

		// update cap substrate dimensions
		decorObj.transform.GetChild(0).localPosition =
			Vector3.up * (MAT_THICKNESS + _glassThickness + _capThickness * 0.5f);
		Vector3 oldScale = decorObj.transform.GetChild(0).localScale;
		Vector3 newScale = new Vector3(
			_tankDimensions.x - _glassThickness * 2f,
			_capThickness,
			_tankDimensions.z - _glassThickness * 2f);
		decorObj.transform.GetChild(0).localScale = newScale;
		// update material
		string path = "Materials/M_" + substrateDropdown.captionText.text;
		//Debug.Log(path);
		Material newMat = Resources.Load<Material>(path);
		decorObj.transform.GetChild(0).GetComponent<MeshRenderer>().material = newMat;

		// update water
		float waterDepth = _tankDimensions.y - _waterOffset - _capThickness - _glassThickness;
		decorObj.transform.GetChild(1).localPosition =
			Vector3.up * (MAT_THICKNESS + _glassThickness + _capThickness + waterDepth * 0.5f);
		decorObj.transform.GetChild(1).localScale = new Vector3(
			_tankDimensions.x - _glassThickness * 2f,
			waterDepth,
			_tankDimensions.z - _glassThickness * 2f);

		// update other internal decorations
		for (int i = 2; i < decorObj.transform.childCount; i++)
			decorObj.transform.GetChild(i).position = new Vector3(
				decorObj.transform.GetChild(i).position.x / oldScale.x * newScale.x,
				decorObj.transform.GetChild(0).position.y + newScale.y * 0.5f + 0.01f,
				decorObj.transform.GetChild(i).position.z / oldScale.z * newScale.z
			);

		// update the info text
		UpdateInfoText();
	}

	public void TransitionToBasicVisualizer()
	{
		PlayerPrefInterface.SetInsideSpecs(_capThickness, _waterOffset);

		SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
	}
}
