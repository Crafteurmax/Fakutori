using UnityEngine;
using UnityEngine.UI;

public class DictionaryButton : SelectableButton
{
    [Header("Color")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color alternativeColor;

    [Header("Pin")]
    [SerializeField] private Sprite pinnedSprite;
    [SerializeField] private Sprite notPinnedSprite;

    private MultiLayerButton pinButton;

    override protected void Awake()
    {
        base.Awake();

        pinButton = transform.Find("Pin Button").GetComponent<MultiLayerButton>();
    }

    // Color
    public void TriggerAlternative(bool trigger)
    {
        targetGraphic.color = trigger ? baseColor : alternativeColor;
        pinButton.SetColor(0, trigger ? baseColor : alternativeColor);
    }

    #region Pin
    public void Pin(bool pin)
    {
        pinButton.SetIconSprite(pin ? pinnedSprite : notPinnedSprite);
    }

    public Button GetPinButton()
    {
        return pinButton;
    }
    #endregion Pin
}
