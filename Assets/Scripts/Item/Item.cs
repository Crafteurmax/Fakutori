using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Item : MonoBehaviour
{
    public enum SymbolType {
        Hiragana,
        Katakana,
        Kanji,
        Romaji,
        None
    }

    [System.Serializable]
    public struct Symbol
    {
        public char character;
        public SymbolType type;
    }

    [SerializeField] private List<Symbol> symbols = new List<Symbol>();
    [SerializeField] private string characters;
    [SerializeField] private TextMeshPro charactersText;

    private Quaternion defaultRotation;
    private Vector3 defaultScale;
    private float heightOffset;

    private void Awake()
    {
        heightOffset = 4 * transform.Find("Model").localScale.y;
        defaultRotation = transform.rotation;
        defaultScale = transform.localScale;

        //symbols.Add(new Symbol { character = 'あ', type = SymbolType.Hiragana });
        //UpdateSymbols();
    }

    private void OnEnable() {
        ClearCharacters();
    }

    private void ChangeColor() {
        if (symbols.Count == 0) return;
        Color color = Color.white;
        switch (symbols[0].type)
        {
            case SymbolType.Hiragana:
                color = Color.green;
                break;
            case SymbolType.Katakana:
                color = Color.red;
                break;
            case SymbolType.Kanji:
                color = Color.yellow;
                break;
        }
        transform.Find("Base").GetComponent<MeshRenderer>().material.color = color;
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
        charactersText.text = characters;
        ChangeColor();
    }

    public void ClearCharacters() {
        symbols.Clear();
        UpdateSymbols();
    }

    public void SetCharacters(List<Symbol> newSymbols) {
        symbols = newSymbols;
        UpdateSymbols();
    }

    public void AddCharacter(Symbol symbol) {
        symbols.Add(symbol);
        UpdateSymbols();
    }

    public List<Symbol> GetSymbols() {
        return symbols;
    }

    public void SetSymbols(List<Symbol> symbols) {
        this.symbols = symbols;
        UpdateSymbols();
    }

    public bool Equals(Item other) 
    {  
        return other.characters == characters;
        /*
        if(other.symbols.Count != symbols.Count) return false;
        for (int i = 0; i < symbols.Count; i++)
        {
            if (symbols[i].character != other.symbols[i].character) return false;
            if (symbols[i].type != other.symbols[i].type) return false;
        }
        return true;
        */
    }

    public override string ToString()
    {
        return characters;
    }

    public void SetInvisible(bool invisible) {
        transform.Find("Model").GetComponent<MeshRenderer>().enabled = !invisible;
        transform.Find("Base").GetComponent<MeshRenderer>().enabled = !invisible;
        charactersText.enabled = !invisible;
    }

    public void ResetRotationAndScale() {
        transform.rotation = defaultRotation;
        transform.localScale = defaultScale;
    }

    public void Reset() {
        ClearCharacters();
        SetInvisible(false);
        ResetRotationAndScale();
    }
}
