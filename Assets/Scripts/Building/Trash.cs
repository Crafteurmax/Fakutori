using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trash : Building
{
    [SerializeField] BuildingInput trashInput;

    private void Awake() {
        trashInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(trashInput.GetPosition(), trashInput);
        SetBuildingType(BuildingType.Trash);
    }

    private void Update() {
        Item item = trashInput.GetItem();
        if (item != null) {
            ItemFactory.Instance.Release(item);
            trashInput.SetItem(null);
        }
    }

    public override void Release()
    {
        BuildingManager.Instance.RemoveBuildingInput(trashInput.GetPosition());

        trashInput.Reset();
        base.Release();
    }
}
