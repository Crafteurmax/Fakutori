using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public enum BuildingType {
        Belt,
        Kanjificator,
    }

    private BuildingType buildingType;

    public virtual void OnEnable() {}

    public virtual void OnDisable() {}

    public void SetBuildingType(BuildingType buildingType) {
        this.buildingType = buildingType;
    }

    public BuildingType GetBuildingType() {
        return buildingType;
    }

    protected virtual void Release() {
        BuildingFactory.Instance.ReleaseBuilding(this);
    }
}
