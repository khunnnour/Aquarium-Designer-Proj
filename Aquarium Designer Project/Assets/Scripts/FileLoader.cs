using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class FileLoader : MonoBehaviour
{
    const string ASSET_ROOT = "Assets/Resources/";
    const string FILE_PATH = "Files/";

    [Header("Files")] public TextAsset substrateFile;

    [Header("UI Elements")] public Dropdown substrateDropdown;


    private void Awake()
    {
        // get substrates
        //StreamReader subsFile = new StreamReader(ASSET_ROOT + FILE_PATH + substrateFileName);
        string[] lines = substrateFile.text.Split('\n');

        Hashtable substrates = new Hashtable();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] elements = lines[i].Split(',');
            substrates.Add(elements[0], float.Parse(elements[1]));
            options.Add(new Dropdown.OptionData(elements[0]));
        }

        //subsFile.ReadLine(); // discard first line
        // retrieve data
        //while (!subsFile.EndOfStream)
        //{
        //    string line = subsFile.ReadLine();
        //    string[] elements = line.Split(',');
        //    substrates.Add(elements[0], float.Parse(elements[1]));
        //    options.Add(new Dropdown.OptionData(elements[0]));
        //}
        // update ui
        substrateDropdown.options = options;
        // update designer manager
        DesignerManager.Instance.SetSubstrates(substrates);
    }
}
