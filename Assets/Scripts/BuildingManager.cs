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
            BuildingInput nextBuildingInput = buildingInputs[nextPosition];
            if (!nextBuildingInput.gameObject.activeSelf || (!nextBuildingInput.IsBeltInput() && nextBuildingInput.GetDirection() != direction) ) {
                return null;
            }
            return buildingInputs[nextPosition];
        }
        return null;
    }

    public void AddBuildingInput(Vector3Int position, BuildingInput buildingInput) {
        if (buildingInputs.ContainsKey(position)) {
            Debug.LogError("BuildingInput already exists at " + position);
        }
        buildingInputs.Add(position, buildingInput);
    }

    public void RemoveBuildingInput(Vector3Int position) {
        if (buildingInputs.ContainsKey(position)) {
            buildingInputs.Remove(position);
        }
    }
}
