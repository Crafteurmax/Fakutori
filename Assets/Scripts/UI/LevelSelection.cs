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
    [SerializeField] private Button deleteSaveButton;

    [Header("Panels")]
    [SerializeField] private List<LevelPanelController> panelObjectList;

    private readonly string levelsDataPath = Application.dataPath + "/Resources/levelPanelData.json";
    private readonly string saveFileFolderPath = Application.dataPath + "/Resources/Save/";

    //private PanelWrapper wrapper = new PanelWrapper();
    private List<Panel> panelList = new();

    #region Start
    private void Start()
    {
        panelList = ReadLevelsData();
        WriteLevelsData();
        TogglePlayDeleteButton(false);
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
        PanelWrapper wrapper = JsonUtility.FromJson<PanelWrapper>(json);

        //Debug.Log(wrapper.panelList.Count)

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

    private void TogglePlayDeleteButton(bool newState)
    {
        playButton.interactable = newState;
        deleteSaveButton.interactable = newState;
    }

    public void OnToggleButtonChange()
    {
        ToggleButton currentToggle = levelToggleGroup.GetCurrentToggledButton();

        if (currentToggle == null)
        {
            TogglePlayDeleteButton(false);
            ClearLevelDescription();
            return;
        }

        levelDescription.text = currentToggle.GetComponent<LevelButton>().GetLevelDescription();
        UpdateLevelDataOnToggledButtonChange();
        TogglePlayDeleteButton(true);

        //Debug.Log("Current toggle changed");
    }

    private void UpdateLevelDataOnToggledButtonChange()
    {
        LevelButton currentLevel = levelToggleGroup.GetCurrentToggledButton().GetComponent<LevelButton>();

        LevelData.mapName = currentLevel.GetMap();
        LevelData.goalFileName = currentLevel.GetGoal();
        LevelData.dialoguename = currentLevel.GetDialogue();

        LevelData.currentPanel = currentLevel.GetPanel();
        LevelData.currentLevelIndex = currentLevel.GetLevelIndex();

        LevelData.isProccedural = false;
    }

    public void DeleteSelectedSave()
    {
        string saveFilePath = saveFileFolderPath + LevelData.mapName;
        File.Delete(saveFilePath);
    }

    public void LaunchSelectedLevel()
    {
        SceneManager.LoadScene("PlayGround");
    }

    public void LaunchFreeMode()
    {
        LevelData.isProccedural = true;
        LevelData.dialoguename = "freeMode";
        LevelData.mapName = "freeMode"; 
        LaunchSelectedLevel();
    }
}
