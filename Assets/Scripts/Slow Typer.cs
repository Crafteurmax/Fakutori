using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlowTyper : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] TMPro.TextMeshProUGUI text;
    private string originalT;
    [SerializeField] float speed = 50;

    private bool isTyping;

    public void Begin(string newText)
    {
        text.text = newText;

        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        
        string originalText = text.text;
        originalT = originalText;
        text.text = "";
        isTyping = true;
        for ( int i = 0; i < originalText.Length; i++ )
        {
            text.text += originalText[i];
            // if (shouldSkipText) { text.text = originalText; shouldSkipText = false;
            // break; }
            yield return new WaitForSeconds( 1/speed );
        }

        text.text = originalText;
        isTyping = false;
    }

    public void Clear()
    {
        text.text = "";
    }

    public bool IsTyping() {
        return isTyping;
    }

    public void finishText() {
        StopAllCoroutines();
        text.text = originalT;
        isTyping = false;
    }

}
