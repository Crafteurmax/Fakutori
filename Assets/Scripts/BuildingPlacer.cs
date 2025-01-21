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
    [SerializeField] private GameObject selectionPanel;

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
    
    // Logic to know in which state we are
    private bool enablePlacement = false;
    private bool enableRemoval = false;
    private bool isLeftPress = false;

    //Initialize an instance of History with the building placer
    private void Start()
    {
        History.Instance.Initialize(this);
    }

    private void OnEnable()
    {
        selectionUI.NewCurrentBuildingType.AddListener(NewBuildingSelected);
    }

    private void OnDisable()
    {
        selectionUI.NewCurrentBuildingType.RemoveListener(NewBuildingSelected);
    }

    private void NewBuildingSelected(Building.BuildingType buildingType)
    {
        tileIndicator.ChangeIndicator(buildingType);
    }

    #region Enable / disable placement
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
        tileIndicator.RemoveIndicator();
        if (selectionPanel != null) { selectionPanel.tag = PanelManger.NoEscape; }
        enableRemoval = true;
    }

    private void DisableRemoval()
    {
        tileIndicator.HideMouseIndicator();
        if (selectionPanel != null) { selectionPanel.tag = PanelManger.Untagged; }
        enableRemoval = false;
    }

    public void DisableRemoval(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            StartCoroutine(DisableRemovalDelayed());
        }
    }

    public IEnumerator DisableRemovalDelayed()
    {
        yield return new WaitForNextFrameUnit();

        DisableRemoval();
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

        if (!enableRemoval)
        {
            EnableRemoval();
            tileIndicator.ChangeIndicator(Building.BuildingType.None);
        } 
        else DisableRemoval();
    }

    public void OnUndoPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            History.Instance.Undo();
        }
    }

    public void OnRedoPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            History.Instance.Redo();
        }
    }

    #endregion

    #region Building placement / removal

    public bool PlaceBuildingAtPosition(Building.BuildingType aBuildingType, Vector3Int aTilePosition, Quaternion aBuildingRotation)
    {
        // Get the correct building from the database
        BuildingData buildingData = buildingDatabaseSO.buildingData[(int)aBuildingType - 1];
        
        // Check if there is not already a building at the place we are trying to put the new building
        if (CanBuildingBePlaced(aTilePosition, aBuildingType, aBuildingRotation))
        {
            BuildingTile occupiedTile = ScriptableObject.CreateInstance<BuildingTile>();

            Vector3 buildingPosition = new(aTilePosition.x + 0.5f, 0f, aTilePosition.y + 0.5f);
            GameObject go = Instantiate(buildingData.buildingPrefab, buildingPosition, aBuildingRotation, buildingsMap.transform);
            occupiedTile.building = go.GetComponent<Building>();

            // Check the size of the building and update the tilemap accordingly
            // Case for (1, 1) buildings
            if (buildingData.size == new Vector2Int(1, 1)) 
                BuildingManager.Instance.buildingTilemap.SetTile(aTilePosition, occupiedTile);
            // Cases (1, 2) buildings :
            else if (buildingData.size == new Vector2Int(1, 2))
            {
                Vector3Int tempPos = aTilePosition;
                BuildingTile occupiedTile2 = ScriptableObject.CreateInstance<BuildingTile>();

                BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile);
                switch (aBuildingRotation.eulerAngles.y)
                {
                    case 0: tempPos.x += 1; break; // When indicator faces +z -> (+2, +1) (x, y)
                    case 90: tempPos.y -= 1; break; // When indicator faces +x -> (+1, -2)
                    case 180: tempPos.x -= 1; break; // When indictor faces -z -> (-2, +1)
                    case 270: tempPos.y += 1; break; // When indicator faces -x -> (+1, +2)
                }

                Vector3 fillerPosition = new(tempPos.x + 0.5f, 0f, tempPos.y + 0.5f);
                GameObject filler = Instantiate(fillerPrefab, fillerPosition, aBuildingRotation, buildingsMap.transform);
                occupiedTile2.building = filler.GetComponent<Building>();

                occupiedTile2.building.pair = occupiedTile.building;
                occupiedTile.building.pair = occupiedTile2.building;

                BuildingManager.Instance.buildingTilemap.SetTile(tempPos, occupiedTile2);
            }

            return true;
        }
        else
        {
            UnityEngine.Debug.Log("Buildding of type " + aBuildingType + " can't be placed here");
            return false;
        }
    }

    private static readonly HashSet<Building.BuildingType> oneByTwoBuildings = new ()
    {
        Building.BuildingType.Splitter,
        Building.BuildingType.Concatenator,
        Building.BuildingType.Exchangificator,
        Building.BuildingType.Troncator
    };

    private bool CanBuildingBePlaced(Vector3Int tilePositionToCheck, Building.BuildingType aBuildingType, Quaternion aBuildingRotation)
    {
        // Case for (1, 1) buildings
        if (aBuildingType == Building.BuildingType.None || BuildingManager.Instance.buildingTilemap.HasTile(tilePositionToCheck)) return false;

        // Cases for (1, 2) buildings
        else if (oneByTwoBuildings.Contains(aBuildingType))
        {
            Vector3Int tempPositon = tilePositionToCheck;

            switch (aBuildingRotation.eulerAngles.y)
            {
                case 0:  tempPositon.x += 1; break;
                case 90: tempPositon.y -= 1; break;
                case 180:tempPositon.x -= 1; break;
                case 270:tempPositon.y += 1; break;
            }

            BuildingTile tile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(tempPositon);
            if(tile) UnityEngine.Debug.Log(tempPositon + " " + tile.building);
         
            return !BuildingManager.Instance.buildingTilemap.HasTile(tempPositon);
        }
        return true;
    }

    private static readonly HashSet<Building.BuildingType> oneByTwoBuildingsName = new ()
    {
        Building.BuildingType.Splitter,
        Building.BuildingType.Concatenator,
        Building.BuildingType.Exchangificator,
        Building.BuildingType.Troncator,
    };

    public void RemoveBuildingAtPosition(Vector3Int aTilePosition)
    {
        BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(aTilePosition);

        if (buildingTile == null) return;

        if (buildingTile.building.pair != null)
        {
            Vector3Int fillerPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(buildingTile.building.pair.transform.position);
            buildingTile.building.pair = null;
            RemoveBuildingAtPosition(fillerPosition);
        }

        //buildingTile.building;
        BuildingManager.Instance.buildingTilemap.SetTile(aTilePosition, null);
        BuildingManager.Instance.RemoveBuildingInput(aTilePosition);
        buildingTile.building.Release();
        Destroy(buildingTile.building.gameObject);
    }

    #endregion

    #region Undo/Redo

    public void Undo()
    {
        History.Instance.Undo();
    }

    public void Redo()
    {
        History.Instance.Redo();
    }
    #endregion

    public bool IsRemovalEnabled()
    {
        return enableRemoval;
    }

    private void Update()
    {
        //buildingType = selectionUI.GetCurrentBuildingType();
        //tileIndicator.ChangeIndicator(buildingType);

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

        if (isLeftPress && enablePlacement)
        {
            Vector3Int tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
            if (PlaceBuildingAtPosition(buildingType, tilePosition, tileIndicator.transform.rotation))
                History.Instance.AddToHistory(buildingType, tilePosition, tileIndicator.transform.rotation, true);
        }
        else if (isLeftPress && enableRemoval)
        {
            Vector3Int tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
            BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(tilePosition);
            if (buildingTile != null)
            {
                Building.BuildingType removedBuildingType = buildingTile.building.GetBuildingType();
                Quaternion removedBuildingRotation = buildingTile.building.transform.rotation;
                RemoveBuildingAtPosition(tilePosition);
                History.Instance.AddToHistory(removedBuildingType, tilePosition, removedBuildingRotation, false);
            }
        }
    }
}
