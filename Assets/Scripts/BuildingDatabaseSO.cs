using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BuildingDatabaseSO : ScriptableObject
{
    public List<BuildingData> buildingData;
}

[Serializable]
public class BuildingData
{
    [field: SerializeField]
    public string name { get; private set; }
    [field: SerializeField]
    public Building.BuildingType type { get; private set; }
    [field: SerializeField]
    public Vector2Int size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject buildingPrefab { get; private set; }
}
