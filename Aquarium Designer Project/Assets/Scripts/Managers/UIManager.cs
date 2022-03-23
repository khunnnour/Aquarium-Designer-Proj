using UnityEngine;

public class UIManager : MonoBehaviour
{
	public Transform tankSidePanelObj;

	private void Start()
	{
		ResizeUI();
	}

	private void ResizeUI()
	{
		tankSidePanelObj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width - tankSidePanelObj.GetComponent<RectTransform>().rect.height);
	}
}
