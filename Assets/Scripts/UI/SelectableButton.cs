using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MultiLayerButton
{
    [Header("Selection")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private bool isSelected = false;

    #region Selection
    public void SelectButton(bool isSelected)
    {
        if (isSelected)
        {
            SelectButton();
        }
        else
        {
            DeselectButton();
        }
    }

    private void SelectButton()
    {
        isSelected = true;
        DoStateTransition(Selectable.SelectionState.Normal, false);
    }

    private void DeselectButton()
    {
        isSelected = false;
        DoStateTransition(Selectable.SelectionState.Normal, false);
    }
    #endregion Selection

    public void SetSelectedColor(Color color)
    {
        selectedColor = color;
    }

    protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
    {
        if (isSelected)
        {
            // Switch for a single variable
            Color tintColor = isSelected switch
            {
                true => selectedColor,
                false => colors.normalColor,
            };

            targetGraphic.CrossFadeColor(tintColor * colors.colorMultiplier, 0f, true, true);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(tintColor * colors.colorMultiplier, 0f, true, true);
            }
        }
        else
        {
            base.DoStateTransition(state, instant);
        }
    }
}
