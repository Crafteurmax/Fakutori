using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    private string levelName;
    private string levelDescription;
    private string levelGoal;
    private string levelMap;
    private string levelDialogue;

    private int levelIndex;
    private Panel panel;


    private void Start()
    {
        buttonText.text = levelName;
    }

    #region Setter / Getter

    public void SetPanel(Panel panel)
    {
        this.panel = panel;
    }

    public Panel GetPanel()
    {
        return panel;
    }

    public void SetLevelIndex(int inedx)
    {
        levelIndex = inedx;
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public void SetName(string name)
    {
        levelName = name;
        buttonText.text = levelName;
    }

    public string GetName()
    {
        return levelName;
    }

    public void SetDescription(string description)
    {
        levelDescription = description;
    }

    public string GetLevelDescription()
    {
        return levelDescription;
    }

    public void SetGoal(string goal)
    {
        levelGoal = goal;
    }

    public string GetGoal()
    {
        return levelGoal;
    }

    public void SetMap(string map)
    {
        levelMap = map;
    }

    public string GetMap()
    {
        return levelMap;
    }

    public void SetDialogue(string dialogue)
    {
        levelDialogue = dialogue;
    }

    public string GetDialogue()
    {
        return levelDialogue;
    }

    #endregion
}
