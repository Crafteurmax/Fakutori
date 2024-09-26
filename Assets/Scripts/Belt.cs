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
        Release();
    }

    protected override void Release() {
        if (beltInput.GetItem() != null) ItemFactory.Instance.Release(beltInput.GetItem());
        beltInput.SetItem(null);
        if (beltInput.GetIncomingItem() != null) ItemFactory.Instance.Release(beltInput.GetIncomingItem());
        beltInput.SetIncomingItem(null);
        if (beltOutput.GetItem() != null) ItemFactory.Instance.Release(beltOutput.GetItem());
        beltOutput.SetItem(null);
        if (beltOutput.GetOutgoingItem() != null) ItemFactory.Instance.Release(beltOutput.GetOutgoingItem());
        beltOutput.SetOutgoingItem(null);

        beltInput.SetOutputFull(false);
        BuildingManager.Instance.RemoveBuildingInput(beltInput.GetPosition());
        
        base.Release();
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