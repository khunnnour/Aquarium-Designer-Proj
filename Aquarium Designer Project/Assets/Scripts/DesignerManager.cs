using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public struct Props
{
    public float density;
    public float porosity;
}

public class DesignerManager : MonoBehaviour
{
    public static DesignerManager Instance;

    [Header("Object References")] public GameObject tankObj;
    public GameObject decorObj;

    [Header("Tank Spec Fields")] public Toggle materialField;
    public InputField glassThickField;
    public InputField tankDimX;
    public InputField tankDimY;
    public InputField tankDimZ;
	public Transform tankSidePanelObj;

    [Header("Decoration Fields")] public InputField capThickField;
    public InputField waterOffsetField;
    public Dropdown substrateDropdown;


    [Header("Text Output Fields")] public Text tankCapacityText;
    public Text tankSafetyFactor;
    public Text tankDeflection;
    public Text substrateAmtText;
    public Text waterAmtText;

    // parameters from user
    private float _glassThickness;
    private float _capThickness = 0.03f;
    private float _waterOffset = 1f;
    private Vector3 _tankDimensions;

    // other stuff
    private Transform _camTran;
    private CameraController _camController;

    private Hashtable _substrates;

    public void SetSubstrates(Hashtable subs)
    {
        _substrates = subs;
    }

