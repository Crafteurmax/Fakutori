using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelPanelController : MonoBehaviour
{
    [Header("Text Fields")]
    [SerializeField] private TMP_Text panelName;
    [SerializeField] private TMP_Text panelDescription;

    [Header("Buttons list")]
    [SerializeField] private List<GameObject> buttonList;
    [SerializeField] private List<ToggleButton> toggleButtonList;
    [SerializeField] private List<LevelButton> levelButtonList;

    [SerializeField] private ToggleButtonGroup toggleGroup;

    private void Start()
    {
        for (int i =0; i < buttonList.Count; i++)
        {
            toggleGroup.RegisterToggle(toggleButtonList[i]);
            //buttonList[i].SetActive(false);
        }
    }

    public void SetPanelName(string text)
    {
        panelName.text = text;
    }

    public void SetPanelDescription(string description)
    {
        panelDescription.text = description;
    }

    public void SetButtons(Panel panel)
    {
        for (int i = 0; i < panel.levelList.Count; i++)
        {
            levelButtonList[i].SetName(panel.levelList[i].name);
            levelButtonList[i].SetDescription(panel.levelList[i].description);
            levelButtonList[i].SetGoal(panel.levelList[i].goal);
            levelButtonList[i].SetMap(panel.levelList[i].map);
            levelButtonList[i].SetDialogue(panel.levelList[i].dialogue);

            levelButtonList[i].SetLevelIndex(i);
            levelButtonList[i].SetPanel(panel);

            buttonList[i].SetActive(true);
        }

        for (int i = panel.levelList.Count; i <  buttonList.Count; i++)
        {
            buttonList[i].SetActive(false);
        }
    }
}
