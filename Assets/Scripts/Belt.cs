using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Building
{
    private static int beltID = 0;

    private BuildingInput beltInput;
    private BuildingOutput beltOutput;

    private void Awake() {
        beltInput = GetComponentInChildren<BuildingInput>();
        beltOutput = GetComponentInChildren<BuildingOutput>();

        beltID++;
        gameObject.name = "Belt" + beltID;
    }

    public override void OnEnable() {
        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();

        if (beltInput.GetItem() != null) Destroy(beltInput.GetItem().gameObject);
        beltInput.SetItem(null);
        if (beltInput.GetIncomingItem() != null) Destroy(beltInput.GetIncomingItem().gameObject);
        beltInput.SetIncomingItem(null);
        if (beltOutput.GetItem() != null) Destroy(beltOutput.GetItem().gameObject);
        beltOutput.SetItem(null);
        if (beltOutput.GetOutgoingItem() != null) Destroy(beltOutput.GetOutgoingItem().gameObject);
        beltOutput.SetOutgoingItem(null);

        beltInput.SetOutputFull(false);
        BuildingManager.Instance.RemoveBuildingInput(beltInput.GetPosition());
    }

    private void Update() {
        beltInput.SetOutputFull(beltOutput.IsOccupied());
        if (beltInput.IsOccupied() && !beltOutput.IsOccupied() && !beltOutput.isMovingItem) {
            beltOutput.SetItem(beltInput.GetItem());
            beltInput.SetOutputFull(true);
            beltInput.SetItem(null);
        }
    }
}