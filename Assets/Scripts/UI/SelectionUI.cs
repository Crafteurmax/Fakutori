using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Building;

[System.Serializable]
public class BuildingCategory
{
    public Sprite sprite;
    public List<BuildingButton> buttons;
}

[System.Serializable]
public class BuildingButton
{
    public Building.BuildingType buildingType;
    public Sprite sprite;
}

[System.Serializable]
public class ButtonLayout
{
    public GridLayoutGroup layout;
    public RectTransform rectTransform;
    public Vector2 cellSize;
    public float spacing;
    public Vector2 padding;
}

public class SelectionUI : MonoBehaviour
{
    [SerializeField] private Building.BuildingType currentBuildingType = Building.BuildingType.None;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private BuildingPlacer buildingPlacer;

    [Header("Building Selection")]
    [SerializeField] private SelectableButton buildingButtonPrefab;
    [SerializeField] private SelectableButton categoryButtonPrefab;
    [SerializeField] private ButtonLayout categoryLayout;
    [SerializeField] private ButtonLayout buttonLayout;
    [SerializeField] private List<BuildingCategory> buildingCategories = new();

    private readonly List<List<SelectableButton>> buildingButtons = new();
    private readonly List<SelectableButton> categoryButtons = new();
    private Vector2Int currentBuilding = new(-1, -1);
    private int currentCategory = -1;

    public UnityEvent<Building.BuildingType> NewCurrentBuildingType { get; } = new();

    private void Start()
    {
        InitializeBuildingButtons();
    }

