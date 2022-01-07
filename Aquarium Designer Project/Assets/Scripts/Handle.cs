using UnityEngine;

public class Handle : MonoBehaviour
{
	public enum Axis
	{
		X_AXIS,
		Y_AXIS,
		Z_AXIS
	}

	public Axis axis;

	public Vector3 _axis
	{
		get;
		private set;
	}

	// Start is called before the first frame update
	void Start()
	{
		MeshRenderer mRenderer = GetComponent<MeshRenderer>();
		// choose color based on axis
		switch (axis)
		{
			case Axis.X_AXIS:
				mRenderer.material.SetColor("Tint", Color.red);
				_axis = Vector3.right;
				transform.rotation = Quaternion.Euler(Vector3.up * 90f);
				break;
			case Axis.Y_AXIS:
				mRenderer.material.SetColor("Tint", Color.green);
				_axis = Vector3.up;
				transform.rotation = Quaternion.Euler(Vector3.right * -90f);
				break;
			case Axis.Z_AXIS:
				mRenderer.material.SetColor("Tint", Color.blue);
				_axis = -Vector3.forward;
				break;
		}
	}
}
