using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelInput : Building
{
    [SerializeField] private BuildingInput buildingInput;
    [SerializeField] private TunnelOutput tunnelOutput;

    [SerializeField] private BuildingInput intermediateInput;

    private BuildingOutput buildingOutput;
    private float distance;

    private bool isMovingStart = false;
    private bool isMovingIntermediate = false;

    private void Awake() {
        buildingOutput = tunnelOutput.GetBuildingOutput();
        distance = Vector3.Distance(buildingInput.GetPosition(), buildingOutput.GetPosition());

        intermediateInput.transform.Translate(Vector3.forward * distance / 2);
        intermediateInput.SetItemPosition(intermediateInput.transform.position);

        buildingInput.SetIsBeltInput(true);
    }

    public override void OnEnable() {
        BuildingManager.Instance.AddBuildingInput(buildingInput.GetPosition(), buildingInput);

        base.OnEnable();
    }

    public override void OnDisable() {
        BuildingManager.Instance.RemoveBuildingInput(buildingInput.GetPosition());

        base.OnDisable();
    }

    private void Update() {
        buildingInput.SetOutputFull(intermediateInput.IsOccupied());
        if (!isMovingStart && buildingInput.IsOccupied() && !intermediateInput.IsOccupied() && !isMovingIntermediate) {
            StartCoroutine(MoveItemStart());
            //Debug.Log("MoveItemStart");
        }
        //Debug.Log("intermediate item : " + intermediateInput.GetItem());
        if (!isMovingIntermediate && intermediateInput.IsOccupied() && !buildingOutput.IsOccupied() && !buildingOutput.IsMovingItem()) {
            StartCoroutine(MoveItemIntermediate());
            //Debug.Log("MoveItemIntermediate");
        }
    }

    private IEnumerator MoveItemStart() {
        buildingInput.SetOutputFull(true);
        Item movingItem = buildingInput.GetItem();        
        buildingInput.SetItem(null);
        isMovingStart = true;

        Vector3 targetPosition = intermediateInput.GetItemPosition(movingItem.GetItemHeightOffset());

        while (movingItem != null && movingItem.transform.position != targetPosition && intermediateInput != null)
        {
            movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * distance / 2 * Time.deltaTime);

            yield return null;
        }

        if (intermediateInput != null) {
            intermediateInput.SetItem(movingItem);
        }

        isMovingStart = false;
    }

    private IEnumerator MoveItemIntermediate() {
        intermediateInput.SetOutputFull(true);
        Item movingItem = intermediateInput.GetItem();
        intermediateInput.SetItem(null);
        isMovingIntermediate = true;

        Vector3 targetPosition = buildingOutput.GetItemPosition(movingItem.GetItemHeightOffset());

        while (movingItem != null && movingItem.transform.position != targetPosition && buildingOutput != null)
        {
            movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * distance / 2 * Time.deltaTime);

            yield return null;
        }

        if (buildingOutput != null) {
            buildingOutput.SetItem(movingItem);
        }

        isMovingIntermediate = false;
    }
}