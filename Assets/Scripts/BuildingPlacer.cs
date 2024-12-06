using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
        if (context.ReadValue<float>() >= 1)
        {   
            isLeftPress = true;
        }
        else
        {
            isLeftPress = false;
        }
    }

    public void OnRemovePress(InputAction.CallbackContext context)
    {
        if (context.performed && !enableRemoval)
        {
            tileIndicator.ChangeIndicatorMaterial();
            EnableRemoval();
        }
        else if (context.performed && enableRemoval)
        {
            tileIndicator.ChangeIndicatorMaterial();
            DisableRemoval();
        }
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

            Tile occupiedTile = new Tile();
            occupiedTile.name = buildingData.name;

            // Check the size of the building and update the tilemap accordingly
            // Case for (1, 1) buildings
            if (buildingData.size == new Vector2Int(1,1))
            {
                BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, occupiedTile);
            }

            // Cases (1, 2) buildings :
            else if (buildingData.size == new Vector2Int(1, 2))
            {
                Vector3Int tempPos = tilePosition;
                Tile occupiedTile2 = new Tile();
                occupiedTile2.name = buildingData.name + " (right)";
                occupiedTile.name = buildingData.name + " (left)";

                // When indicator faces +z -> (+2, +1) (x, y)
                if (tileIndicator.transform.rotation.eulerAngles.y == 0)
                {
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                    tempPos.x += 1;
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
                }
                // When indicator faces +x -> (+1, -2)
                else if (tileIndicator.transform.rotation.eulerAngles.y == 90)
                {
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                    tempPos.y -= 1;
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
                }
                // When indictor faces -z -> (-2, +1)
                else if (tileIndicator.transform.rotation.eulerAngles.y == 180)
                {
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                    tempPos.x -= 1;
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
                }
                // When indicator faces -x -> (+1, +2)
                else
                {
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                    tempPos.y += 1;
                    BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
                }
            }

            Vector3 buildingPosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);
            Instantiate(buildingData.buildingPrefab, buildingPosition, tileIndicator.transform.rotation, buildingsMap.transform);
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
        if (buildingType == Building.BuildingType.None || BuildingManager.Instance.buildingTilemap.HasTile(tilePositionToCheck))
        {
            return false;
        }
        // Cases for (1, 2) buildings
        else if (oneByTwoBuildings.Contains(buildingType))
        {
            Vector3Int tempPositon = tilePositionToCheck;

            if (tileIndicator.transform.rotation.eulerAngles.y == 0)
            {
                tempPositon.x += 1;
                return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
            }
            else if (tileIndicator.transform.rotation.eulerAngles.y == 90)
            {
                tempPositon.y -= 1;
                return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
            }
            else if (tileIndicator.transform.rotation.eulerAngles.y == 180)
            {
                tempPositon.x -= 1;
                return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
            }
            else
            {
                tempPositon.y += 1;
                return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
            }
        }
        return true;
    }

    private static readonly HashSet<string> oneByTwoBuildingsName = new HashSet<string>
    {
        "splitter (left)",
        "concatenator (left)",
        "exchangificator (left)",
        "troncator (left)",
        "splitter (right)",
        "concatenator (right)",
        "exchangificator (right)",
        "troncator (right)"
    };

    private void RemoveBuilding()
    {
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
        TileBase buildingTile = BuildingManager.Instance.buildingTilemap.GetTile(tilePosition);
        if (buildingTile != null)
        {
            // Case (1, 2) buildings
            if (oneByTwoBuildingsName.Contains(buildingTile.name))
            {
                Debug.Log("1x2 building to remove");
                // We have to check the orientation to know which tile to delete from the tile map

                // We first get the actual building gameObject
                Vector3 removePosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);
                Collider[] buildingToRemove = Physics.OverlapSphere(removePosition, 0.2f, buildingMask);

                if (buildingToRemove.Length > 0)
                {
                    foreach(var building  in buildingToRemove)
                    {
                        Vector3Int tempPos = tilePosition;

                        if ((building.transform.parent.rotation.eulerAngles.y == 0 && buildingTile.name.Contains("(left)")) || (building.transform.parent.rotation.eulerAngles.y == 180 && buildingTile.name.Contains("(right)")))
                        {
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                            tempPos.x += 1;
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                        }
                        else if ((building.transform.parent.rotation.eulerAngles.y == 90 && buildingTile.name.Contains("(left)")) || (building.transform.parent.rotation.eulerAngles.y == 270 && buildingTile.name.Contains("(right)")))
                        {
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                            tempPos.y -= 1;
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                        }
                        else if ((building.transform.parent.rotation.eulerAngles.y == 180 && buildingTile.name.Contains("(left)")) || (building.transform.parent.rotation.eulerAngles.y == 0 && buildingTile.name.Contains("(right)")))
                        {
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                            tempPos.x -= 1;
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                        }
                        else if ((building.transform.parent.rotation.eulerAngles.y == 270 && buildingTile.name.Contains("(left)")) || (building.transform.parent.rotation.eulerAngles.y == 90 && buildingTile.name.Contains("(right)")))
                        {
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                            tempPos.y += 1;
                            BuildingManager.Instance.buildingTilemap.SetTile(tempPos, null);
                        }

                        Destroy(building.transform.parent.gameObject);
                    }
                }

            }
            else
            {
                Debug.Log("1x1 building to remove");
                BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, null);
                Vector3 removePosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);
                Collider[] buildingToRemove = Physics.OverlapSphere(removePosition, 0.2f, buildingMask);

                if (buildingToRemove.Length > 0) 
                { 
                    foreach(var building in buildingToRemove)
                    {
                        Destroy(building.transform.parent.gameObject);
                    }
                }
            }
        }

        //tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());

        //if (BuildingManager.Instance.buildingTilemap.HasTile(tilePosition))
        //{
        //    Debug.Log("Building removed");
        //    BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, null);

        //    Vector3 removePosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);

        //    Collider[] buildingToRemove = Physics.OverlapSphere(removePosition, 0.2f, buildingMask);

        //    if (buildingToRemove.Length > 0)
        //    {
        //        foreach (var collider in buildingToRemove)
        //        { 
        //            Transform parent = collider.transform.parent;
        //            Debug.Log("Collier hit position:" + collider.gameObject.transform.position + "\nParent position:" + parent.transform.position);
        //            Destroy(parent.gameObject);
        //        }
        //    }
        //}

        //RaycastHit hit;
        //tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
        //Vector3 rayOrigin = tilePosition;
        //rayOrigin.z +=  rayOrigin.y + 0.5f;
        //rayOrigin.x += 0.5f;
        //rayOrigin.y = 2f;
        //Debug.Log(rayOrigin);
        //Debug.DrawRay(rayOrigin, Vector3.down * 3, Color.red, 3);
        //if (Physics.Raycast(tilePosition, Vector3.down, out hit, 3f, buildingMask, QueryTriggerInteraction.Collide)) 
        //{
        //    Debug.Log("Hit !");
        //}
        //Debug.Log(hit.collider);
    }

    #endregion

    private void Update()
    {
        buildingType = selectionUI.GetCurrentBuildingType();
        if (buildingType != Building.BuildingType.None && !enableRemoval)
        {
            EnablePlacement();
        }
        else if (buildingType != Building.BuildingType.None && enableRemoval)
        {
            DisablePlacement();
            EnableRemoval();
        }
        else if (buildingType == Building.BuildingType.None && enableRemoval)
        {
            EnableRemoval();
        }
        else
        {
            DisablePlacement();
            DisableRemoval();
        }

        if (isLeftPress && enablePlacement)
        {
            PlaceBuilding();
        }
        else if (isLeftPress && enableRemoval)
        {
            RemoveBuilding();
        }

        //if (buildingType != Building.BuildingType.None || isRemovingBuilding)
        //{
        //    tileIndicator.ShowMouseIndicator();
        //    tileIndicator.MouseIndicator();
        //}
        //else
        //{
        //    tileIndicator.HideMouseIndicator();
        //}

        //if (isLeftPress && !isRemovingBuilding)
        //{
        //    PlaceBuilding();
        //}
        //else if (isLeftPress && isRemovingBuilding)
        //{
        //    RemoveBuilding();
        //}
    }
}
