using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Library : MonoBehaviour
{
    // Start is called before the first frame update

    static private string kanjiSaveFolder;
    //static private string kanjiSaveFolder = Application.dataPath + "/Resources/RawData/vocab.csv";
    static private readonly Dictionary<string, List<string>> hiraganaToKanjiDictionary = new();
    static private readonly Dictionary<string, List<string>> KanjiToHiraganaDictionary = new();

    void Start()
    {
        InitPath();

        hiraganaToKanjiDictionary.Clear();
        KanjiToHiraganaDictionary.Clear();
        string rawData = System.IO.File.ReadAllText(kanjiSaveFolder);
        string[] rawDataArray = rawData.Split(new string[] { ";", "\n" }, System.StringSplitOptions.None);

        for (int i = 3; i < rawDataArray.Length; i += 3)
        {
            AddKanjiToHiraganaToKanjiDictionary(rawDataArray[i+1], rawDataArray[i]);
            AddHiraganaToKanjiToHiraganaDictionary(rawDataArray[i], rawDataArray[i + 1]);
        }
    }

    static private void InitPath()
    {
        kanjiSaveFolder = Path.Combine(Application.dataPath, "Resources", "RawData", "vocab.csv");
    }

    static public List<string> GetKanjiFromKana(string kana)
    {
        return hiraganaToKanjiDictionary.TryGetValue(kana, out List<string> retour) ? retour : null;
    }

    static public bool TryGetKanjiFromKana(string kana, out List<string> kanji)
    {
        kanji = GetKanjiFromKana(kana);
        return kanji != null;
    }

    static private void AddKanjiToHiraganaToKanjiDictionary(string kana, string kanji)
    {
        if (hiraganaToKanjiDictionary.ContainsKey(kana)) hiraganaToKanjiDictionary[kana].Add(kanji);
        else
        {
            List<string> list = new List<string>();
            list.Add(kanji);
            hiraganaToKanjiDictionary.Add(kana, list);
        }
    }

    static public List<string> GetKanaFromKanji(string kanji)
    {
        List<string> retour = new List<string>();
        if (KanjiToHiraganaDictionary.TryGetValue(kanji, out retour)) return retour;
        return null;
    }

    static public bool TryGetKanaFromKanji(string kanji, out List<string> kana)
    {
        kana = GetKanaFromKanji(kanji);
        return kana != null;
    }

    static private void AddHiraganaToKanjiToHiraganaDictionary(string kanji, string kana)
    {
        if (KanjiToHiraganaDictionary.ContainsKey(kanji)) KanjiToHiraganaDictionary[kanji].Add(kana);
        else
        {
            List<string> list = new List<string>();
            list.Add(kana);
            KanjiToHiraganaDictionary.Add(kanji, list);
        }
    }

    static public string GetKanjiSaveFolder()
    {
        return kanjiSaveFolder;
    }
}
