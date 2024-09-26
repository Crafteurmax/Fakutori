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
        public char character;
        public SymbolType type;
    }

    [SerializeField] private List<Symbol> symbols = new List<Symbol>();
    [SerializeField] private string characters;

    private float heightOffset;
    private GameObject model;

    private void Awake()
    {
        model = transform.Find("Model").gameObject;
        heightOffset = model.transform.localScale.y / 2;

        symbols.Add(new Symbol { character = 'あ', type = SymbolType.Hiragana });
        UpdateSymbols();
    }

    public float GetItemHeightOffset()
    {
        return heightOffset;
    }

    private void UpdateSymbols() {
        characters = "";
        foreach (var symbol in symbols) {
            characters += symbol.character;
        }
    }

    public void SetCharacters(string characters) {
        symbols.Clear();
        foreach (var character in characters) {
            symbols.Add(new Symbol { character = character, type = SymbolType.Hiragana });
        }
        UpdateSymbols();
    }
}
