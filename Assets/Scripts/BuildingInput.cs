using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInput : MonoBehaviour
{
    private static int buildingInputID = 0;

    [SerializeField] private Item item;
    private Item incomingItem;
    private Vector3 itemPosition;

    private Vector3Int position;
    private Vector3Int direction;

    private bool isOutputFull;

    private void Awake() {
        itemPosition = transform.position;
        buildingInputID++;
        gameObject.name = "BuildingInput" + buildingInputID;
    }

    private void OnEnable() {
        Initialize();
    }

    public void Initialize() {
        position = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        direction = new Vector3Int((int)transform.forward.x, (int)transform.forward.z, 0);
    }

    public Item GetItem() {
        return item;
    }
    public void SetItem(Item newItem) {
        item = newItem;
    }
    public void ClearItem() {
        if (item != null) {
            ItemFactory.Instance.Release(item);
            item = null;
        }
    }

    public Item GetIncomingItem() {
        return incomingItem;
    }
    public void SetIncomingItem(Item newIncomingItem) {
        incomingItem = newIncomingItem;
    }
    public void ClearIncomingItem() {
        if (incomingItem != null) {
            ItemFactory.Instance.Release(incomingItem);
            incomingItem = null;
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

    public bool IsOccupied() {
        return item != null;
    }

    public Vector3 GetItemPosition(float yOffset) {
        return itemPosition + new Vector3(0, yOffset, 0);
    }

    public bool IsOutputFull() {
        return isOutputFull;
    }
    public void SetOutputFull(bool outputFull) {
        isOutputFull = outputFull;
    }
}
