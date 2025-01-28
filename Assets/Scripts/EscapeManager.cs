using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeManager : MonoBehaviour
{
    [SerializeField] private PanelManger panelManager;
    [SerializeField] private SelectionUI selectionUI;
    [SerializeField] private BuildingPlacer buildingPlacer;

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started) { return; }

        if (buildingPlacer != null && buildingPlacer.DoActionOnEscape())
        {
            buildingPlacer.OnEscapePress(context);
        }
        else if (selectionUI != null && selectionUI.DoActionOnEscape())
        {
            selectionUI.SetCurrentBuildingTypeToNone(context);
        }

        else if (panelManager != null)
        {
            panelManager.ReturnToPreviousPanel(context);
        }
    }
}
