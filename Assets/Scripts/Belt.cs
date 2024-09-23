using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Building
{
    private BuildingInput beltInput;
    private BuildingOutput beltOutput;

    private void Awake() {
        beltInput = GetComponent<BuildingInput>();
        beltOutput = GetComponent<BuildingOutput>();
    }

    public override void OnEnable() {
        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();
    }

    private void Update() {
        if (beltInput.GetItem() != null && !beltOutput.IsOccupied() && !beltOutput.isMovingItem) {
            beltOutput.SetItem(beltInput.GetItem());
            beltInput.SetItem(null);
        }
    }
}
