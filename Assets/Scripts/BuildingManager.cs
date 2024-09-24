using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    public Dictionary<Vector3Int, BuildingInput> buildingInputs = new Dictionary<Vector3Int, BuildingInput>();

    public Tilemap buildingTilemap;
    public float beltSpeed = 1.0f;

    public static BuildingManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public BuildingInput GetNextBuildingInput(Vector3Int position, Vector3Int direction) {
        Vector3Int nextPosition = position + direction;
        if (buildingInputs.ContainsKey(nextPosition)) {
            //Debug.Log("Next building input found at " + nextPosition);
            return buildingInputs[nextPosition].gameObject.activeSelf ? buildingInputs[nextPosition] : null;
        }
        return null;
    }

    public void AddBuildingInput(Vector3Int position, BuildingInput buildingInput) {
        if (buildingInputs.ContainsKey(position)) {
            Destroy(buildingInputs[position].gameObject);
            buildingInputs.Remove(position);
        }
        buildingInputs.Add(position, buildingInput);
    }

    public void RemoveBuildingInput(Vector3Int position) {
        if (buildingInputs.ContainsKey(position)) {
            buildingInputs.Remove(position);
            Debug.Log("Removed building input at " + position);
        }
    }
}
