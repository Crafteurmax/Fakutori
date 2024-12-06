using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public class TileIndicator : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private MeshRenderer indicatorMeshRenderer;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Material removeMaterial;

    [SerializeField] private LayerMask placementMask;
    [SerializeField] private Camera myCamera;

    private Vector3 lastPosition;

    public void UpdateMouseIndicator()
    {
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
        gameObject.transform.position = indicatorPosition;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            gameObject.transform.Rotate(0, 90, 0);
        }
    }

    public void ShowMouseIndicator()
    {
        gameObject.SetActive(true);
    }

    public void HideMouseIndicator()
    {
        gameObject.SetActive(false);
    }

    public void ChangeIndicatorMaterial()
    {
        if (indicatorMeshRenderer.material.name.Contains(previewMaterial.name))
        {
            indicatorMeshRenderer.material = removeMaterial;
        }
        else
        {
            indicatorMeshRenderer.material = previewMaterial;
        }
    }

    #region Getter

    public Vector3 getLastPosition()
    {
        return lastPosition;
    }

    #endregion
}
