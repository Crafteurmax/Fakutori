using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingButtonHover : MonoBehaviour
{
    [SerializeField] private int timeToWait = 2;
    [SerializeField] private TMP_Text hoverText;
    [SerializeField] private Image backgroundImage;

    Coroutine currentCoroutine = null;

    private void Start()
    {
        ToggleTextView(false);
    }

    public void SetHoverText(string text)
    {
        hoverText.text = text;
    }

    private void ToggleTextView(bool toggle)
    {
        if (hoverText.text != string.Empty)
        {
            hoverText.enabled = toggle;
            backgroundImage.enabled = toggle;
        }
        else
        {
            hoverText.enabled = false;
            backgroundImage.enabled = false;
        }
    }

    public void PointerEventEnter(BaseEventData eventData)
    {
        //Debug.Log("Pointer is hovering the button");
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(TimeOverButton());
    }

    public void PointerEventLeave(BaseEventData eventData)
    {
        StopCoroutine(currentCoroutine);
        ToggleTextView(false);
        //Debug.Log("Pointer is no longer hovering");
    }

    private IEnumerator TimeOverButton()
    {
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);
        yield return new WaitForSeconds(timeToWait);
        ToggleTextView(true);
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
