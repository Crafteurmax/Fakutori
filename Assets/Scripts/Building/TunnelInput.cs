using System.Collections;
using UnityEngine;

public class TunnelInput : Building
{
    [SerializeField] private BuildingInput buildingInput;
    [SerializeField] private TunnelOutput tunnelOutput;
    [SerializeField] private int maxTunnelLength;
    [SerializeField] private GameObject paperPlanePrefab;

    private BuildingOutput buildingOutput;
    private float distance;

    private bool isMovingStart = false;
    private bool isMovingIntermediate = false;

    public void Awake() {

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
        Release();

        base.OnDisable();
    }

    private void Update() {
        if (!tunnelOutput) return;
        if (!isMovingStart && buildingInput.IsOccupied() && !buildingOutput.IsOccupied()) {
            StartCoroutine(MoveItemStart());
        }
        if (!buildingOutput.IsOccupied()) buildingInput.SetOutputFull(false);
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
        movingItem.SetInvisible(true);

        GameObject paperPlane = Instantiate(paperPlanePrefab, movingItem.transform.position, transform.rotation);

        Vector3 initialPosition = movingItem.transform.position;
        Vector3 targetPosition = buildingOutput.GetItemPosition(movingItem.GetItemHeightOffset());

        float hauteur = distance / 8f;

        Vector3 controlPoint1 = movingItem.transform.position + new Vector3(0f, hauteur, 0f);
        Vector3 controlPoint2 = targetPosition + new Vector3(0f, hauteur, 0f);

        float t = 0f;
        Vector3 lastPosition = paperPlane.transform.position;

        while (movingItem != null && movingItem.transform.position != targetPosition && buildingOutput != null)
        {
            t = 1f - Vector3.Distance(movingItem.transform.position, targetPosition) / distance;
            Debug.Log("t = " + t);
            movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * distance * Time.deltaTime);
            
            paperPlane.transform.position = Vector3.Lerp(
                Vector3.Lerp(
                    Vector3.Lerp(initialPosition, controlPoint1, t),
                    Vector3.Lerp(controlPoint1, controlPoint2, t), 
                    t), 
                Vector3.Lerp(
                    Vector3.Lerp(controlPoint1, controlPoint2, t), 
                    Vector3.Lerp(controlPoint2, targetPosition, t), 
                    t), 
                t);
            lastPosition = paperPlane.transform.position;

            paperPlane.transform.rotation = Quaternion.LookRotation(paperPlane.transform.position - lastPosition);
            yield return null;
        }

        if (buildingOutput != null) {
            buildingOutput.SetItem(movingItem);
        }

        movingItem.SetInvisible(false);
        Destroy(paperPlane);

        isMovingStart = false;
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

    public override void Release()
    {
        buildingInput.SetOutputFull(true);
        buildingInput.Reset();
        BuildingManager.Instance.RemoveBuildingInput(buildingInput.GetPosition());
        base.Release();
    }
}