    private const float INCH_2_M = 0.0254f; // in/m
    private const float MAT_THICKNESS = (3f / 8f) * INCH_2_M; // m    

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
    }

    private void Start()
    {
        _camTran = Camera.main.transform;
        _camController = Camera.main.GetComponent<CameraController>();

        UpdateTank();
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

    void UpdateCamPos()
    {
        float newZoom = -0.489f * _tankDimensions.x + -0.562f;
        // Update Camera Position
        Vector3 camTargetPos = new Vector3(0,
            tankObj.transform.GetChild(4).position.y + 0.2f,
            newZoom
        );

        //Quaternion camTargetRot = Quaternion.Euler(15f, 0, 0);
        //StartCoroutine(MoveCamera(camTargetPos, camTargetRot));
        StartCoroutine(MoveCamera(camTargetPos));
    }

    // updates the labels for the main panel labels
    private static readonly Color ColorBad = new Color(0.9f, 0.1f, 0.0f);
    private static readonly Color ColorNorm = new Color(0.1f, 0.1f, 0.1f);
    private static readonly Color ColorWarn = new Color(1.0f, 0.7f, 0.0f);

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

        // -- update safety factor -- //
        float safeFactor = TankSpecCalculator.CalculateSafetyFactor(_tankDimensions, _glassThickness, materialField.isOn);
        tankSafetyFactor.text = "~" + safeFactor.ToString("F2");
        // set text color based on value
        if (safeFactor < 3.0f) tankSafetyFactor.color = ColorBad;
        else if (safeFactor <= 3.5f) tankSafetyFactor.color = ColorWarn;
        else tankSafetyFactor.color = ColorNorm;

        // update deflection --
        float defl = TankSpecCalculator.CalculateDeflection(_tankDimensions, _glassThickness, materialField.isOn);
        tankDeflection.text = "~" + defl.ToString("F2") + " mm";
        // set text color based on value
        if (defl >= 3f) tankDeflection.color = ColorBad;
        else if (defl >= 1f) tankDeflection.color = ColorWarn;
        else tankDeflection.color = ColorNorm;


		// -- update glass panel dimensions -- //
		Vector2 botP, frontP, sideP;
		TankSpecCalculator.CalculateSidePanelDimensions(_tankDimensions, _glassThickness, out botP, out frontP, out sideP);
		//Debug.Log(botP.ToString("F3") + " | " + frontP.ToString("F3") + " | " + sideP.ToString("F3"));

		float in_2_px_scale = 140f/(botP.y + 2f * frontP.y);

		// update the bottom panel
		Vector2 botPxDim = botP * in_2_px_scale;
		tankSidePanelObj.GetChild(0).GetComponent<RectTransform>().sizeDelta = botPxDim;
		tankSidePanelObj.GetChild(0).GetChild(0).GetComponent<Text>().text = botP.x + " x " + botP.y;

		// update front panel(s)
		Vector2 newSize = frontP * in_2_px_scale;
		Vector2 newpos = new Vector2(0f, (newSize.y + botPxDim.y) * 0.5f + 5f);
		tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(1).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(1).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;
		newpos *= -1;
		tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(2).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(2).GetChild(0).GetComponent<Text>().text = frontP.x + " x " + frontP.y;

		// update side panel(s)
		newSize = new Vector2(sideP.y, sideP.x) * in_2_px_scale;
		newpos = new Vector2((newSize.x + botPxDim.x) * 0.5f + 5f, 0);
		tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(3).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(3).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;
		newpos *= -1;
		tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().sizeDelta = newSize;
		tankSidePanelObj.GetChild(4).GetComponent<RectTransform>().anchoredPosition = newpos;
		tankSidePanelObj.GetChild(4).GetChild(0).GetComponent<Text>().text = sideP.x + " x " + sideP.y;



		// get substrate properties from hashtable
		Props p = (Props) _substrates[substrateDropdown.captionText.text];

        /* -  update substrate weight estimate - */
        // get volume of substrate
        float substrateVol = decorObj.transform.GetChild(0).localScale.x *
                             decorObj.transform.GetChild(0).localScale.y *
                             decorObj.transform.GetChild(0).localScale.z;
        substrateAmtText.text = "~" + (substrateVol * p.density).ToString("F1") + " lbs";

        /* - update water capacity estimate - */
        // internal volume in m^3 of water column
        float watVol = (_tankDimensions.x - 2 * _glassThickness) *
                       (_tankDimensions.z - 2 * _glassThickness) *
                       (_tankDimensions.y - _glassThickness - _waterOffset - _capThickness);
        // volume of water in substrate
        watVol += substrateVol * p.porosity;

        // capacity in liters
        float watVolL = watVol * 1000f;
        // capacity in gallons
        float watVolG = watVolL * 0.2641720524f;
        // update the text
        waterAmtText.text = "~" + watVolG.ToString("F1") + " gal (" + watVolL.ToString("F1") + " L)";
    }

    public void UpdateInternals()
    {
        // get thickness of cap substrate
        if (float.TryParse(capThickField.text, out _capThickness))
        {
            _capThickness *= INCH_2_M;
        }
        else
        {
            // if invalid then use a default value
            _capThickness = 1.5f * INCH_2_M;
            capThickField.text = (_capThickness / INCH_2_M).ToString("F1");
        }

        // get water offset
        if (float.TryParse(waterOffsetField.text, out _waterOffset))
        {
            _waterOffset *= INCH_2_M;
        }
        else
        {
            // if invalid then use a default value
            _waterOffset = 1f * INCH_2_M;
            waterOffsetField.text = (_waterOffset / INCH_2_M).ToString("F1");
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
        Vector2 newTScale = new Vector2(
            newScale.x * 1.5f,
            newScale.z * 1.5f);
        newMat.SetTextureScale("_BaseMap", newTScale);

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

    //IEnumerator MoveCamera(Vector3 tPos, Quaternion tRot)
    IEnumerator MoveCamera(Vector3 tPos)
    {
        const float timeToTarget = 0.3f;

        float t = 0;

        Vector3 sPos = _camTran.position;
        Quaternion sRot = _camTran.rotation;
        while (t <= timeToTarget)
        {
            t += Time.deltaTime;
            _camTran.position = Vector3.Slerp(sPos, tPos, t / timeToTarget);
            //_camTran.rotation = Quaternion.Slerp(sRot, tRot, t / timeToTarget);
            yield return new WaitForEndOfFrame();
        }
    }
}
