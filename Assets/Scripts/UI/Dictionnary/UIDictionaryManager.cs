using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDictionaryManager : MonoBehaviour
{
    public List<SelectableButton> CategoryButtons { get; } = new();
    public List<GameObject> Panels { get; } = new();
    public List<ScrollRect> ScrollRects { get; } = new();
}
