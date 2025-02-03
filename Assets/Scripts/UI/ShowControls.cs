using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControls : MonoBehaviour
{
    private MultiLayerButton pinButton;
    private bool controlsShown;
    private RectTransform rectTransform;
    private Vector2 basePosition;

    [Header("Pin")]
    [SerializeField] private Sprite pinnedSprite;
    [SerializeField] private Sprite notPinnedSprite;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        basePosition = transform.localPosition;

        pinButton = GetComponentInChildren<MultiLayerButton>();

        controlsShown = PlayerPrefs.GetInt("ShowControls", 1) == 1;

        if (controlsShown)
        {
            Pin();
        }
        else
        {
            Unpin();
        }
    }

    
    public void SwitchShowControls()
    {
        if (controlsShown)
        {
            Unpin();
        }
        else
        {
            Pin();
        }
    }

    private void Pin()
    {
        controlsShown = true;
        PlayerPrefs.SetInt("ShowControls", 1);
        pinButton.SetIconSprite(pinnedSprite);
        transform.localPosition = basePosition;
    }

    private void Unpin()
    {
        controlsShown = false;
        PlayerPrefs.SetInt("ShowControls", 0);
        pinButton.SetIconSprite(notPinnedSprite);
        transform.localPosition = new Vector2(-(1920 + rectTransform.sizeDelta.x) / 2, basePosition.y);
    }
}
