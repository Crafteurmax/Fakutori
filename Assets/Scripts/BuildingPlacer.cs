using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Tile indicator")]
    [SerializeField] private TileIndicator tileIndicator;

    [Header("Components")]
    [SerializeField] private GameObject buildingsMap;
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private SelectionUI selectionUI;

    [Header("Buildings")]
    [SerializeField] private Building.BuildingType buildingType = Building.BuildingType.None;
    [SerializeField] private BuildingDatabaseSO buildingDatabaseSO;

    [SerializeField] private GameObject fillerPrefab;

    private Vector3Int tilePosition;
    
    // Logic to know in which state we are
    private bool enablePlacement = false;
    private bool enableRemoval = false;
    private bool isLeftPress = false;

    #region Enable / diasble placement
    private void EnablePlacement()
    {
        tileIndicator.ShowMouseIndicator();
        tileIndicator.UpdateMouseIndicator();
        enablePlacement = true;
    }

    private void DisablePlacement()
    {
        buildingType = Building.BuildingType.None;
        tileIndicator.HideMouseIndicator();
        enablePlacement = false;
    }

    private void EnableRemoval()
    {
        tileIndicator.ShowMouseIndicator();
        tileIndicator.UpdateMouseIndicator();
        enableRemoval = true;
    }

    private void DisableRemoval()
    {
        tileIndicator.HideMouseIndicator();
        enableRemoval = false;
    }

    #endregion

    #region Input handling
    public void OnLeftPress(InputAction.CallbackContext context)
    {
        isLeftPress = (context.ReadValue<float>() >= 1) && !EventSystem.current.IsPointerOverGameObject();
    }

    public void OnRemovePress(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        tileIndicator.ChangeIndicatorMaterial();

        if (!enableRemoval) EnableRemoval();
        else DisableRemoval();
    }

    #endregion

    #region Building placement / removal
    private void PlaceBuilding()
    {
        // Get the tile position of the building to place
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());

        // Check if there is not already a building at the place we are trying to put the new building
        if (canBuildingBePlaced(tilePosition))
        {
            // Get the correct building from the database
            BuildingData buildingData = buildingDatabaseSO.buildingData[(int)buildingType - 1];

            BuildingTile occupiedTile = ScriptableObject.CreateInstance<BuildingTile>();

            Vector3 buildingPosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);
            GameObject go = Instantiate(buildingData.buildingPrefab, buildingPosition, tileIndicator.transform.rotation, buildingsMap.transform);

            occupiedTile.building = go.GetComponent<Building>();

            // Check the size of the building and update the tilemap accordingly
            // Case for (1, 1) buildings
            if (buildingData.size == new Vector2Int(1,1)) BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, occupiedTile);
            // Cases (1, 2) buildings :
            else if (buildingData.size == new Vector2Int(1, 2))
            {
                Vector3Int tempPos = tilePosition;
                BuildingTile occupiedTile2 = ScriptableObject.CreateInstance<BuildingTile>();
                occupiedTile2.name = buildingData.name + " (right)";
                occupiedTile.name = buildingData.name + " (left)";

                BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                switch (tileIndicator.transform.rotation.eulerAngles.y)
                {
                    case 0:  tempPos.x += 1; break; // When indicator faces +z -> (+2, +1) (x, y)
                    case 90: tempPos.y -= 1; break; // When indicator faces +x -> (+1, -2)
                    case 180:tempPos.x -= 1; break; // When indictor faces -z -> (-2, +1)
                    case 270:tempPos.y += 1; break; // When indicator faces -x -> (+1, +2)

                }

                GameObject filler = Instantiate(fillerPrefab, tempPos, tileIndicator.transform.rotation, buildingsMap.transform);
                occupiedTile2.building = filler.GetComponent<Building>();

                occupiedTile2.building.pair = occupiedTile.building;
                occupiedTile.building.pair = occupiedTile2.building;

                BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
            }

        }
        else
        {
            Debug.Log("Buildding can't be placed here");
        }
    }

    private static readonly HashSet<Building.BuildingType> oneByTwoBuildings = new HashSet<Building.BuildingType>
    {
        Building.BuildingType.Splitter,
        Building.BuildingType.Concatenator,
        Building.BuildingType.Exchangificator,
        Building.BuildingType.Troncator
    };

    private bool canBuildingBePlaced(Vector3Int tilePositionToCheck)
    {
        // Case for (1, 1) buildings
        if (buildingType == Building.BuildingType.None || BuildingManager.Instance.buildingTilemap.HasTile(tilePositionToCheck))  return false;

        // Cases for (1, 2) buildings
        else if (oneByTwoBuildings.Contains(buildingType))
        {
            Vector3Int tempPositon = tilePositionToCheck;

            switch (tileIndicator.transform.rotation.eulerAngles.y)
            {
                case 0:  tempPositon.x += 1; break;
                case 90: tempPositon.y -= 1; break;
                case 180:tempPositon.x -= 1; break;
                case 270:tempPositon.y += 1; break;
            }
            // Debug.Log(tempPositon);
            return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
        }
        return true;
    }

    private static readonly HashSet<Building.BuildingType> oneByTwoBuildingsName = new HashSet<Building.BuildingType>
    {
        Building.BuildingType.Splitter,
        Building.BuildingType.Concatenator,
        Building.BuildingType.Exchangificator,
        Building.BuildingType.Troncator,
    };

    private void RemoveBuilding()
    {
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
        BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(tilePosition);

        if (buildingTile == null) return;

        if (buildingTile.building.pair != null) 
        {
            Vector3Int fillerPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(buildingTile.building.pair.transform.position);
            BuildingTile buildingTile2 = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(fillerPosition);

            BuildingManager.Instance.buildingTilemap.SetTile(fillerPosition, null);
            Destroy(buildingTile2.building.gameObject);
        }

        BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, null);
        Destroy(buildingTile.building.gameObject);
        
    }

    #endregion

    private void Update()
    {
        buildingType = selectionUI.GetCurrentBuildingType();

        if (buildingType != Building.BuildingType.None)
        {
            if (!enableRemoval) EnablePlacement();
            else
            {
                DisablePlacement();
                EnableRemoval();
            }
        }
        else 
        {
            if (enableRemoval) EnableRemoval();
            else
            {
                DisablePlacement();
                DisableRemoval();
            }
        }

        if (isLeftPress && enablePlacement) PlaceBuilding(); 
        else if (isLeftPress && enableRemoval) RemoveBuilding();
    }
}
