using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlowTyper : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] float speed = 50;

    private UnityAction finishAction;

    private bool shouldSkipText = false;

    // private void Update() {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         shouldSkipText = true;
    //     }
    // }

    public void Begin(string newText)
    {
        text.text = newText;
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        string originalText = text.text;
        text.text = "";
        for ( int i = 0; i < originalText.Length; i++ )
        {
            text.text += originalText[i];
            // if (shouldSkipText) { text.text = originalText; shouldSkipText = false;
            // break; }
            yield return new WaitForSeconds( 1/speed );
        }

        finishAction?.Invoke();
    }

    public void Clear()
    {
        text.text = "";
    }

    public void SetFinishAction(UnityAction action)
    {
        finishAction = action;
    }

}
