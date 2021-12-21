using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoPlacer : MonoBehaviour
{
    private const string ASSET_PATH = "Prefabs/";

    private Camera _mainCam;
    private Transform _clampObj;
    private GameObject _objToPlace;
    private GameObject _prefabToPlace;
    private bool _placing;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = Camera.main;

        _clampObj = DesignerManager.Instance.decorObj.transform.GetChild(0);

        _placing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_placing)
            GetInput();
    }

    void GetInput()
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
        // get world location of mouse pointer
        Ray mouseRay = _mainCam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(mouseRay, out RaycastHit hit, 5f,~LayerMask.GetMask("Glass","Water"));
        
        Debug.DrawRay(_mainCam.transform.position, mouseRay.direction * 5f);
        Debug.DrawLine(hit.point, hit.point + hit.normal * 2);

        Vector3 constraints = _clampObj.localScale * 0.5f;

        // location is where the raycast hit
        Vector3 newPos = hit.point;
        // clamp values to decor object
        newPos.x = Mathf.Min(constraints.x, Mathf.Max(-constraints.x, newPos.x));
        newPos.y = _clampObj.position.y + constraints.y + 0.01f;
        newPos.z = Mathf.Min(constraints.z, Mathf.Max(-constraints.z, newPos.z));

        //Debug.Log(newPos);
        
        // set object you are placing to that location
        _objToPlace.transform.position = newPos;
    }

    public void FindObjectToPlace(string objName)
    {
        // get prefab of object to spawn
        _prefabToPlace = Resources.Load<GameObject>(ASSET_PATH + objName);

        // start the placing
        StartPlacing();
    }

    private void StartPlacing()
    {
        _objToPlace = Instantiate(_prefabToPlace, DesignerManager.Instance.decorObj.transform);
        _objToPlace.layer = LayerMask.NameToLayer("Glass");
        _placing = true;
    }

    private void PlaceObject()
    {
        // put on regular layer
        _objToPlace.layer = LayerMask.NameToLayer("Default");
        // clear object to place
        _objToPlace = null;
        // clear placing
        _placing = false;
    }
}
