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
            buttonList[i].SetActive(false);
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

    public void SetButtons(List<Level> levelList)
    {
        for (int i = 0; i < levelList.Count; i++)
        {
            levelButtonList[i].SetName(levelList[i].name);
            levelButtonList[i].SetDescription(levelList[i].description);
            levelButtonList[i].SetGoal(levelList[i].goal);
            levelButtonList[i].SetMap(levelList[i].map);
            levelButtonList[i].SetDialogue(levelList[i].dialogue);

            buttonList[i].SetActive(true);
        }
    }
}
