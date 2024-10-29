using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PanelManger : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject niponPanel;
    [SerializeField] private GameObject dictionnaryPanel;
    [SerializeField] private GameObject optionsPanel;

    private readonly List<GameObject> panels = new();
    private readonly Stack<GameObject> currentPanels = new();
    private SelectionUI selectionUI;

    private void Start()
    {
        selectionUI = GetComponent<SelectionUI>();

        panels.Add(mainPanel);
        panels.Add(niponPanel);
        panels.Add(dictionnaryPanel);
        panels.Add(optionsPanel);

        TogglePanel(mainPanel);
    }

    private void TogglePanel(GameObject panelToToggle)
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

        foreach (GameObject panel in panels)
        {
            panel.SetActive(panel == currentPanels.Peek());
        }
    }

    public void ReturnToPreviousPanel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (currentPanels.Count == 1 && selectionUI.GetCurrentBuildingType() == Building.BuildingType.None)
            {
                TogglePanel(optionsPanel);
            }
            else
            {
                ReturnToPreviousPanel();
            }
        }
    }

    #region Panel Functions
    public void ToggleNiponPanel()
    {
        TogglePanel(niponPanel);
    }

    public void ToggleDictionnaryPanel()
    {
        TogglePanel(dictionnaryPanel);
    }

    public void ToggleOptionPanel()
    {
        TogglePanel(optionsPanel);
    }
    #endregion Panel Functions
}