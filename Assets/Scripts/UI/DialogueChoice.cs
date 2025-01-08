using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueChoice : MonoBehaviour
{
    private int id;
    DialogueManager manager;

    [SerializeField] TextMeshProUGUI text;

    public void SetupButton(int _id, string _text, DialogueManager _manager) 
    { 
        id = _id;
        manager = _manager;
        text.text = _text;
    }

    public void WhenButtonIsPressed()
    {
        manager.NextChoice(id);
    }
}
