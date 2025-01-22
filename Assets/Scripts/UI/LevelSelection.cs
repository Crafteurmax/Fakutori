using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[Serializable]
public class Level
{
    public string name;
    public string description;
    public string map;
    public string goal;
    public string dialogue;
}

[Serializable]
public class Panel
{
    public string name;
    public string description;
    public List<Level> levelList;
}

[Serializable]
public class PanelWrapper
{
    public List<Panel> panelList;
}


public class LevelSelection : MonoBehaviour
{
    [SerializeField] private ToggleButtonGroup levelToggleGroup;

    [Header("Level Description")]
    [SerializeField] private TMP_Text levelDescription;

    [Header("Navigation Buttons")]
    [SerializeField] private Button playButton;

    [Header("Panels")]
    [SerializeField] private List<LevelPanelController> panelObjectList;

    private int maxPanelNumber = 1;
    private int selectedPanel = 0;
    private string levelsDataPath = Application.dataPath + "/Resources/levelPanelData.json";

    private PanelWrapper wrapper = new PanelWrapper();
    private List<Panel> panelList = new List<Panel>();

    #region Start
    private void Start()
    {
        panelList = ReadLevelsData();
        WriteLevelsData();       
        TogglePlayButton(false);
    }

    private void OnEnable()
    {
        levelToggleGroup.NewToggledButton.AddListener(OnToggleButtonChange);
    }

    private void OnDisable()
    {
        levelToggleGroup.NewToggledButton.RemoveListener(OnToggleButtonChange);
    }
    #endregion

    #region Initialize data
    private List<Panel> ReadLevelsData()
    {
        string json = File.ReadAllText(levelsDataPath);
        wrapper = JsonUtility.FromJson<PanelWrapper>(json);

        //Debug.Log(wrapper.panelList.Count);

        maxPanelNumber = wrapper.panelList.Count;

        return wrapper.panelList;
    }

    private void WriteLevelsData()
    {
        for (int i = 0; i < panelList.Count; i++)
        {
            panelObjectList[i].SetPanelName(panelList[i].name);
            panelObjectList[i].SetPanelDescription(panelList[i].description);

            panelObjectList[i].SetButtons(panelList[i]);
        }

        if (panelList.Count <= panelObjectList.Count) {

            for (int i = panelList.Count; i < panelObjectList.Count; i++)
            {
                Debug.Log("a");
                panelObjectList[i].gameObject.SetActive(false);
            }
        }
    }
    #endregion

    private void ClearLevelDescription()
    {
        levelDescription.text = string.Empty;
    }

    public void QuitApp()
    {
        Debug.Log("Exit the game");
        Application.Quit();
    }

    private void TogglePlayButton(bool newState)
    {
        playButton.interactable = newState;
    }

    public void OnToggleButtonChange()
    {
        ToggleButton currentToggle = levelToggleGroup.GetCurrentToggledButton();


        if (currentToggle == null)
        {
            TogglePlayButton(false);
            ClearLevelDescription();
            return;
        }

        levelDescription.text = currentToggle.GetComponent<LevelButton>().GetLevelDescription();
        TogglePlayButton(true);

        //Debug.Log("Current toggle changed");
    }

    public void LaunchSelectedLevel()
    {
        LevelButton currentLevel = levelToggleGroup.GetCurrentToggledButton().GetComponent<LevelButton>();

        LevelData.mapName = currentLevel.GetMap();
        LevelData.goalFileName = currentLevel.GetGoal();
        LevelData.dialoguename = currentLevel.GetDialogue();

        LevelData.currentPanel = currentLevel.GetPanel();
        LevelData.currentLevelIndex = currentLevel.GetLevelIndex();

        SceneManager.LoadScene("PlayGround");
    }
}
