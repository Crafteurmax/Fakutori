using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Building
{
    [SerializeField] BuildingInput vendorInput;

    private void Awake() {
        BuildingManager.Instance.AddBuildingInput(vendorInput.GetPosition(), vendorInput);
        SetBuildingType(BuildingType.Vendor);
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
}
