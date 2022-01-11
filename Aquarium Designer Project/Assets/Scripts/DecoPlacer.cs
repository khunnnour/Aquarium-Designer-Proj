using UnityEngine;

public class DecoPlacer : MonoBehaviour
{
	public static DecoPlacer Instance;

	public GameObject moveHandleObj;

    private const string ASSET_PATH = "Prefabs/";

    private Camera _mainCam;
    private Ray _mouseRay;
    private RaycastHit _mouseRayHit;
    private Transform _clampObjTran;
    private GameObject _objToPlace;
    private GameObject _prefabToPlace;
    private bool _placing, _moving;
    private Vector3 _moveAxis, _shiftOff;

	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
        _mainCam = Camera.main;

        _clampObjTran = DesignerManager.Instance.decorObj.transform.GetChild(0);

        _placing = false;
        _moving = false;

        moveHandleObj.SetActive(false);

        _moveAxis = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // get world location of mouse pointer
        _mouseRay = _mainCam.ScreenPointToRay(Input.mousePosition);
        // cast ray
        Physics.Raycast(_mouseRay, out _mouseRayHit, 5f, ~LayerMask.GetMask("Glass", "Water"));

        // get world location of mouse pointer
        if (_placing)
            HandlePlacing();
        else
        {
            if (_moving)
                Shift();

            HandleMoving();
        }
    }

    private void Shift()
    {
        // get new position
        Vector3 newPos = FindClosestPointOnLine() + _shiftOff;

        Transform oT = _objToPlace.transform;
        // set the position of the object 
        oT.position = newPos;

        // adjust to be in tank
        oT.position += VerifyInTank();
        moveHandleObj.transform.position = oT.position;
    }

    // returns the offset to keep it fully in the tank, or a zero vector if no adjustment is needed
    Vector3 VerifyInTank()
    {
        // bounds of the object of interest
        Bounds objBounds = _objToPlace.gameObject.GetComponent<Renderer>().bounds;
        // bounds of substrate 
        Bounds intBounds = _clampObjTran.gameObject.GetComponent<Collider>().bounds;

        // see if inside glass
        // center diff for determining sign of adjustment
        Vector3 diff = objBounds.center - intBounds.center;
        // x-axis
        float x = intBounds.extents.x -
                  (Mathf.Abs(intBounds.center.x) + Mathf.Abs(objBounds.center.x) + objBounds.extents.x);
        if (x > 0) x = 0; // disregard if not out of bounds

        // z-axis
        float z = intBounds.extents.z -
                  (Mathf.Abs(intBounds.center.z) + Mathf.Abs(objBounds.center.z) + objBounds.extents.z);
        if (z > 0) z = 0; // disregard if not out of bounds

        // y-axis -- make sure bottom face of bounds is intersecting w substrate 
        float yMin = intBounds.center.y - intBounds.extents.y + 0.005f;
        float yMax = intBounds.center.y + intBounds.extents.y - 0.005f;
        float y = Mathf.Min(Mathf.Max(objBounds.center.y - objBounds.extents.y, yMin), yMax) + objBounds.extents.y -
                  objBounds.center.y;
        //if (y > 0) y = 0; // disregard if not out of bounds

        Vector3 backShift = new Vector3(
            Mathf.Sign(diff.x) * x,
            y,
            Mathf.Sign(diff.z) * z);
        //Mathf.Sign(objBounds.center.y - objBounds.extents.y - Mathf.Abs(intBounds.center.y)) * y

        //Debug.Log(y.ToString("F3"));
        //Debug.Log(y + " * " + Mathf.Sign(objBounds.center.y - objBounds.extents.y - Mathf.Abs(intBounds.center.y)));

        return backShift;
    }

    // find closest point on axis based on mouse position
    Vector3 FindClosestPointOnLine()
    {
        // find closest point on skew lines mouse ray and move axis 
        //   move axis: v1 = p1 + d1
        //   mouse ray: v2 = p2 + d2

        Vector3 p1 = _objToPlace.transform.position;

        // get perpendicular ray
        Vector3 n = Vector3.Cross(_moveAxis, _mouseRay.direction);
        //Vector3 n1 = Vector3.Cross(_moveAxis, n);
        Vector3 n2 = Vector3.Cross(_mouseRay.direction, n);

        // find closest point
        Vector3 c1 = p1 + _moveAxis * (
            Vector3.Dot(_mouseRay.origin - p1, n2) /
            Vector3.Dot(_moveAxis, n2));
        //Vector3 c2 = _mouseRay.origin + _mouseRay.direction * (
        //    Vector3.Dot(p1 - _mouseRay.origin, n1) /
        //    Vector3.Dot(_mouseRay.direction, n1));


        //Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * 2f);
        //Debug.DrawRay(p1, _moveAxis * 2f);
        //Debug.DrawLine(c1, c2, Color.red);

        return c1;
    }

    void HandleMoving()
    {
        // place on left click
        if (Input.GetMouseButtonDown(0))
        {
            // if there is already an object selected
            if (_objToPlace)
            {
                // if object clicked is a handle
                if (_mouseRayHit.transform.CompareTag("Handle"))
                {
                    _moving = true;
                    _moveAxis = _mouseRayHit.transform.GetComponent<Handle>()._axis;
                    // get shift offset using relative positions of init 'grab' location and current pos of handle
                    _shiftOff = _objToPlace.transform.position - FindClosestPointOnLine();
                }
                else // otherwise clear selection
                {
                    ClearMoving();
                }
            }
            else
            {
                // otherwise, see if clicked a valid object to move
                if (_mouseRayHit.transform && _mouseRayHit.transform.CompareTag("Decor"))
                {
                    //_moving = true;
                    _objToPlace = _mouseRayHit.transform.gameObject;
                    moveHandleObj.transform.position = _objToPlace.transform.position;
                    moveHandleObj.SetActive(true);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _moving = false;
        }
    }

    private void ClearMoving()
    {
        _moving = false;
        _moveAxis = Vector3.zero;
        _objToPlace = null;
        moveHandleObj.SetActive(false);
    }

    void HandlePlacing()
    {
        // place on left click
        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
            return;
        }

        // stop placing on right click
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(_objToPlace);
            _objToPlace = null;
            _placing = false;
            return;
        }

        // move object based on mouse position
        UpdateObject();
    }

    void UpdateObject()
    {
        Vector3 constraints = _clampObjTran.localScale * 0.5f;

        // location is where the raycast hit
        Vector3 newPos = _mouseRayHit.point;
		// clamp values to decor object
		newPos.x = Mathf.Min(constraints.x, Mathf.Max(-constraints.x, newPos.x));
		newPos.y = _clampObjTran.position.y + constraints.y + 0.01f;
		newPos.z = Mathf.Min(constraints.z, Mathf.Max(-constraints.z, newPos.z));

		//Debug.Log(newPos);

		// set object you are placing to that location
		_objToPlace.transform.position = newPos;
    }

    public void FindObjectToPlace(string objName)
    {
        // get prefab of object to spawn
        _prefabToPlace = Resources.Load<GameObject>(ASSET_PATH + objName);

        // start the placing if found a prefab
        if (_prefabToPlace)
            StartPlacing();
    }

    private void StartPlacing()
    {
        ClearMoving();

        _objToPlace = Instantiate(_prefabToPlace, DesignerManager.Instance.decorObj.transform);
        _objToPlace.layer = LayerMask.NameToLayer("Glass");
        _placing = true;
    }

    private void PlaceObject()
    {
        // put on regular layer
        _objToPlace.layer = LayerMask.NameToLayer("Default");
		_objToPlace.transform.position += VerifyInTank();
		// clear object to place
		_objToPlace = null;
        // clear placing
        _placing = false;
    }
}
