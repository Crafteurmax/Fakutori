using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Building Selection")]
    [SerializeField] private MultiLayerButton buttonPrefab;
    [SerializeField] private ButtonLayout categoryLayout;
    [SerializeField] private ButtonLayout buttonLayout;
    [SerializeField] private List<BuildingCategory> buildingCategories = new();
    [SerializeField] private Color selectedColor;
    [SerializeField] private float colorMultiplier;

    private readonly List<List<MultiLayerButton>> buildingButtons = new();
    private readonly List<MultiLayerButton> categoryButtons = new();
    private int currentCategory = -1;

    [Header("Description")]
    [SerializeField] private Image desciptionImage;

    [Header("Controls")]
    [SerializeField] private Image controlsImage;

    [Header("Objectifs")]
    [SerializeField] private Image objectifsImage;

    public UnityEvent NewCurrentBuildingType { get; } = new();

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
            CreateBuildingButton(buildingCategory.buttons[0], buildingButtons.Count - 1);
            buildingButtons[^1][0].transform.SetParent(categoryLayout.rectTransform);
            buildingButtons[^1][0].onClick.AddListener(delegate { CloseCurrentBuildingCategory(); });

            categoryButtons.Add(new());
        }
        else
        {
            foreach (BuildingButton buildingButton in buildingCategory.buttons)
            {
                CreateBuildingButton(buildingButton, buildingButtons.Count - 1);
            }

            foreach (MultiLayerButton button in buildingButtons[^1])
            {
                button.transform.SetParent(buttonLayout.rectTransform);
                button.gameObject.SetActive(false);
            }

            CreateCategoryButton(buildingCategory, buildingButtons.Count);
        }
    }

    private void CreateCategoryButton(BuildingCategory buildingCategory, int index)
    {
        MultiLayerButton categoryButton = GameObject.Instantiate(buttonPrefab);
        categoryButtons.Add(categoryButton);
        categoryButton.transform.SetParent(categoryLayout.rectTransform);

        categoryButton.SetIconSprite(buildingCategory.sprite);

        categoryButton.onClick.AddListener(delegate { OpenBuildingCategory(index - 1); });
    }

    private void CreateBuildingButton(BuildingButton buildingButton, int categoryIndex)
    {
        MultiLayerButton button = GameObject.Instantiate(buttonPrefab);
        buildingButtons[categoryIndex].Add(button);

        button.SetIconSprite(buildingButton.sprite);

        button.onClick.AddListener(delegate { SetCurrentBuildingType(buildingButton.buildingType); });
    }

    private void OpenBuildingCategory(int categoryIndex)
    {
        if (currentCategory == categoryIndex)
        {
            CloseCurrentBuildingCategory();
            SetCurrentBuildingType(Building.BuildingType.None);
        }
        else
        {
            buttonLayout.layout.gameObject.SetActive(true);

            if (currentCategory > 0)
            {
                foreach (MultiLayerButton button in buildingButtons[currentCategory])
                {
                    button.gameObject.SetActive(false);
                }
            }

            foreach (MultiLayerButton button in buildingButtons[categoryIndex])
            {
                button.gameObject.SetActive(true);
            }

            buttonLayout.layout.constraintCount = buildingButtons[categoryIndex].Count;
            buttonLayout.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (buttonLayout.cellSize.x + buttonLayout.spacing) * buttonLayout.layout.constraintCount + 2 * buttonLayout.padding.x);

            SelectButton(categoryButtons[categoryIndex]);
            currentCategory = categoryIndex;
        }
    }

    private void CloseCurrentBuildingCategory()
    {
        if (currentCategory < 0) { return; }

        buttonLayout.layout.gameObject.SetActive(false);

        foreach (MultiLayerButton button in buildingButtons[currentCategory])
        {
            button.gameObject.SetActive(false);
        }
        DeselectButton(categoryButtons[currentCategory]);

        currentCategory = -1;
    }
    #endregion Building Buttons

    #region Current Building Type
    private void SetCurrentBuildingType(Building.BuildingType buildingType)
    {
        if (currentBuildingType != buildingType)
        {
            currentBuildingType = buildingType;
            NewCurrentBuildingType.Invoke();
        }
    }

    public void SetCurrentBuildingTypeToNone(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            SetCurrentBuildingType(Building.BuildingType.None);
            CloseCurrentBuildingCategory();
        }
    }

    public Building.BuildingType GetCurrentBuildingType() { return currentBuildingType; }

    private void SelectButton(MultiLayerButton button)
    {
        foreach (Image image in button.GetImages())
        {
            image.CrossFadeColor(selectedColor * colorMultiplier, 0f, true, true);
        }
    }

    private void DeselectButton(MultiLayerButton button)
    {
        foreach (Image image in button.GetImages())
        {
            image.CrossFadeColor(Color.white * colorMultiplier, 0f, true, true);
        }
    }
    #endregion Current Building Type

#if UNITY_EDITOR
    private void OnValidate()
    {
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
