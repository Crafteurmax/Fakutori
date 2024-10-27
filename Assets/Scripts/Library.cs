using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Library : MonoBehaviour
{
    // Start is called before the first frame update

    private string kanjiSaveFolder = Application.dataPath + "/RawData/vocab.csv";
    private Dictionary<string, List<string>> hiraganaToKanjiDictionary = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> KanjiToHiraganaDictionary = new Dictionary<string, List<string>>();

    void Start()
    {
        string rawData = System.IO.File.ReadAllText(kanjiSaveFolder);
        string[] rawDataArray = rawData.Split(new string[] { ",", "\n" }, System.StringSplitOptions.None);

        for (int i = 3; i < rawDataArray.Length; i += 3)
        {
            AddKanjiToHiraganaToKanjiDictionary(rawDataArray[i+1], rawDataArray[i]);
            AddHiraganaToKanjiToHiraganaDictionary(rawDataArray[i], rawDataArray[i + 1]);
        }
        Debug.Log(GetKanjiFromKana("こう")[0]);
        Debug.Log(GetKanaFromKanji("好")[0]);

    }


    public List<string> GetKanjiFromKana(string kana)
    {
        List<string> retour = new List<string>();
        if (hiraganaToKanjiDictionary.TryGetValue(kana,out retour)) return retour;
        return null;
    }

    private void AddKanjiToHiraganaToKanjiDictionary(string kana, string kanji)
    {
        if (hiraganaToKanjiDictionary.ContainsKey(kana)) hiraganaToKanjiDictionary[kana].Add(kanji);
        else
        {
            List<string> list = new List<string>();
            list.Add(kanji);
            hiraganaToKanjiDictionary.Add(kana, list);
        }
    }

    public List<string> GetKanaFromKanji(string kanji)
    {
        List<string> retour = new List<string>();
        if (KanjiToHiraganaDictionary.TryGetValue(kanji, out retour)) return retour;
        return null;
    }

    private void AddHiraganaToKanjiToHiraganaDictionary(string kanji, string kana)
    {
        if (KanjiToHiraganaDictionary.ContainsKey(kanji)) KanjiToHiraganaDictionary[kanji].Add(kana);
        else
        {
            List<string> list = new List<string>();
            list.Add(kana);
            KanjiToHiraganaDictionary.Add(kanji, list);
        }
    }
}
