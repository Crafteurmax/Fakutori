using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum SymbolType {
        Hiragana,
        Katakana,
        Kanji
    }

    public struct Symbol
    {
        public string character;
        public SymbolType type;
    }

    [SerializeField] private List<Symbol> symbols = new List<Symbol>();

    private float heightOffset;
    private GameObject model;

    private void Awake()
    {
        model = transform.Find("Model").gameObject;
        heightOffset = model.transform.localScale.y / 2;
    }

    public float GetItemHeightOffset()
    {
        return heightOffset;
    }
}
