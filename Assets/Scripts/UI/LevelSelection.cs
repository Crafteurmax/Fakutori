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
    [SerializeField] private CenteredGridLayout levelButtonLayout;
    [SerializeField] private GameObject levelToggleButtonPrefab;
    [SerializeField] private ToggleButtonGroup levelToggleGroup;
    [SerializeField] private Vector2 spacing;

    [Header("Level Description")]
    [SerializeField] private TMP_Text levelDescription;

    [Header("Panel Data")]
    [SerializeField] private TMP_Text panelName;
    [SerializeField] private TMP_Text panelDescription;

    [Header("Navigation Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button playButton;

    private int maxPanelNumber = 1;
    private int selectedPanel = 0;
    private string levelsDataPath = Application.dataPath + "/Resources/levelPanelData.json";

    private PanelWrapper wrapper = new PanelWrapper();
    private List<Panel> panelList = new List<Panel>();

    private List<ToggleButton > toggleButtonList = new List<ToggleButton>();

    #region Start
    private void Start()
    {
        panelList = ReadLevelsData();
        ToggleLeftButton(false);
        ToggleRightButton(true);
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

    private List<Panel> ReadLevelsData()
    {
        string json = File.ReadAllText(levelsDataPath);
        wrapper = JsonUtility.FromJson<PanelWrapper>(json);

        //Debug.Log(wrapper.panelList.Count);

        maxPanelNumber = wrapper.panelList.Count;

        return wrapper.panelList;
    }

    public void LoadSelectedPanel()
    {
        ClearButtonLayout();

        Panel panel = panelList[selectedPanel];

        panelName.text = panel.name;
        panelDescription.text = panel.description;

        foreach (Level level in panel.levelList)
        {
            GameObject toggleButton = Instantiate(levelToggleButtonPrefab);
            ToggleButton toggleButtonComponent = toggleButton.GetComponent<ToggleButton>();

            toggleButtonComponent._group = levelToggleGroup;

            LevelButton levelButton = toggleButton.GetComponent<LevelButton>();

            levelButton.SetName(level.name);
            levelButton.SetDescription(level.description);
            levelButton.SetMap(level.map);
            levelButton.SetGoal(level.goal);
            levelButton.SetDialogue(level.dialogue);

            toggleButtonList.Add(toggleButton.GetComponent<ToggleButton>());
        }
        levelButtonLayout.SetSpacing(spacing);
        levelButtonLayout.AddItems(toggleButtonList);
    }

    public void GoToNextPanel()
    {
        selectedPanel += 1;

        if (selectedPanel > 0)
        {
            ToggleLeftButton(true);
        }

        if (selectedPanel >= maxPanelNumber - 1)
        {
            ToggleRightButton(false);
            selectedPanel = maxPanelNumber - 1;
        }

        TogglePlayButton(false);
        ClearLevelDescription();
        LoadSelectedPanel();
    }

    public void GoToPreviousPanel()
    {
        selectedPanel -= 1;
       
        if (selectedPanel < maxPanelNumber - 1)
        {
            ToggleRightButton(true);
        }

        if (selectedPanel <= 0)
        {
            ToggleLeftButton(false);
            selectedPanel = 0;
        }

        TogglePlayButton(false);
        ClearLevelDescription();
        LoadSelectedPanel();
    }

    private void ClearButtonLayout()
    {
        foreach (Transform child in levelButtonLayout.transform)
        {
            foreach(Transform child2 in child)
            {
                if (child2.name.Contains("Level Toggle"))
                {
                    //Destroy(child2.GetComponent<LevelButton>());
                    Destroy(child2.gameObject);
                }
            }
        }
        toggleButtonList.Clear();
        levelToggleGroup.ClearList();
    }

    private void ClearLevelDescription()
    {
        levelDescription.text = string.Empty;
    }

    public void ToggleLeftButton(bool toggle)
    {
        leftButton.interactable = toggle;
    }

    public void ToggleRightButton(bool toggle)
    {
        rightButton.interactable = toggle;
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
        //Debug.Log(currentToggle.name);

        if (currentToggle == null)
        {
            TogglePlayButton(false);
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

        SceneManager.LoadScene("PlayGround");
    }
}