    #region Building Buttons
    private void InitializeBuildingButtons()
    {
        categoryLayout.layout.constraintCount = buildingCategories.Count;
        categoryLayout.layout.cellSize = categoryLayout.cellSize;
        categoryLayout.layout.spacing = new Vector2(categoryLayout.spacing, 0);
        categoryLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (categoryLayout.cellSize.x + categoryLayout.spacing) * categoryLayout.layout.constraintCount + 2 * categoryLayout.padding.x);
        categoryLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, categoryLayout.cellSize.y + 2 * categoryLayout.padding.y);

        buttonLayout.layout.cellSize = buttonLayout.cellSize;
        buttonLayout.layout.spacing = new Vector2(buttonLayout.spacing, 0);
        buttonLayout.rectTransform.localPosition = new Vector2(0, categoryLayout.rectTransform.sizeDelta.y);
        buttonLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonLayout.cellSize.y + 2 * categoryLayout.padding.y);
        buttonLayout.layout.gameObject.SetActive(false);

        foreach (BuildingCategory buildingCategory in buildingCategories)
        {
            CreateBuildingCategory(buildingCategory);
        }
    }

    private void CreateBuildingCategory(BuildingCategory buildingCategory)
    {
        if (buildingCategory.buttons.Count == 0) { return; }

        buildingButtons.Add(new());
        if (buildingCategory.buttons.Count == 1)
        {
            CreateBuildingButton(buildingCategory.buttons[0], buildingButtons.Count - 1, 0);
            buildingButtons[^1][0].transform.SetParent(categoryLayout.rectTransform);
            buildingButtons[^1][0].transform.localScale = Vector3.one;
            buildingButtons[^1][0].onClick.AddListener(delegate { CloseCurrentBuildingCategory(); });

            categoryButtons.Add(null);
        }
        else
        {
            for (int i = 0; i < buildingCategory.buttons.Count; i++)
            {
                CreateBuildingButton(buildingCategory.buttons[i], buildingButtons.Count - 1, i);
            }

            foreach (SelectableButton button in buildingButtons[^1])
            {
                button.transform.SetParent(buttonLayout.rectTransform);
                button.transform.localScale = Vector3.one;
                button.gameObject.SetActive(false);
            }

            CreateCategoryButton(buildingCategory, buildingButtons.Count);
        }
    }

    private void CreateCategoryButton(BuildingCategory buildingCategory, int index)
    {
        SelectableButton categoryButton = GameObject.Instantiate(categoryButtonPrefab);
        categoryButtons.Add(categoryButton);
        categoryButton.transform.SetParent(categoryLayout.rectTransform);
        categoryButton.transform.localScale = Vector3.one;

        categoryButton.SetIconSprite(buildingCategory.sprite);

        categoryButton.gameObject.GetComponent<BuildingButtonHover>().SetHoverText(string.Empty);

        categoryButton.onClick.AddListener(delegate { OpenBuildingCategory(index - 1); });
    }

    private void CreateBuildingButton(BuildingButton buildingButton, int categoryIndex, int buildingIndex)
    {
        SelectableButton button = GameObject.Instantiate(buildingButtonPrefab);
        buildingButtons[categoryIndex].Add(button);

        button.SetIconSprite(buildingButton.sprite);

        //Debug.Log(buildingButton.buildingType.ToString());
        button.gameObject.GetComponent<BuildingButtonHover>().SetHoverText(buildingButton.buildingType.ToString());

        button.onClick.AddListener(delegate { SetCurrentBuildingType(buildingButton.buildingType, new Vector2Int(categoryIndex, buildingIndex)); });
    }

    private void OpenBuildingCategory(int categoryIndex)
    {
        int tmp = currentCategory;
        CloseCurrentBuildingCategory();
        SetCurrentBuildingTypeToNone();

        if (categoryIndex == tmp) { return; }

        buttonLayout.layout.gameObject.SetActive(true);

        foreach (SelectableButton button in buildingButtons[categoryIndex])
        {
            button.gameObject.SetActive(true);
        }

        buttonLayout.layout.constraintCount = buildingButtons[categoryIndex].Count;
        buttonLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (buttonLayout.cellSize.x + buttonLayout.spacing) * buttonLayout.layout.constraintCount + 2 * buttonLayout.padding.x);

        buttonLayout.rectTransform.SetPositionAndRotation(
            new Vector2(categoryButtons[categoryIndex].transform.position.x, buttonLayout.rectTransform.position.y), 
            Quaternion.identity); 

        selectionPanel.tag = PanelManger.NoEscape;
        categoryButtons[categoryIndex].SelectButton(true);

        currentCategory = categoryIndex;
    }

    private void CloseCurrentBuildingCategory()
    {
        if (currentCategory < 0) { return; }

        buttonLayout.layout.gameObject.SetActive(false);

        foreach (SelectableButton button in buildingButtons[currentCategory])
        {
            button.gameObject.SetActive(false);
        }
        categoryButtons[currentCategory].SelectButton(false);

        selectionPanel.tag = PanelManger.Untagged;
        currentCategory = -1;
    }
    #endregion Building Buttons

    #region Current Building Type
    private void SetCurrentBuildingType(Building.BuildingType buildingType, Vector2Int buildingButtonIndex)
    {
        if (buildingType == Building.BuildingType.None || buildingButtonIndex == currentBuilding)
        {
            SetCurrentBuildingTypeToNone();
            return;
        }

        if (currentBuilding.x >= 0)
        {
            buildingButtons[currentBuilding.x][currentBuilding.y].SelectButton(false);
        }
        buildingButtons[buildingButtonIndex.x][buildingButtonIndex.y].SelectButton(true);
        currentBuilding = buildingButtonIndex;
        currentBuildingType = buildingType;

        selectionPanel.tag = PanelManger.NoEscape;

        NewCurrentBuildingType.Invoke(buildingType);
    }

    private void SetCurrentBuildingTypeToNone()
    {
        if (currentBuilding.x >= 0)
        {
            buildingButtons[currentBuilding.x][currentBuilding.y].SelectButton(false);
        }

        selectionPanel.tag = PanelManger.Untagged;
        currentBuilding = new(-1, -1);
        currentBuildingType = Building.BuildingType.None;
        NewCurrentBuildingType.Invoke(Building.BuildingType.None);
    }

    public void SetCurrentBuildingTypeToNone(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && !buildingPlacer.IsRemovalEnabled())
        {
            StartCoroutine(SetCurrentBuildingTypeToNoneDelayed());
        }
    }

    public IEnumerator SetCurrentBuildingTypeToNoneDelayed()
    {
        yield return new WaitForNextFrameUnit();

        SetCurrentBuildingTypeToNone();
        CloseCurrentBuildingCategory();
    }

    public Building.BuildingType GetCurrentBuildingType() { return currentBuildingType; }
    #endregion Current Building Type

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        categoryLayout.layout.constraintCount = buildingCategories.Count;
        categoryLayout.layout.cellSize = categoryLayout.cellSize;
        categoryLayout.layout.spacing = new Vector2(categoryLayout.spacing, 0);
        categoryLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (categoryLayout.cellSize.x + categoryLayout.spacing) * categoryLayout.layout.constraintCount + 2 * categoryLayout.padding.x);
        categoryLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, categoryLayout.cellSize.y + 2 * categoryLayout.padding.y);

        buttonLayout.layout.cellSize = buttonLayout.cellSize;
        buttonLayout.layout.spacing = new Vector2(buttonLayout.spacing, 0);
        buttonLayout.rectTransform.localPosition = new Vector2(0, categoryLayout.cellSize.y + 2 * categoryLayout.padding.y);
        buttonLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (buttonLayout.cellSize.x + buttonLayout.spacing) * buttonLayout.layout.constraintCount + 2 * buttonLayout.padding.x);
        buttonLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonLayout.cellSize.y + 2 * buttonLayout.padding.y);
    }
#endif
}
