using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingOutput : MonoBehaviour
{
    private static int buildingOutputID = 0;

    private Item item;
    private Item outgoingItem;
    private Vector3 itemPosition;

    private BuildingInput nextBuildingInput;
    private bool isMovingItem;

    private Vector3Int position;
    private Vector3Int direction;

    private void Awake() {
        itemPosition = transform.position;
        buildingOutputID++;
        gameObject.name = "BuildingOutput" + buildingOutputID;
    }

    private void OnEnable() {
        position = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        direction = new Vector3Int((int)transform.forward.x, (int)transform.forward.z, 0);

        isMovingItem = false;
    }

    public void Reset() {
        ClearItem();
        ClearOutgoingItem();
        SetIsMovingItem(false);
    }

    private void Update() {
        if (nextBuildingInput == null || !nextBuildingInput.transform.parent.gameObject.activeSelf) nextBuildingInput = GetNextBuildingInput();

        if (!isMovingItem && item != null && nextBuildingInput != null && nextBuildingInput.IsOccupied() == false && nextBuildingInput.GetIncomingItem() == null && !nextBuildingInput.IsOutputFull())
        {
            StartCoroutine(MoveItem());
        }
    }

    private IEnumerator MoveItem()
    {
        Item movingItem = item;
        outgoingItem = item;
        nextBuildingInput.SetIncomingItem(movingItem);
        item = null;
        isMovingItem = true;

        Vector3 targetPosition = nextBuildingInput.GetItemPosition(movingItem.GetItemHeightOffset());

        while (movingItem != null && movingItem.transform.position != targetPosition && nextBuildingInput != null)
        {
            movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * Time.deltaTime);

            yield return null;
        }

        if (nextBuildingInput != null)
        {
            nextBuildingInput.SetItem(movingItem);
            nextBuildingInput.SetIncomingItem(null);
        }
        //else ItemFactory.Instance.Release(movingItem);
        outgoingItem = null;
        isMovingItem = false;
    }

    private BuildingInput GetNextBuildingInput()
    {
        BuildingInput nextBuildingInput = BuildingManager.Instance.GetNextBuildingInput(position, direction);

        return nextBuildingInput;
    }

    public Item GetItem() {
        return item;
    }
    public void SetItem(Item newItem)
    {
        item = newItem;
    }
    public void ClearItem() {
        if (item != null) {
            ItemFactory.Instance.Release(item);
            item = null;
        }
    }
    
    public Item GetOutgoingItem() {
        return outgoingItem;
    }
    public void SetOutgoingItem(Item newOutgoingItem) {
        outgoingItem = newOutgoingItem;
    }
    public void ClearOutgoingItem() {
        if (outgoingItem != null) {
            ItemFactory.Instance.Release(outgoingItem);
            nextBuildingInput.SetIncomingItem(null);
            outgoingItem = null;
        }
    }

    public Vector3Int GetPosition() {
        return position;
    }
    public void SetPosition(Vector3Int newPosition) {
        position = newPosition;
    }

    public Vector3Int GetDirection() {
        return direction;
    }
    public void SetDirection(Vector3Int newDirection) {
        direction = newDirection;
    }


    public bool IsMovingItem() {
        return isMovingItem;
    }
    public void SetIsMovingItem(bool moving) {
        isMovingItem = moving;
    }
    
    public bool IsOccupied() {
        return item != null;
    }

    public Vector3 GetItemPosition(float yOffset) {
        return itemPosition + new Vector3(0, yOffset, 0);
    }

    public Vector3 GetWorldPosition() {
        return transform.position;
    }
}
