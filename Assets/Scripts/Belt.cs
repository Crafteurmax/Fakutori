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

        beltInput.SetIsBeltInput(true);

        beltID++;
        gameObject.name = "Belt" + beltID;
    }

    public override void OnEnable() {
        BuildingManager.Instance.AddBuildingInput(beltInput.GetPosition(), beltInput);

        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();
        Release();
    }

    protected override void Release() {
        beltInput.Reset();
        beltOutput.Reset();
  
        BuildingManager.Instance.RemoveBuildingInput(beltInput.GetPosition());
        
        base.Release();
    }

    private void Update() {
        beltInput.SetOutputFull(beltOutput.IsOccupied());
        if (beltInput.IsOccupied() && !beltOutput.IsOccupied() && !beltOutput.IsMovingItem()) {
            beltOutput.SetItem(beltInput.GetItem());
            beltInput.SetOutputFull(true);
            beltInput.SetItem(null);
        }
    }
}