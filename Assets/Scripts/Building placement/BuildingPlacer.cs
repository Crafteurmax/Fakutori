using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    [Header("Selection plane")]
    [SerializeField] private GameObject selectionPlane;

    [Header("Other stuff")]
    [SerializeField] private GameObject fillerPrefab;
    [SerializeField] private LayerMask placementMask;


    // Logic to know in which state we are
    private bool enablePlacement = false;
    private bool enableRemoval = false;
    private bool isLeftPress = false;
    private bool isMultiSelectionHappening = false;

    private bool isLastTogglePlacement = false;

    private Vector3Int firstPosition = Vector3Int.zero;
    private Vector3Int secondPosition = Vector3Int.zero;

    private readonly List<Tuple<Vector3Int, BuildingTile>> selectedBuildings = new();

    #region Start
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
    #endregion

    #region Event listner
    private void NewBuildingSelected(Building.BuildingType buildingType)
    {
        tileIndicator.ChangeIndicator(buildingType);
        this.buildingType = buildingType;

        if (buildingType != Building.BuildingType.None)
        {
            EnablePlacement();
            DeselectBuildings();

            if (enableRemoval)
            {
                DisableRemoval();
            }
            return;
        }
        else
        {
            DisablePlacement();
        }
    }
    #endregion

    #region Enable / disable placement
    private void EnablePlacement()
    {
        tileIndicator.ShowMouseIndicator();
        enablePlacement = true;
        isLastTogglePlacement = true;
    }

    public void DisablePlacement()
    {
        buildingType = Building.BuildingType.None;
        tileIndicator.HideMouseIndicator();
        enablePlacement = false;
        if (enableRemoval)
        {
            isLastTogglePlacement = false;
        }
    }

    private void EnableRemoval()
    {
        tileIndicator.ShowMouseIndicator();
        tileIndicator.RemoveIndicator();
        enableRemoval = true;
        isLastTogglePlacement = false;
    }

    public void DisableRemoval()
    {
        //Debug.Log("Enable placement: " + enablePlacement);
        tileIndicator.HideMouseIndicator();
        enableRemoval = false;
        if (enablePlacement)
        {
            tileIndicator.ShowMouseIndicator();
            tileIndicator.ChangeIndicator(buildingType);
            isLastTogglePlacement = true;
        }
    }
    #endregion

    #region Input handling
    public void OnLeftPress(InputAction.CallbackContext context)
    {
        isLeftPress = (context.ReadValue<float>() >= 1) && !EventSystem.current.IsPointerOverGameObject();
    }

    public void OnMultiSelection(InputAction.CallbackContext context)
    {
        if (!enablePlacement && !enableRemoval)
        {
            Vector3 lastPosition = Vector3Int.zero;

            Vector3 mousPosition = Input.mousePosition;
            mousPosition.z = Camera.main.nearClipPlane;
            Ray ray = Camera.main.ScreenPointToRay(mousPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, placementMask))
            {
                lastPosition = hit.point;
            }

            if (context.started)
            {
                firstPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
                isMultiSelectionHappening = true;
                selectionPlane.SetActive(true);
                //Debug.Log("First position: " + firstPosition);
            }

            if (context.canceled)
            {
                secondPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
                isMultiSelectionHappening = false;
                selectionPlane.SetActive(false);
                //Debug.Log("Second position: " + secondPosition);
                GetBuildingInSelection();
            }
        }
    }

    public void OnRemovePress(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        if (!enableRemoval && selectedBuildings.Count == 0 && !selectionPlane.activeSelf)
        {
            EnableRemoval();
            tileIndicator.ChangeIndicator(Building.BuildingType.None);
        }
        else if (!enableRemoval && selectedBuildings.Count > 0)
        {
            MultiDeletPress();
        }
        else if (enableRemoval)
        {
            DisableRemoval();
        }
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

    public bool DoActionOnEscape()
    {
        return enableRemoval || selectedBuildings.Count != 0;
    }

    public void OnEscapePress(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started) { return; }

        if (selectedBuildings.Count == 0)
        {
            DisableRemoval();
        }
        else
        {
            DeselectBuildings();
        }
    }

    public void OnPipettePress(InputAction.CallbackContext context)
    {
        //if (enablePlacement && enableRemoval && context.performed)
        //{
        //    DisableRemoval();
        //    SelectPipetteBuilding();
        //    return;
        //}

        //if (enableRemoval && context.performed)
        //{
        //    DisableRemoval();
        //    SelectPipetteBuilding();
        //    return;
        //}

        //if (enablePlacement && context.performed)
        //{
        //    SelectPipetteBuilding();
        //    return;
        //}

        //if (!enablePlacement && !enableRemoval && context.performed)
        //{
        //    SelectPipetteBuilding();
        //    return;
        //}

        if (context.phase != InputActionPhase.Started)
        {
            return;
        }

        if (buildingType != Building.BuildingType.None)
        {
            selectionUI.SetCurrentBuildingTypeToNone(context);
            DisablePlacement();
        }

        else if (buildingType == Building.BuildingType.None && !enableRemoval)
        {
            SelectPipetteBuilding();
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

    #region Multiple building selection / removal
    public void GetBuildingInSelection()
    {
        if (firstPosition != Vector3Int.zero && secondPosition != Vector3Int.zero)
        {
            for (int i = Mathf.Min(firstPosition.x, secondPosition.x); i <= Mathf.Max(firstPosition.x, secondPosition.x); i++)
            {
                for (int j = Mathf.Min(firstPosition.y, secondPosition.y); j <= Mathf.Max(firstPosition.y, secondPosition.y); j++)
                {
                    Vector3Int currentCoords = new(i, j, 0);
                    BuildingTile currentTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(currentCoords);
                    if (currentTile != null)
                    {
                        if (currentTile.building.GetBuildingType() != Building.BuildingType.Filler)
                        {
                            //Debug.Log("Building: " + currentTile.building.GetType().ToString());
                            selectedBuildings.Add(Tuple.Create(currentCoords, currentTile));
                            currentTile.building.GetComponent<Outline>().OutlineColor = Color.red;
                        }
                    }
                }
            }
        }
    }

    public void DeselectBuildings()
    {
        for (int i = 0; i < selectedBuildings.Count; i++)
        {
            selectedBuildings[i].Item2.building.GetComponent<Outline>().OutlineColor = Color.clear;
        }
        selectedBuildings.Clear();
    }

    public void MultiDeletPress()
    {
        List<History.buildingAction> actionList = new();

        foreach (var building in selectedBuildings)
        {
            RemoveBuildingAtPosition(building.Item1);
            if ((int)building.Item2.building.GetBuildingType() >= 8 && (int)building.Item2.building.GetBuildingType() < 17)
            {
                Dictionary<List<string>, List<Item.Symbol>> cache = building.Item2.building.GetComponent<Factory>().GetFactoryCache();
                History.buildingAction newAction = new(building.Item2.building.GetBuildingType(), building.Item1, building.Item2.building.transform.rotation, false, cache);
                actionList.Add(newAction);
            }
            else
            {
                History.buildingAction newAction = new(building.Item2.building.GetBuildingType(), building.Item1, building.Item2.building.transform.rotation, false);
                actionList.Add(newAction);
            }
        }
        History.Instance.AddListToHistory(actionList);
        selectedBuildings.Clear();
    }
    #endregion

    public bool IsRemovalEnabled()
    {
        return enableRemoval;
    }

    public bool IsSelectionEmpty()
    {
        return selectedBuildings.Count == 0;
    }

    private void SelectPipetteBuilding()
    {
        Vector3 mousPosition = Input.mousePosition;
        mousPosition.z = Camera.main.nearClipPlane;
        Ray ray = Camera.main.ScreenPointToRay(mousPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000, placementMask))
        {
            Vector3 lastPosition = hit.point;

            Vector3Int buildingPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
            BuildingTile tile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(buildingPosition);

            if (tile != null)
            {
                Building.BuildingType pipetteType = Building.BuildingType.None;

                if (tile.building.GetBuildingType() == Building.BuildingType.Filler)
                {
                    pipetteType = tile.building.pair.GetBuildingType();
                }
                else
                {
                    pipetteType = tile.building.GetBuildingType();
                }

                if (pipetteType == buildingType)
                {
                    return;
                }

                Vector2Int buildingButtonPair = Vector2Int.zero;

                for (int i = 0; i < selectionUI.buildingCategories.Count; i++)
                {
                    for (int j = 0; j < selectionUI.buildingCategories[i].buttons.Count; j++)
                    {
                        if (selectionUI.buildingCategories[i].buttons[j].buildingType == pipetteType)
                        {
                            buildingButtonPair.x = i;
                            buildingButtonPair.y = j;
                            break;
                        }
                    }
                }

                selectionUI.SetCurrentBuildingType(pipetteType, buildingButtonPair);
            }
        }
    }

    private void Update()
    {
        //buildingType = selectionUI.GetCurrentBuildingType();
        //tileIndicator.ChangeIndicator(buildingType);

        //if (buildingType != Building.BuildingType.None)
        //{
        //    if (!enableRemoval) EnablePlacement();
        //    else
        //    {
        //        DisablePlacement();
        //        EnableRemoval();
        //    }
        //}
        //else 
        //{
        //    if (enableRemoval) EnableRemoval();
        //    else
        //    {
        //        DisablePlacement();
        //        DisableRemoval();
        //    }
        //}


        if (enableRemoval || enablePlacement)
        {
            tileIndicator.UpdateMouseIndicator();
        }

        if (isLeftPress && enableRemoval && !isLastTogglePlacement)
        {
            Vector3Int tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
            BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(tilePosition);
            if (buildingTile != null)
            {
                Building.BuildingType removedBuildingType = buildingTile.building.GetBuildingType();
                Quaternion removedBuildingRotation = buildingTile.building.transform.rotation;

                if ((int)removedBuildingType >= 8 &&  (int)removedBuildingType < 17)
                {
                    Dictionary<List<string>, List<Item.Symbol>> cache = buildingTile.building.GetComponent<Factory>().GetFactoryCache();
                    History.Instance.AddToHistory(removedBuildingType, tilePosition, removedBuildingRotation, false, cache); 
                }
                else
                {
                    History.Instance.AddToHistory(removedBuildingType, tilePosition, removedBuildingRotation, false);
                }
                RemoveBuildingAtPosition(tilePosition);
            }
        }
        else if(isLeftPress && enablePlacement && isLastTogglePlacement)
        {
            Vector3Int tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(tileIndicator.getLastPosition());
            if (PlaceBuildingAtPosition(buildingType, tilePosition, tileIndicator.transform.rotation))
                History.Instance.AddToHistory(buildingType, tilePosition, tileIndicator.transform.rotation, true);
        }

        if (isMultiSelectionHappening)
        {
            UpdateSelectionPlane();
        }


        //Debug.Log("Current placement state: " + enablePlacement +"\nCurrent removal state: " + enableRemoval + "\nSelection UI tag: " + selectionUI.tag);
    }

    private void UpdateSelectionPlane()
    {
        #region Get current mouse position
        Vector3Int secondPosition = Vector3Int.zero;

        Vector3 mousPosition = Input.mousePosition;
        mousPosition.z = Camera.main.nearClipPlane;
        Ray ray = Camera.main.ScreenPointToRay(mousPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000, placementMask))
        {
            Vector3 lastPosition = hit.point;

            secondPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
        }
        #endregion

        float planeLength = Mathf.Abs(firstPosition.x - secondPosition.x) + 1; // x
        float planeWidth = Mathf.Abs(firstPosition.y - secondPosition.y) + 1; // y

        //Debug.Log("L: " + planeLength + " W: " + planeWidth);

        Vector3 centerPoint = Vector3.zero;

        centerPoint.x = (float)(firstPosition.x + secondPosition.x) / 2;
        centerPoint.z = (float)(firstPosition.y + secondPosition.y) / 2;

        centerPoint.x += 0.5f;
        centerPoint.z += 0.5f;

        selectionPlane.transform.localScale = new Vector3(planeLength * 0.1f, 1, planeWidth * 0.1f);

        selectionPlane.transform.position = centerPoint;
    }
}
