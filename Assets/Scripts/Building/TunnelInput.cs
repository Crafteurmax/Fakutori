using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TunnelInput : Building
{
    [SerializeField] private BuildingInput buildingInput;
    [SerializeField] private TunnelOutput tunnelOutput;
    [SerializeField] private int maxTunnelLength;

    /* The intermediate input is located in the middle of the tunnel.
    * It is needed so that the tunnel acts as a conveyor belt and
    * items don't clog up before the input.
    */
    [SerializeField] private BuildingInput intermediateInput;

    private BuildingOutput buildingOutput;
    private float distance;

    private bool isMovingStart = false;
    private bool isMovingIntermediate = false;

    public void Awake() {

        intermediateInput.transform.Translate(Vector3.forward * distance / 2);
        intermediateInput.SetItemPosition(intermediateInput.transform.position);

        buildingInput.SetIsBeltInput(true);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.TunnelInput);
        buildingInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(buildingInput.GetPosition(), buildingInput);
        UpdateOutput();
    }

    public override void OnDisable() {
        BuildingManager.Instance.RemoveBuildingInput(buildingInput.GetPosition());

        base.OnDisable();
    }

    private void Update() {
        if (!tunnelOutput) return;
        buildingInput.SetOutputFull(intermediateInput.IsOccupied());
        if (!isMovingStart && buildingInput.IsOccupied() && !intermediateInput.IsOccupied() && !isMovingIntermediate) {
            StartCoroutine(MoveItemStart());
        }

        if (!isMovingIntermediate && intermediateInput.IsOccupied() && !buildingOutput.IsOccupied() && !buildingOutput.IsMovingItem()) {
            StartCoroutine(MoveItemIntermediate());
        }
    }

    /// <summary>
    /// Coroutine to start moving the item from the input to the intermediate input.
    /// </summary>
    /// <returns></returns>
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

    private void LookForOutputToBeLinkedTo()
    {
        Vector3Int inputPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        // Debug.Log("inputPosition : " + inputPosition);
        Vector3Int direction = getTunnelDirection();
        // Debug.Log("direction : " + direction);
        for (int i = 1; i <= maxTunnelLength; i++)
        {
            Vector3Int pos = inputPosition + direction * i;
            // Debug.Log(gameObject.name + " is looking at : " + pos);
            BuildingTile tile =  BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(pos);
            if (!tile) continue;
            BuildingType buildingTypeOfFoundBuilding = tile.building.GetBuildingType();
            switch (buildingTypeOfFoundBuilding)
            {
                case BuildingType.TunnelOutput:
                    TunnelOutput potentialTunnelOutput = (TunnelOutput) tile.building;
                    if(direction != potentialTunnelOutput.getTunnelDirection()) continue;
                    tunnelOutput = potentialTunnelOutput;
                    if(tunnelOutput.hasALink()) tunnelOutput.unlinkTunnel();
                    tunnelOutput.SetTunnelInput(this);
                    return;
                case BuildingType.TunnelInput:
                    TunnelInput potentialBlockingTunnelInput = (TunnelInput)tile.building;
                    if (direction != potentialBlockingTunnelInput.getTunnelDirection()) continue;
                    tunnelOutput = null;
                    return;
            }
        }
        tunnelOutput = null;
    }

    public Vector3Int getTunnelDirection()
    {
        if (transform.eulerAngles.y == 180) return new Vector3Int( 0,-1, 0);
        if (transform.eulerAngles.y == 0  ) return new Vector3Int( 0, 1, 0);
        if (transform.eulerAngles.y == 270) return new Vector3Int(-1, 0, 0);
        if (transform.eulerAngles.y == 90 ) return new Vector3Int( 1, 0, 0);
        return Vector3Int.one;
    }

    public void UpdateOutput()
    {
        LookForOutputToBeLinkedTo();

        if (tunnelOutput != null)
        {
            buildingOutput = tunnelOutput.GetBuildingOutput();
            distance = Vector3.Distance(buildingInput.GetWorldPosition(), buildingOutput.GetWorldPosition());
        }
    }

    public void SetTunnelOutput(TunnelOutput _tunnelOutput)
    {
        tunnelOutput = _tunnelOutput;
        if (tunnelOutput == null) return;

        buildingOutput = tunnelOutput.GetBuildingOutput();
        distance = Vector3.Distance(buildingInput.GetWorldPosition(), buildingOutput.GetWorldPosition());

    }

    public void unlinkTunnel() { tunnelOutput.SetTunnelInput(null); }

    public bool hasALink() { return tunnelOutput != null; }
}
