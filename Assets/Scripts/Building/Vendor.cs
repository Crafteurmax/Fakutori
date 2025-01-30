using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Building
{
    [SerializeField] BuildingInput vendorInput;

    private void Awake() {
        vendorInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(vendorInput.GetPosition(), vendorInput);
        SetBuildingType(BuildingType.Vendor);

        GetComponentInChildren<BuildingInput>().SetIsBeltInput(true);
    }

    private void Update() 
    {
        Item item = vendorInput.GetItem();

        if (item != null)
        {
            GoalController.Instance.IncreaseScore(item.ToString());
            ItemFactory.Instance.Release(item);
            vendorInput.SetItem(null);
        }
    }

    public override void Release()
    {
        BuildingManager.Instance.RemoveBuildingInput(vendorInput.GetPosition());

        vendorInput.Reset();
        base.Release();
    }
}
