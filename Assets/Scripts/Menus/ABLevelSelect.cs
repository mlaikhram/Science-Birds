// SCIENCE BIRDS: A clone version of the Angry Birds game used for
// research purposes
//
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class ABLevelSelect : ABMenu {

	public int _lines = 5;

	public GameObject _levelSelector;
    public GameObject _ratingStar;
	public GameObject _canvas;

	public Vector2 _startPos;
	public Vector2 _buttonSize;

	private int _clickedButton;

    public Camera _camera;

    public GameObject lSystemButtons;
    public GameObject levelButtons;
    private List<GameObject> tempLevelButtons;


    public void InitializeLSystems()
    {
        /* TODO: 
         * populate RatingSystem.lSystems with 6 LSystems
         */
        SqlManager.SqlManagerInstance.StartCoroutine(SqlConnection.GetPopulation());
        //  Initialize 6 randomized LSystems, 3 rules each, 
        //      max size of successor being 5.
        for (int i = 0; i < RatingSystem.MAX_LSYSTEMS; i++) {
            RatingSystem.lSystems.Add(new LSystem(3, 5));
            RatingSystem.GenerateXMLs(i, 5);
        }

        //for (int i = 0; i < RatingSystem.lSystems.Count; ++i)
        //{
        //    RatingSystem.GenerateXMLs(i, 5); // hardcoded height
        //}
    }

    public void LoadScreenshots(int lSystemIndex)
    {
        RatingSystem.StartGeneratingScreenshots(lSystemIndex);
        ABLevelSelector sel = gameObject.AddComponent<ABLevelSelector>();
        sel.LevelIndex = lSystemIndex * RatingSystem.MAX_LEVELS;

        LoadNextScene("GameWorld", true, sel.UpdateLevelList);
    }

    //public void GenerateXML(int lSystemIndex, string filename)
    //{
    //    /* TODO: 
    //     * Generate Level from L System into the resources/levels directory (should be a constant here somewhere...)
    //     * Set filemode to overwrite the file if it already exists
    //     */

    //    //filename = Path.Combine(ABConstants.DEFAULT_LEVELS_FOLDER, filename);
    //    //  Generates a structure of height 5
    //    RatingSystem.lSystems[lSystemIndex].GenerateXMLs(5);
    //}

    //public void GenerateNewLevels(int lSystemIndex)
    //{
    //    for (int i = 0; i < RatingSystem.MAX_LEVELS; ++i)
    //    {
    //        GenerateXML(lSystemIndex, String.Format("level-{0:D2}.xml", lSystemIndex * RatingSystem.MAX_LEVELS + i));
    //    }
    //}

    public void SubmitRatings()
    {
        RatingSystem.SubmitRatings();
        RatingSystem.ClearAll();
        ABSceneManager.Instance.LoadScene("LevelSelectMenu");
        //RatingSystem.GenerateXMLs(RatingSystem.CurrentLSystemIndex, 5); // hardcoded height
        //LoadScreenshots(RatingSystem.CurrentLSystemIndex);
        //GenerateNewLevels(RatingSystem.CurrentLSystemIndex);
    }

    // deprecated
    public string[] loadXMLsOld()
    {
        // Load levels in the resources folder
        TextAsset[] levelsData = Resources.LoadAll<TextAsset>(ABConstants.DEFAULT_LEVELS_FOLDER);

        string[] resourcesXml = new string[levelsData.Length];
        for (int i = 0; i < levelsData.Length; i++)
            resourcesXml[i] = levelsData[i].text;


#if UNITY_WEBGL && !UNITY_EDITOR

		// WebGL builds does not load local files
		string[] streamingXml = new string[0];

#else
        // Load levels in the streaming folder
        string levelsPath = Application.dataPath + ABConstants.CUSTOM_LEVELS_FOLDER;
        string[] levelFiles = Directory.GetFiles(levelsPath, "*.xml");

        string[] streamingXml = new string[levelFiles.Length];
        for (int i = 0; i < levelFiles.Length; i++)
            streamingXml[i] = File.ReadAllText(levelFiles[i]);

#endif

        // Combine the two sources of levels
        string[] allXmlFiles = new string[resourcesXml.Length + streamingXml.Length];
        resourcesXml.CopyTo(allXmlFiles, 0);
        streamingXml.CopyTo(allXmlFiles, resourcesXml.Length);

        _startPos.x = Mathf.Clamp(_startPos.x, 0, 1f) * Screen.width;
        _startPos.y = Mathf.Clamp(_startPos.y, 0, 1f) * Screen.height;

        LevelList.Instance.LoadLevelsFromSource(allXmlFiles);

        return allXmlFiles;
    }

    public string[] loadXMLs()
    {
        string[] allXmls = new string[RatingSystem.MAX_LEVELS * RatingSystem.MAX_LSYSTEMS];
        //Debug.Log("TOTAL: " + (RatingSystem.MAX_LEVELS * RatingSystem.MAX_LSYSTEMS));

        for (int i = 0; i < RatingSystem.MAX_LSYSTEMS; ++i)
        {
            for (int j = 0; j < RatingSystem.MAX_LEVELS; ++j)
            {
                //Debug.Log(i * RatingSystem.MAX_LEVELS + j);
                allXmls[i * RatingSystem.MAX_LEVELS + j] = RatingSystem.levelData[i][j].levelXML;
            }
        }

        _startPos.x = Mathf.Clamp(_startPos.x, 0, 1f) * Screen.width;
        _startPos.y = Mathf.Clamp(_startPos.y, 0, 1f) * Screen.height;

        LevelList.Instance.LoadLevelsFromSource(allXmls);

        foreach (string xml in allXmls)
        {
            Debug.Log(xml);
        }

        return allXmls;
    }

    // deprecated
    public void DisplayLSystems()
    {
        lSystemButtons.SetActive(true);
        levelButtons.SetActive(false);

        foreach (GameObject obj in tempLevelButtons)
        {
            Destroy(obj);
        }
    }

    // deprecated
    public void DisplayLevels(int lSystemIndex)
    {
        if (RatingSystem.levelData[lSystemIndex][0].levelSprite == null)
        {
            LoadScreenshots(lSystemIndex);
        }
        else
        {
            lSystemButtons.SetActive(false);
            levelButtons.SetActive(true);

            RatingSystem.CurrentLSystemIndex = lSystemIndex;
            int j = 0;
            RatingSystem.EndGeneratingScreenshots();
            for (int i = 0; i < RatingSystem.MAX_LEVELS; i++)
            {

                GameObject obj = Instantiate(_levelSelector, Vector2.zero, Quaternion.identity) as GameObject;
                obj.GetComponent<Image>().sprite = RatingSystem.levelData[RatingSystem.CurrentLSystemIndex][i].levelSprite;

                obj.transform.SetParent(_canvas.transform, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = new Vector3(0.16f, 0.16f, 1f);

                //Vector2 pos = _startPos + new Vector2((i % _lines) * _buttonSize.x, j * _buttonSize.y);

                Vector2 pos = _startPos + new Vector2((i % _lines) * (_camera.scaledPixelWidth / 3.1f), j * (_camera.scaledPixelHeight / 3.1f));
                obj.transform.position = pos;

                //Debug.Log(obj.transform.position);

                ABLevelSelector sel = obj.AddComponent<ABLevelSelector>();
                sel.LevelIndex = lSystemIndex * RatingSystem.MAX_LEVELS + i;

                Button selectButton = obj.GetComponent<Button>();

                selectButton.onClick.AddListener(delegate
                {
                    LoadNextScene("GameWorld", true, sel.UpdateLevelList);
                });

                Text selectText = selectButton.GetComponentInChildren<Text>();
                selectText.text = "";// + (i + 1);

                // create rating button
                GameObject star = Instantiate(_ratingStar, Vector2.zero, Quaternion.identity) as GameObject;
                star.transform.SetParent(_canvas.transform, false);
                star.transform.localPosition = Vector3.zero;
                star.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                star.transform.position = pos + new Vector2(-_camera.scaledPixelWidth / 8f, _camera.scaledPixelHeight / 10f);

                if (RatingSystem.levelData[RatingSystem.CurrentLSystemIndex][i].pressedButton)
                {
                    star.GetComponent<Image>().color = Color.yellow;
                }
                else
                {
                    star.GetComponent<Image>().color = Color.black;
                }
                star.GetComponent<Button>().onClick.AddListener(delegate
                {
                    RatingSystem.RateLevel(sel.LevelIndex, star);
                //LoadNextScene("GameWorld", true, sel.UpdateLevelList);
            });

                tempLevelButtons.Add(obj);
                tempLevelButtons.Add(star);

                if ((i + 1) % _lines == 0)
                    j--;
            }
        }
    }

    // Use this for initialization
    void Start () {

        tempLevelButtons = new List<GameObject>();

        if (RatingSystem.lSystems.Count <= 0)
        {
            Debug.Log("Initializing LSystems...");
            InitializeLSystems();
            loadXMLs();
        }

        for (int i = 0; i < RatingSystem.levelData.Count; ++i)
        {
            if (RatingSystem.levelData[i][0].levelSprite == null)
            {
                //Debug.Log("LSystem " + i + " does not have screenshots generated");
                LoadScreenshots(i);
                goto Finished;
            }
        }
        RatingSystem.EndGeneratingScreenshots();
        //Debug.Log("Done generating all screenshots");

        //loadXMLs();

        //if (RatingSystem.CurrentLSystemIndex >= 0)
        //{
        //    DisplayLevels(RatingSystem.CurrentLSystemIndex);
        //}
        Finished:
        Debug.Log("Done with LevelSelect.Start");
    }
}
