using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingOutput : MonoBehaviour
{
    private Item item;
    private Item outgoingItem;
    private Vector3 itemPosition;

    private BuildingInput nextBuildingInput;
    public bool isMovingItem;

    private Vector3Int position;
    private Vector3Int direction;

    private void Awake() {
        itemPosition = transform.position;
    }

    private void OnEnable() {
        position = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        direction = new Vector3Int((int)transform.forward.x, (int)transform.forward.z, 0);

        isMovingItem = false;
    }

    private void Update() {
        if (nextBuildingInput == null || !nextBuildingInput.gameObject.activeSelf) nextBuildingInput = GetnextBuildingInput();   

        if (!isMovingItem && item != null && nextBuildingInput != null && nextBuildingInput.IsOccupied() == false)
        {
            StartCoroutine(MoveItem());
            //Debug.Log("Moving item of belt output (" + item.name + ") to next building input");
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
        Debug.Log("Target position for item (" + movingItem.name + "): " + targetPosition);

        while (movingItem.transform.position != targetPosition && nextBuildingInput != null)
        {
            movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * Time.deltaTime);

            yield return null;
        }

        if (nextBuildingInput != null) {
            nextBuildingInput.SetItem(movingItem);
        } else {
            Destroy(movingItem.gameObject);
        }
        outgoingItem = null;
        isMovingItem = false;
    }

    private BuildingInput GetnextBuildingInput()
    {
        return BuildingManager.Instance.GetNextBuildingInput(position, direction);
    }

    public Item GetItem() {
        return item;
    }

    public void SetItem(Item newItem) {
        item = newItem;
    }

    public Item GetOutgoingItem() {
        return outgoingItem;
    }

    public void SetOutgoingItem(Item newOutgoingItem) {
        outgoingItem = newOutgoingItem;
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

    public bool IsOccupied() {
        return item != null;
    }
}
