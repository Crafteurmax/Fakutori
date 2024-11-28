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
    [SerializeField] private GameObject tileIndicator;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Material removeMaterial;

    [Header("Components")]
    //[SerializeField] private Tilemap map;
    [SerializeField] private GameObject buildingsMap;
    [SerializeField] private Camera myCamera;
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private SelectionUI selectionUI;

    [Header("Buildings")]
    [SerializeField] private Building.BuildingType buildingType;
    [SerializeField] private List<GameObject> buildings;

    private Vector3 lastPosition;
    private Vector3Int tilePosition;
    private bool isLeftPress = false;
    private bool isRemovingBuilding = false;

    private Tile occupiedTile;

    private void Start()
    {
        buildingType = Building.BuildingType.None;
        occupiedTile = new Tile();
    }

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
        if (context.performed && !isRemovingBuilding)
        {
            ChangeIndicatorMaterial(removeMaterial);
            isRemovingBuilding = true;
        }
        else if (context.performed && isRemovingBuilding)
        {
            ChangeIndicatorMaterial(previewMaterial);
            isRemovingBuilding = false;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tileIndicator.transform.Rotate(0, 90, 0);
        }       
    }

    private void MouseIndicator()
    {
        tileIndicator.SetActive(true);

        Vector3 mousPosition = Input.mousePosition;
        mousPosition.z = myCamera.nearClipPlane;
        Ray ray = myCamera.ScreenPointToRay(mousPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, placementMask))
        {
            lastPosition = hit.point;
        }

        Vector3 currentPos = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
        Vector3 indicatorPosition = new Vector3(currentPos.x + 0.5f, 0f, currentPos.y + 0.5f);
        tileIndicator.transform.position = indicatorPosition;
    }

    private void HideMouseIndicator()
    {
        tileIndicator.gameObject.SetActive(false);
    }

    private void ChangeIndicatorMaterial(Material newMaterial)
    {
        tileIndicator.GetComponent<MeshRenderer>().material = newMaterial;
    }

    private void PlaceBuilding()
    {
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);

        if (buildingType != Building.BuildingType.None && !BuildingManager.Instance.buildingTilemap.HasTile(tilePosition))
        {
            BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, occupiedTile);
            Vector3 buildingPosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);
            Instantiate(buildings[(int)buildingType], buildingPosition, tileIndicator.transform.rotation, buildingsMap.transform);
        }
    }

    private void RemoveBuilding()
    {
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);

        if (BuildingManager.Instance.buildingTilemap.HasTile(tilePosition))
        {
            Debug.Log("Building removed");
            BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, null);

            Vector3 removePosition = new Vector3(tilePosition.x + 0.5f, 0f, tilePosition.y + 0.5f);

            Collider[] buildingToRemove = Physics.OverlapSphere(removePosition, 0.2f, buildingMask);

            if (buildingToRemove.Length > 0)
            {
                foreach (var collider in buildingToRemove)
                { 
                    Transform parent = collider.transform.parent;
                    Destroy(parent.gameObject);
                }
            }
        }
    }

    private void Update()
    {
        buildingType = selectionUI.GetCurrentBuildingType();        
        if (buildingType != Building.BuildingType.None || isRemovingBuilding)
        {
            MouseIndicator();
        }
        else
        {
            HideMouseIndicator();
        }

        if (isLeftPress && !isRemovingBuilding)
        {
            PlaceBuilding();
        }
        else if (isLeftPress && isRemovingBuilding)
        {
            RemoveBuilding();
        }
    }
}
