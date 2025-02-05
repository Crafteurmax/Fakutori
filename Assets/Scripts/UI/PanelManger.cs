using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PanelManger : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject returnPanel;
    [SerializeField] private GameObject DialoguePanel;

    [Header("Panels")]
    [SerializeField] private List<GameObject> panels = new();

    private readonly Stack<GameObject> currentPanels = new();
    [SerializeField] WorldSaver worldSaver;

    public static string Untagged { get; private set; } = "Untagged";
    public static string NoEscape { get; private set; } = "noEscape";

    private void Start()
    {
        TogglePanel(startPanel);
        if(DialoguePanel != null) TogglePanel(DialoguePanel);
    }

    public void TogglePanel(GameObject panelToToggle)
    {
        if (currentPanels.Count > 0 && currentPanels.Peek().name == "Dictionnary Panel") {
            AudioManager.instance.PlayEffect("CloseBook");
        }

        currentPanels.Push(panelToToggle);

        if (panelToToggle.name == "Dictionnary Panel") {
            AudioManager.instance.PlayEffect("OpenBook");
        }

        foreach (GameObject panel in panels)
        {
            panel.SetActive(panel == panelToToggle);
        }
    }

    public void ReturnToPreviousPanel()
    {
        if(currentPanels.Count <= 1) { return; }

        
        GameObject previousPanel = currentPanels.Pop();
        if (previousPanel.name == "Dictionnary Panel") {
            AudioManager.instance.PlayEffect("CloseBook");
        }

        GameObject currentPanel = currentPanels.Pop();
        if (currentPanel.name == "Dictionnary Panel") {
            AudioManager.instance.PlayEffect("OpenBook");
        }

        TogglePanel(currentPanel);
    }

    public void ReturnToPreviousPanel(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started) return;
        if (currentPanels.Peek().CompareTag(NoEscape)) return;

        if (currentPanels.Count > 1)
        {
            ReturnToPreviousPanel();
        }
        else
        {
            TogglePanel(returnPanel);
        }
    }

    public void LoadTitleMenu()
    {
        worldSaver.WriteData(LevelData.mapName);
        SceneManager.LoadScene("Menu");
    }
}