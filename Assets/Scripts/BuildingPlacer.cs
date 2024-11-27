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

        Vector3 indicatorPosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);
        indicatorPosition.z = indicatorPosition.y;
        indicatorPosition.y = 0;
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
            tilePosition.z = tilePosition.y;
            tilePosition.y = 0;
            Instantiate(buildings[(int)buildingType], tilePosition, tileIndicator.transform.rotation);
        }
    }

    private void RemoveBuilding()
    {
        tilePosition = BuildingManager.Instance.buildingTilemap.WorldToCell(lastPosition);

        if (BuildingManager.Instance.buildingTilemap.HasTile(tilePosition))
        {
            Debug.Log("Building removed");
            BuildingManager.Instance.buildingTilemap.SetTile(tilePosition, null);

            tilePosition.z = tilePosition.y;
            tilePosition.y = 0;

            Collider[] buildingToRemove = Physics.OverlapSphere(tilePosition, 0.2f, buildingMask);

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
