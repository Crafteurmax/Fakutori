using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingFactory : MonoBehaviour
{
    public static BuildingFactory Instance { get; private set; }

    private Dictionary<Building.BuildingType, Stack<Building>> buildingPool = new Dictionary<Building.BuildingType, Stack<Building>>();

    [Header("Prefabs")]
    [SerializeField] private List<Building.BuildingType> buildingTypes;
    [SerializeField] private List<Building> buildingPrefabs;
    private Dictionary<Building.BuildingType, Building> buildingPrefabDict = new Dictionary<Building.BuildingType, Building>();

    private void Awake() {
        Instance = this;

        for (int i = 0; i < buildingTypes.Count; i++) {
            buildingPrefabDict.Add(buildingTypes[i], buildingPrefabs[i]);
        }
    }

    public Building GetBuilding(Building.BuildingType buildingType) {
        Building building = null;

        if (buildingPool.ContainsKey(buildingType)) {
            buildingPool[buildingType].TryPop(out building);
        } else {
            buildingPool.Add(buildingType, new Stack<Building>());
        }

        if (building != null) {
            building.gameObject.SetActive(true);
            return building;
        }
        return InstantiateBuilding(buildingType);
    }

    private Building InstantiateBuilding(Building.BuildingType buildingType) {
        Building building = null;

        if (buildingPrefabDict.ContainsKey(buildingType)) {
            building = Instantiate(buildingPrefabDict[buildingType], transform);
            building.SetBuildingType(buildingType);
        } else {
            Debug.LogError("Building type not found: " + buildingType);
        }

        return building;
    }

    public void ReleaseBuilding(Building building) {
        building.gameObject.SetActive(false);

        Building.BuildingType buildingType = building.GetBuildingType();
        if (buildingPool.ContainsKey(buildingType)) {
            buildingPool[building.GetBuildingType()].Push(building);
        } else {
            buildingPool.Add(buildingType, new Stack<Building>());
        }
    }
}
