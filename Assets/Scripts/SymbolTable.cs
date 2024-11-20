using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymbolTable
{
    private int[] tentenIndex = { 1, 2, 3, 5 };
    private List<List<char>> hiraganaTable;
    private List<List<char>> katakanaTable;

    public SymbolTable()
    {
        // X are placed at invalid places (non-existing equivalent mixing corresponding consonant and vowel)
        hiraganaTable = new List<List<char>>()
        {
            //                a     i     u     e    o     n   
            new List<char> { 'あ', 'い', 'う', 'え', 'お' }, // 
            new List<char> { 'か', 'き', 'く', 'け', 'こ' }, // k
            new List<char> { 'さ', 'し', 'す', 'せ', 'そ' }, // s
            new List<char> { 'た', 'ち', 'つ', 'て', 'と' }, // t
            new List<char> { 'な', 'に', 'ぬ', 'ね', 'の' }, // n
            new List<char> { 'は', 'ひ', 'ふ', 'へ', 'ほ' }, // h
            new List<char> { 'ま', 'み', 'む', 'め', 'も' }, // m
            new List<char> { 'や', 'X', 'ゆ', 'X', 'よ' }, // y
            new List<char> { 'ら', 'り', 'る', 'れ', 'ろ' }, // r
            new List<char> { 'わ', 'X', 'X', 'X', 'を' }, // w
            new List<char> { 'X', 'X', 'X', 'X', 'X', 'ん' } //
        };

        katakanaTable = new List<List<char>>()
        {
            //                a     i     u     e    o     n   
            new List<char> { 'ア', 'イ', 'ウ', 'エ', 'オ' }, // 
            new List<char> { 'カ', 'キ', 'ク', 'ケ', 'コ' }, // k
            new List<char> { 'サ', 'シ', 'ス', 'セ', 'ソ' }, // s
            new List<char> { 'タ', 'チ', 'ツ', 'テ', 'ト' }, // t
            new List<char> { 'ナ', 'ニ', 'ヌ', 'ネ', 'ノ' }, // n
            new List<char> { 'ハ', 'ヒ', 'フ', 'ヘ', 'ホ' }, // h
            new List<char> { 'マ', 'ミ', 'ム', 'メ', 'モ' }, // m
            new List<char> { 'ヤ', 'X', 'ユ', 'X', 'ヨ' }, // y
            new List<char> { 'ラ', 'リ', 'ル', 'レ', 'ロ' }, // r
            new List<char> { 'ワ', 'X', 'X', 'X', 'ヲ' }, // w
            new List<char> { 'X', 'X', 'X', 'X', 'X', 'ン' } //
        };

    }

    public void GetSymbolPosition(Item.Symbol anItemValue, out int outConsonantIndex, out int outVowelIndex)
    {
        outConsonantIndex = -1;
        outVowelIndex = -1;
        List<List<char>> symbolTable = new List<List<char>>();

        if (anItemValue.type == Item.SymbolType.Kanji)
            return;

        symbolTable = anItemValue.type == Item.SymbolType.Hiragana ? hiraganaTable : katakanaTable;

        for (int i = 0; i < symbolTable.Count; i++)
        {
            int j = symbolTable[i].FindIndex(symbol => symbol == anItemValue.character);
            if (j != -1)
            {
                outConsonantIndex = i;
                outVowelIndex = j;
                return;
            }
        }
    }

    public char GetSymbol(int aConsonantIndex, int aVowelIndex, Item.SymbolType aType)
    {
        if (aConsonantIndex < 0 || aVowelIndex < 0 || aConsonantIndex >= hiraganaTable.Count || aVowelIndex >= hiraganaTable[aConsonantIndex].Count)
            return 'X';
        if (aType == Item.SymbolType.Hiragana)
            return hiraganaTable[aConsonantIndex][aVowelIndex];
        if (aType == Item.SymbolType.Katakana)
            return katakanaTable[aConsonantIndex][aVowelIndex];

        return 'X';
    }

    public void AddTenten(ref Item.Symbol aSymbol)
    {
        int consonantIndex, vowelIndex;
        GetSymbolPosition(aSymbol, out consonantIndex, out vowelIndex);

        aSymbol.character = tentenIndex.Contains(consonantIndex)? (char)(aSymbol.character + 0x01) : 'X';
    }

    public void AddMaru(ref Item.Symbol aSymbol)
    {
        int consonantIndex, vowelIndex;
        GetSymbolPosition(aSymbol, out consonantIndex, out vowelIndex);

        aSymbol.character = (consonantIndex == 5) ? (char)(aSymbol.character + 0x02) : 'X';
    }
}
