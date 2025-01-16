using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class SymbolTable
{
    private readonly int[] tentenIndex = { 1, 2, 3, 5 };
    private readonly char[] miniatureTable = { 'ゃ', 'ゅ', 'ょ', 'ャ', 'ュ', 'ョ' };
    private readonly char[] preModificatorTable = { 'っ', 'ッ' };
    private readonly char[] postModificatorTable = { 'ー', 'い', 'う' };
    private readonly List<List<char>> hiraganaTable;
    private readonly List<List<char>> katakanaTable;
    private readonly Dictionary<char, string> basicRomajiTable;
    private readonly Dictionary<string, string> composedRomajiTable;

    public SymbolTable()
    {
        // X are placed at invalid places (non-existing equivalent mixing corresponding consonant and vowel)
        hiraganaTable = new List<List<char>>()
        {
            //                a     i     u     e    o     n   
            new() { 'あ', 'い', 'う', 'え', 'お' },
            new() { 'か', 'き', 'く', 'け', 'こ' }, // k
            new() { 'さ', 'し', 'す', 'せ', 'そ' }, // s
            new() { 'た', 'ち', 'つ', 'て', 'と' }, // t
            new() { 'な', 'に', 'ぬ', 'ね', 'の' }, // n
            new() { 'は', 'ひ', 'ふ', 'へ', 'ほ' }, // h
            new() { 'ま', 'み', 'む', 'め', 'も' }, // m
            new() { 'や', 'X',  'ゆ', 'X', 'よ' }, // y
            new() { 'ら', 'り', 'る', 'れ', 'ろ' }, // r
            new() { 'わ', 'X',  'X', 'X',  'を' }, // w
            new() { 'X',  'X',  'X', 'X',  'X', 'ん' }
        };

        katakanaTable = new List<List<char>>()
        {
            //                a     i     u     e    o     n   
            new() { 'ア', 'イ', 'ウ', 'エ', 'オ' },
            new() { 'カ', 'キ', 'ク', 'ケ', 'コ' }, // k
            new() { 'サ', 'シ', 'ス', 'セ', 'ソ' }, // s
            new() { 'タ', 'チ', 'ツ', 'テ', 'ト' }, // t
            new() { 'ナ', 'ニ', 'ヌ', 'ネ', 'ノ' }, // n
            new() { 'ハ', 'ヒ', 'フ', 'ヘ', 'ホ' }, // h
            new() { 'マ', 'ミ', 'ム', 'メ', 'モ' }, // m
            new() { 'ヤ', 'X',  'ユ', 'X', 'ヨ' }, // y
            new() { 'ラ', 'リ', 'ル', 'レ', 'ロ' }, // r
            new() { 'ワ', 'X',  'X',  'X', 'ヲ' }, // w
            new() { 'X',  'X',  'X',  'X', 'X', 'ン' }
        };

        basicRomajiTable = new Dictionary<char, string> { 
            { 'あ', "a"  }, { 'い', "i"   }, { 'う', "u"   }, { 'え', "e"  }, { 'お', "o"  },
            { 'か', "ka" }, { 'き', "ki"  }, { 'く', "ku"  }, { 'け', "ke" }, { 'こ', "ko" },
            { 'さ', "sa" }, { 'し', "si"  }, { 'す', "su"  }, { 'せ', "se" }, { 'そ', "so" },
            { 'た', "ta" }, { 'ち', "chi" }, { 'つ', "tsu" }, { 'て', "te" }, { 'と', "to" },
            { 'な', "na" }, { 'に', "ni"  }, { 'ぬ', "nu"  }, { 'ね', "ne" }, { 'の', "no" },
            { 'は', "ha" }, { 'ひ', "hi"  }, { 'ふ', "fu"  }, { 'へ', "he" }, { 'ほ', "ho" },
            { 'ま', "ma" }, { 'み', "mi"  }, { 'む', "mu"  }, { 'め', "me" }, { 'も', "mo" },
            { 'や', "ya" },                  { 'ゆ', "yu"  },                { 'よ', "yo" },
            { 'ら', "ra" }, { 'り', "ri"  }, { 'る', "ru"  }, { 'れ', "re" }, { 'ろ', "ro" },
            { 'わ', "wa" },                                                  { 'を', "wo" },
            { 'ん', "n"  },
            { 'が', "ga" }, { 'ぎ', "gi"  }, { 'ぐ', "gu"  }, { 'げ', "ge" }, { 'ご', "go" },
            { 'ざ', "za" }, { 'じ', "ji"  }, { 'ず', "zu"  }, { 'ぜ', "ze" }, { 'ぞ', "zo" },
            { 'だ', "da" }, { 'ぢ', "ji"  }, { 'づ', "du"  }, { 'で', "de" }, { 'ど', "do" },
            { 'ば', "ba" }, { 'び', "bi"  }, { 'ぶ', "bu"  }, { 'べ', "be" }, { 'ぼ', "bo" },
            { 'ぱ', "pa" }, { 'ぴ', "pi"  }, { 'ぷ', "pu"  }, { 'ぺ', "pe" }, { 'ぽ', "po" },

            { 'ア', "a"  }, { 'イ', "i"   }, { 'ウ', "u"   }, { 'エ', "e"  }, { 'オ', "o" },
            { 'カ', "ka" }, { 'キ', "ki"  }, { 'ク', "ku"  }, { 'ケ', "ke" }, { 'コ', "ko" },
            { 'サ', "sa" }, { 'シ', "si"  }, { 'ス', "su"  }, { 'セ', "se" }, { 'ソ', "so" },
            { 'タ', "ta" }, { 'チ', "chi" }, { 'ツ', "tsu" }, { 'テ', "te" }, { 'ト', "to" },
            { 'ナ', "na" }, { 'ニ', "ni"  }, { 'ヌ', "nu"  }, { 'ネ', "ne" }, { 'ノ', "no" },
            { 'ハ', "ha" }, { 'ヒ', "hi"  }, { 'フ', "fu"  }, { 'ヘ', "he" }, { 'ホ', "ho" },
            { 'マ', "ma" }, { 'ミ', "mi"  }, { 'ム', "mu"  }, { 'メ', "me" }, { 'モ', "mo" },
            { 'ヤ', "ya" },                  { 'ユ', "yu"  },                { 'ヨ', "yo" },
            { 'ラ', "ra" }, { 'リ', "ri"  }, { 'ル', "ru"  }, { 'レ', "re" }, { 'ロ', "ro" },
            { 'ワ', "wa" },                                                  { 'ヲ', "wo" },
            { 'ン', "n"  },
            { 'ガ', "ga" }, { 'ギ', "gi"  }, { 'グ', "gu"  }, { 'ゲ', "ge" }, { 'ゴ', "go" },
            { 'ザ', "za" }, { 'ジ', "ji"  }, { 'ズ', "zu"  }, { 'ゼ', "ze" }, { 'ゾ', "zo" },
            { 'ダ', "da" }, { 'ヂ', "ji"  }, { 'ヅ', "du"  }, { 'デ', "de" }, { 'ド', "do" },
            { 'バ', "ba" }, { 'ビ', "bi"  }, { 'ブ', "bu"  }, { 'ベ', "be" }, { 'ボ', "bo" },
            { 'パ', "pa" }, { 'ピ', "pi"  }, { 'プ', "pu"  }, { 'ペ', "pe" }, { 'ポ', "po" }
        };

        composedRomajiTable = new Dictionary<string, string> {
            { "きゃ", "kya" }, { "きゅ", "kyu" }, { "きょ", "kyo" },
            { "しゃ", "sha" }, { "しゅ", "shu" }, { "しょ", "sho" },
            { "ちゃ", "cha" }, { "ちゅ", "chu" }, { "ちょ", "cho" },
            { "にゃ", "nya" }, { "にゅ", "nyu" }, { "にょ", "nyo" },
            { "ひゃ", "hya" }, { "ひゅ", "hyu" }, { "ひょ", "hyo" },
            { "みゃ", "mya" }, { "みゅ", "myu" }, { "みょ", "myo" },
            { "りゃ", "rya" }, { "りゅ", "ryu" }, { "りょ", "ryo" },

            { "ぎゃ", "gya" }, { "ぎゅ", "gyu" }, { "ぎょ", "gyo" },
            { "じゃ", "ja"  }, { "じゅ", "ju"  }, { "じょ", "jo"  },
            { "びゃ", "bya" }, { "びゅ", "byu" }, { "びょ", "byo" },
            { "ぴゃ", "pya" }, { "ぴゅ", "pyu" }, { "ぴょ", "pyo" },

            { "キゃ", "kya" }, { "キゅ", "kyu" }, { "キょ", "kyo" },
            { "シゃ", "sha" }, { "シゅ", "shu" }, { "シょ", "sho" },
            { "チゃ", "cha" }, { "チゅ", "chu" }, { "チょ", "cho" },
            { "ニゃ", "nya" }, { "ニゅ", "nyu" }, { "ニょ", "nyo" },
            { "ヒゃ", "hya" }, { "ヒゅ", "hyu" }, { "ヒょ", "hyo" },
            { "ミゃ", "mya" }, { "ミゅ", "myu" }, { "ミょ", "myo" },
            { "リゃ", "rya" }, { "リゅ", "ryu" }, { "リょ", "ryo" },

            { "ギゃ", "gya" }, { "ギゅ", "gyu" }, { "ギょ", "gyo" },
            { "ジゃ", "ja"  }, { "ジゅ", "ju"  }, { "ジょ", "jo"  },
            { "ビゃ", "bya" }, { "ビゅ", "byu" }, { "ビょ", "byo" },
            { "ピゃ", "pya" }, { "ピゅ", "pyu" }, { "ピょ", "pyo" },
        };
    }

    public void GetSymbolPosition(Item.Symbol anItemValue, out int outConsonantIndex, out int outVowelIndex)
    {
        outConsonantIndex = -1;
        outVowelIndex = -1;
        List<List<char>> symbolTable = new ();

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

    public string KanaToRomaji(string kanas)
    {
        StringBuilder romaji = new();
        int i = 0;
        string str;
        while (i < kanas.Length - 1)
        {
            if (miniatureTable.Contains(kanas[i + 1]))
            {
                romaji.Append(composedRomajiTable.TryGetValue(kanas[i] + "" + kanas[i+1], out str) ? str : "");
                i++;
            }
            else if (preModificatorTable.Contains(kanas[i]))
            {
                romaji.Append('/'); // Will be replace with the next letter at the end
            }
            else if (romaji.Length > 0 && postModificatorTable.Contains(kanas[i]))
            {
                if (kanas[i] == 'ー')
                {
                    romaji.Append(romaji[^1]);
                }
                else if (kanas[i] == 'い' && romaji.ToString()[^1] == 'e')
                {
                    romaji.Append('e');
                }
                else if (kanas[i] == 'う' && romaji[^1] == 'o')
                {
                    romaji.Append('o');
                }
                else
                {
                    romaji.Append(basicRomajiTable.TryGetValue(kanas[i], out str) ? str : "");
                }
            }
            else // Normal case
            {
                if (basicRomajiTable.TryGetValue(kanas[i], out str))
                {
                    romaji.Append(str);
                }
            }
            i++;
        }

        if (i < kanas.Length)
        {
            romaji.Append(basicRomajiTable.TryGetValue(kanas[i], out str) ? str : "");
        }

        // Replace the double letters from っ and ッ
        for (int j = 0; j < romaji.Length; j++)
        {
            if (romaji[j] == '/')
            {
                romaji.Remove(j, 1);
                romaji.Insert(j, romaji[j + 1]);
            }
        }

        //Debug.Log($"{kanas} : {romaji.ToString()}");
        return romaji.ToString();
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
