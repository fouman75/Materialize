﻿using UnityEngine;
using System.Collections;
using System.IO;
using FreeImageAPI;

public class TextureImportTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

		string PathToLoad = "test";

		FIBITMAP bitmap = FreeImage.LoadEx (PathToLoad);
		bool importSuccess = FreeImage.SaveEx (bitmap, Application.dataPath + "/tempImage.png", FREE_IMAGE_FORMAT.FIF_PNG);
        Debug.Log("Import Success: " + importSuccess);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
