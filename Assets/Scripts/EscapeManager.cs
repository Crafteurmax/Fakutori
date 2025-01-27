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

        //Debug.Log("Coucou");
        //Debug.Log(panelManager != null);

        if (buildingPlacer != null && (buildingPlacer.DoActionOnEscape() || !buildingPlacer.IsSelectionEmpty()))
        {
            //Debug.Log(1);
            buildingPlacer.OnEscapePress(context);
        }
        else if (selectionUI != null && selectionUI.DoActionOnEscape())
        {
            //Debug.Log(2);
            selectionUI.SetCurrentBuildingTypeToNone(context);
        }

        else if (panelManager != null)
        {
            //Debug.Log(3);
            panelManager.ReturnToPreviousPanel(context);
        }
    }
}
