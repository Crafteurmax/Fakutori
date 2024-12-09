using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TunnelOutput : Building
{
    [SerializeField] private BuildingOutput buildingOutput;
    public TunnelInput tunnelInput;
    [SerializeField] private int maxTunnelLength;

    public BuildingOutput GetBuildingOutput() {
        return buildingOutput;
    }


    void Awake()
    {
        
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.TunnelOutput);
        Vector3Int inputPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        LookForOutputToBeLinkedTo();
        if (tunnelInput)
        {
            Debug.Log(gameObject.name + " : Found something !!! " + tunnelInput.gameObject.name + " <3");
        }
        else Debug.Log(gameObject.name + " : found nothing :,(");
    }

    private void LookForOutputToBeLinkedTo()
    {
        Vector3Int outputPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        // Debug.Log("outputPosition : " + outputPosition);
        Vector3Int direction = getTunnelDirection();
        // Debug.Log("direction : " + direction);
        for (int i = 1; i <= maxTunnelLength; i++)
        {
            Vector3Int pos = outputPosition - direction * i;
            BuildingTile tile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(pos);
            if (!tile) continue;
            BuildingType buildingTypeOfFoundBuilding = tile.building.GetBuildingType();
            switch (buildingTypeOfFoundBuilding)
            {
                case BuildingType.TunnelInput:
                    TunnelInput potentialTunnelInput = (TunnelInput)tile.building;
                    if (direction != potentialTunnelInput.getTunnelDirection()) continue;
                    tunnelInput = potentialTunnelInput;
                    if (tunnelInput.hasALink()) tunnelInput.unlinkTunnel();
                    tunnelInput.SetTunnelOutput(this);
                    return;
                case BuildingType.TunnelOutput:
                    TunnelOutput potentialBlockingTunnelOutput = (TunnelOutput)tile.building;
                    if (direction != potentialBlockingTunnelOutput.getTunnelDirection()) continue;
                    tunnelInput = null;
                    return;
            }
        }
        tunnelInput = null;
    }

    public Vector3Int getTunnelDirection()
    {
        if (transform.eulerAngles.y == 180) return new Vector3Int(0, -1, 0);
        if (transform.eulerAngles.y == 0) return new Vector3Int(0, 1, 0);
        if (transform.eulerAngles.y == 270) return new Vector3Int(-1, 0, 0);
        if (transform.eulerAngles.y == 90) return new Vector3Int(1, 0, 0);
        return Vector3Int.one;
    }

    public void SetTunnelInput(TunnelInput _tunnelInput)
    {
        tunnelInput = _tunnelInput;
    }

    public void unlinkTunnel() { tunnelInput.SetTunnelOutput(null); }

    public bool hasALink() { return tunnelInput != null; }
}
