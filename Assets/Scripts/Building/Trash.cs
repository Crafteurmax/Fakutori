using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] BuildingInput trashInput;

    private void Awake() {
        BuildingManager.Instance.AddBuildingInput(trashInput.GetPosition(), trashInput);
    }

    private void Update() {
        Item item = trashInput.GetItem();
        if (item != null) {
            ItemFactory.Instance.Release(item);
            trashInput.SetItem(null);
        }
    }
}
