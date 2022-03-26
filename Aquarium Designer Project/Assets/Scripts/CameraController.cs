using UnityEngine;

public class CameraController : MonoBehaviour
{
	private DesignerManager _designerManager;
	private Transform _clampObj;
	private Vector3 _targetLookPos;

	private Vector3 _lastMousePos;

	private float _currZoom = 1f; // [0,1]
	private float _minZoom = 0.5f;
	private float _maxZoom = 3.2f;

	// Start is called before the first frame update
	void Start()
	{
		// get the manager
		_designerManager = GameObject.FindGameObjectWithTag("DesignerManager").GetComponent<DesignerManager>();
		// get object controller is using to clamp
		_clampObj = _designerManager.decorObj.transform.GetChild(0);
		// set initial target look location
		_targetLookPos = _clampObj.position + Vector3.up * _clampObj.localScale.y;

		_lastMousePos = Input.mousePosition;

		PlaceCamera();
	}

	// Update is called once per frame
	void Update()
	{
		// Get input
		Vector3 currMousePos = Input.mousePosition;
		Vector3 diff = currMousePos - _lastMousePos;

		// make [-1, 1]
		diff.x /= Screen.width;
		diff.y /= Screen.height;

		// if right mouse is pressed, then orbit
		if (Input.GetMouseButton(1))
		{
			// rotate camera
			diff *= 90f;
			Vector3 newEuler = new Vector3(
				Mathf.Min(88f, Mathf.Max(2f, transform.rotation.eulerAngles.x + diff.y)),
				transform.rotation.eulerAngles.y - diff.x,
				0);
			transform.rotation = Quaternion.Euler(newEuler);
		}

		// update translation
		TranslateTarget();

		// update camera placement
		PlaceCamera();

		// update last mouse position
		_lastMousePos = currMousePos;

		// zoom based on mouse scroll
		float mSDelta = Input.mouseScrollDelta.y;
		if (mSDelta != 0f)
		{
			_currZoom = Mathf.Min(1f, Mathf.Max(0f, _currZoom - mSDelta * 0.05f));
			PlaceCamera();
		}
	}

	void TranslateTarget()
	{
		const float offset = 0.05f;

		// adjust remove vertical from for vector (when looking down, y is irrelevant)
		Vector3 adjForward = transform.forward;
		adjForward.y = 0;
		adjForward.Normalize();

		Vector3 shift = Input.GetAxis("Horizontal") * transform.right
						  + Input.GetAxis("Vertical") * adjForward;
		shift = shift.normalized * 0.005f;

		//Debug.Log(shift.ToString("F3"));

		Vector3 constraints = _clampObj.localScale * 0.5f;

		//Vector3 newPos = new Vector3(
		//    Mathf.Min(constraints.x - offset, Mathf.Max(offset - constraints.x, _targetLookPos.x + shift.x)),
		//    _clampObj.position.y + constraints.y + offset,
		//    Mathf.Min(constraints.z - offset, Mathf.Max(offset - constraints.z, _targetLookPos.z + shift.y))
		//);
		// shift position
		Vector3 newPos = _targetLookPos + shift;
		// clamp values
		newPos.x = Mathf.Min(constraints.x - offset, Mathf.Max(offset - constraints.x, newPos.x));
		newPos.y = _clampObj.position.y + constraints.y + offset;
		newPos.z = Mathf.Min(constraints.z - offset, Mathf.Max(offset - constraints.z, newPos.z));

		_targetLookPos = newPos;

		PlaceCamera();
	}

	// translate camera based on rotation/zoom/target look
	public void PlaceCamera()
	{
		// get look direction
		Vector3 lookDir = transform.rotation * Vector3.forward;

		// calculate zoom in world units
		float zoom = _minZoom + (_maxZoom - _minZoom) * _currZoom;

		// update position
		transform.position = _targetLookPos - lookDir * zoom;
	}
}
