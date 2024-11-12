using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HiraganaTable
{
    private List<List<char>> table;

    public HiraganaTable()
    {
        table = new List<List<char>>()
        {
            new List<char> { 'あ', 'い', 'う', 'え', 'お' }, // a i u e o
            new List<char> { 'か', 'き', 'く', 'け', 'こ' }, // ke ki ku ke ko
            new List<char> { 'さ', 'し', 'す', 'せ', 'そ' }, // sa shi su se so
            new List<char> { 'た', 'ち', 'つ', 'て', 'と' }, // ta chi tsu te to
            new List<char> { 'な', 'に', 'ぬ', 'ね', 'の' }, // na ni nu ne no
            new List<char> { 'は', 'ひ', 'ふ', 'へ', 'ほ' }, // ha hi fu he ho
            new List<char> { 'ま', 'み', 'む', 'め', 'も' }, // ma mi mu me mo
            new List<char> { 'や', 'ゆ', 'よ' }, // ya yu yo
            new List<char> { 'ら', 'り', 'る', 'れ', 'ろ' }, // ra ri ru re ro
            new List<char> { 'わ', 'ん', 'を' } //wa n wo
        };
    }

    public void GetSymbolPosition(char aSymbol, out int outConsonantIndex, out int outVowelIndex)
    {
        outConsonantIndex = -1;
        outVowelIndex = -1;

        for (int i = 0; i < table.Count; i++)
        {
            int j = table[i].FindIndex(symbol => symbol == aSymbol);
            if (j != -1) 
            {
                outConsonantIndex = i;
                outVowelIndex = j;
                return;
            }
        }
    }

    public char GetSymbol(int aConsonantIndex, int aVowelIndex)
    {
        return table[aConsonantIndex][aVowelIndex];
    }

}
