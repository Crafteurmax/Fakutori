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
        currentPanels.Push(panelToToggle);

        foreach (GameObject panel in panels)
        {
            panel.SetActive(panel == panelToToggle);
        }
    }

    public void ReturnToPreviousPanel()
    {
        if(currentPanels.Count <= 1) { return; }

        currentPanels.Pop();
        TogglePanel(currentPanels.Pop());
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