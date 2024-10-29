using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class BuildingButtonInformation
{
    public Building.BuildingType buildingType;
    public Sprite sprite;
}

public class SelectionUI : MonoBehaviour
{
    [SerializeField] private Building.BuildingType currentBuildingType = Building.BuildingType.None;

    [Header("Buttons")]
    [SerializeField] private GridLayoutGroup buttonsContainer;
    [SerializeField] private MultiLayerButton buildingButtonPrefab;
    [SerializeField] private List<BuildingButtonInformation> buttonInformationList = new();
    private readonly List<MultiLayerButton> buildingButtons = new();

    [Header("Description")]
    [SerializeField] private Image desciptionImage;

    [Header("Controls")]
    [SerializeField] private Image controlsImage;

    [Header("Objectifs")]
    [SerializeField] private Image objectifsImage;

    public UnityEvent newCurrentBuildingType { get; } = new();



    private void Start()
    {
        buttonsContainer.constraintCount = buttonInformationList.Count;
        InitializeBuildingButtons();
    }

    private void InitializeBuildingButtons()
    {
        foreach(BuildingButtonInformation buttonInformation in buttonInformationList)
        {
            CreateBuildingButtons(buttonInformation);
        }
    }

    private void CreateBuildingButtons(BuildingButtonInformation buttonInformation)
    {
        MultiLayerButton buildingButton = GameObject.Instantiate(buildingButtonPrefab);
        buildingButton.transform.SetParent(buttonsContainer.transform);

        buildingButton.SetIconSprite(buttonInformation.sprite);

        buildingButton.onClick.AddListener(delegate { SetCurrentBuildingType(buttonInformation.buildingType); });

        buildingButtons.Add(buildingButton);
    }

    public void SetBuildingButtonInteractable(Building.BuildingType buildingType, bool interactable)
    {
        for (int i = 0; i < buttonInformationList.Count; i++)
        {
            if (buttonInformationList[i].buildingType == buildingType)
            {
                buildingButtons[i].interactable = interactable;
            }
        }
    }

    #region Current Building Type
    private void SetCurrentBuildingType(Building.BuildingType buildingType)
    {
        if (currentBuildingType != buildingType)
        {
            currentBuildingType = buildingType;
            newCurrentBuildingType.Invoke();
        }
    }

    public void SetCurrentBuildingTypeToNone(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            SetCurrentBuildingType(Building.BuildingType.None);
        }
    }

    public Building.BuildingType GetCurrentBuildingType() { return currentBuildingType; }
    #endregion Current Building Type
}
