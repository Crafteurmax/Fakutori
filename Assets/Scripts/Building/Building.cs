using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public Building pair;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float animationTime = 4.0f;

    public enum BuildingType {
        None,
        Belt,
        Splitter,
        TunnelInput,
        TunnelOutput,
        Extractor,
        Trash,
        Vendor,
        Concatenator,
        Exchangificator,
        Hiraganificator,
        Kanjificator,
        Katanificator,
        Maruificator,
        Miniaturisator,
        Tentenificator,
        Troncator,
        Filler
    }

    private BuildingType buildingType;

    public virtual void OnEnable() 
    {
        Vector3Int positionOnTheGrid = BuildingManager.Instance.buildingTilemap.WorldToCell(transform.position);
        BuildingTile tile = ScriptableObject.CreateInstance<BuildingTile>();
        tile.building = this;

        BuildingManager.Instance.buildingTilemap.SetTile(positionOnTheGrid, tile);
    }

    public virtual void OnDisable() {}

    public void SetBuildingType(BuildingType buildingType) {
        this.buildingType = buildingType;
    }

    
    public BuildingType GetBuildingType() {
        return buildingType;
    }

    public virtual void Release() {
        BuildingFactory.Instance.ReleaseBuilding(this);
    }

    [ContextMenu("PrintBuildingType")]
    private void DebugPrintBuildingType()
    {
        Debug.Log(GetBuildingType());
    }
}
