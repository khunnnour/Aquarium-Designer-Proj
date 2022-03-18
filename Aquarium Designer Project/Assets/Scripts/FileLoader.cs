using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileLoader : MonoBehaviour
{
    const string ASSET_ROOT = "Assets/Resources/";
    const string FILE_PATH = "Files/";
	const string THUMBS_PATH = "Textures/thumbs/";

	[Header("Substrates")] 
	public TextAsset substrateFile;
	public Dropdown substrateDropdown;

    [Header("Decorations")] 
    public TextAsset decorFile;
	public GameObject decorObjsPanel;
	public GameObject decorItemPrefab;

    private void Awake()
    {
		RetrieveSubstrates();
		RetrieveDecorations();
	}

	// retrieves the list of items to place
	void RetrieveSubstrates()
	{
		// get substrates
		//StreamReader subsFile = new StreamReader(ASSET_ROOT + FILE_PATH + substrateFileName);
		string[] lines = substrateFile.text.Split('\n');

		Hashtable substrates = new Hashtable();
		List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
		for (int i = 1; i < lines.Length; i++)
		{
			string[] elements = lines[i].Split(',');
			SubstrateProperties newProps = new SubstrateProperties
			{
				density = float.Parse(elements[1]), 
				porosity = float.Parse(elements[2])
			};
			substrates.Add(elements[0], newProps);
			options.Add(new Dropdown.OptionData(elements[0]));
		}

		// update ui
		substrateDropdown.options = options;
		// update designer manager
		DesignerManager.Instance.SetSubstrates(substrates);
	}


	// loads decorations and populates panel
	void RetrieveDecorations()
	{
		// get decorations
		string[] lines = decorFile.text.Split('\n');

		// for every decoration listed
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i].TrimEnd();

			// get thumbnail
			Sprite thumb = Resources.Load<Sprite>(THUMBS_PATH + "T_" + line);
			//Debug.Log(THUMBS_PATH + "T_" + line + " as a sprite");

			// create and an entry for it
			GameObject newItem = Instantiate<GameObject>(decorItemPrefab, decorObjsPanel.transform);
			// set the source image
			newItem.GetComponent<Image>().sprite = thumb;
			// set the button on click
			newItem.GetComponent<Button>().onClick.AddListener(delegate { DecoPlacer.Instance.FindObjectToPlace(line); });
			// set the text under the button
			newItem.transform.GetChild(0).GetComponent<Text>().text = line;

			// position correctly
			Vector2 pos = new Vector2(
				10 + (i % 3) * (65 + 35),
				-(50 + Mathf.Floor(i / 3) * (65 + 35))
				);
			newItem.GetComponent<RectTransform>().anchoredPosition = pos;
		}
	}
}
