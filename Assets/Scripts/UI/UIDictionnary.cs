using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class Word
{
    public string kanji;
    public string kana;
    public string romanji;
    public List<string> traductions;
    public List<string> examples;

    public Word(string kanji, string kana, string romanji, List<string> traductions, List<string> examples)
    {
        this.kanji = kanji;
        this.kana = kana;
        this.romanji = romanji;
        this.traductions = traductions;
        this.examples = examples;
    }
}

[System.Serializable]
public class Vocabulary
{
    private readonly SymbolTable symbolTable = new();

    public List<string> kanjis = new();
    public List<string> kanas = new();
    public List<string> romanjis = new();
    public List<List<string>> traductionss = new();
    public List<List<string>> exampless = new();

    public void AddWord(string kanji, string kana, List<string> traductions, List<string> examples)
    {
        kanjis.Add(kanji);
        kanas.Add(kana);
        romanjis.Add(symbolTable.KanaToRomaji(kana));
        traductionss.Add(traductions);
        exampless.Add(examples);
    }

    public Word GetWord(int index)
    {
        return new Word(kanji: kanjis[index],
                        kana: kanas[index],
                        romanji: romanjis[index],
                        traductions: traductionss[index],
                        examples: exampless[index]);
    }

    public int GetWordsCount()
    {
        return kanjis.Count ;
    }

    public List<int> Search(Item.SymbolType symbolType, List<int> currentSelection, string value)
    {
        switch (symbolType)
        {
            case Item.SymbolType.Hiragana:
            case Item.SymbolType.Katakana:
                return SearchInList(kanas, kanjis, currentSelection, value);

            case Item.SymbolType.Kanji:
                return SearchInList(kanjis, currentSelection, value);

            case Item.SymbolType.Romaji:
                return SearchInList(romanjis, traductionss, currentSelection, value);
        }
        return new();
    }

    public static List<int> SearchInList(List<string> searchList, List<int> currentSelection, string value)
    {
        List<int> result = new();
        int i = 0;
        while (i < currentSelection.Count)
        {
            if (searchList[currentSelection[i]].Contains(value))
            {
                result.Add(currentSelection[i]);
            }
            i++;
        }
        return result;
    }

    public static List<int> SearchInList(List<string> firstList, List<string> secondList, List<int> currentSelection, string value)
    {
        List<int> result = new();
        int i = 0;
        while (i < currentSelection.Count)
        {
            if (firstList[currentSelection[i]].Contains(value) || secondList[currentSelection[i]].Contains(value))
            {
                result.Add(currentSelection[i]);
            }
            i++;
        }
        return result;
    }


    public static List<int> SearchInList(List<string> firstList, List<List<string>> secondList, List<int> currentSelection, string value)
    {
        List<int> result = new();
        int i = 0;
        while (i < currentSelection.Count)
        {
            if (firstList[currentSelection[i]].Contains(value) || secondList[currentSelection[i]].Any(s => s.Contains(value)))
            {
                result.Add(currentSelection[i]);
            }
            i++;
        }
        return result;
    }
}

[System.Serializable]
public class KanjiRepresentation
{
    public Image image;
    public VideoPlayer videoPlayer;
}

public class UIDictionnary : MonoBehaviour
{
    /*TODO:
     * - Grammar
     * - Limit the size of image
     */

    static private string vocabImagesFolder;
    static private string usedVocabImagesFolder;

    [SerializeField] private TMP_InputField searchField;

    [Header("Vocabulary")]
    [SerializeField] private SelectableButton vocabularyCategoryButton;
    [SerializeField] private ScrollRect vocabularyScrollRect;
    [SerializeField] private VerticalLayoutGroup vocabularyLayout;
    [SerializeField] private VocabularyButton vocabularyButtonPrefab;
    private readonly List<VocabularyButton> vocabularyButtons = new();
    private int currentVocab;
    private readonly Vocabulary vocabulary = new();

    [Header("Vocabulary Template")]
    [SerializeField] private GameObject vocabularyPanel;
    [SerializeField] private List<KanjiRepresentation> kanjiRepresentations;
    [SerializeField] private TextMeshProUGUI readingTextMesh;
    [SerializeField] private TextMeshProUGUI traductionsTextMesh;
    [SerializeField] private TextMeshProUGUI examplesTextMesh;
    private int currentVideoPlayer = 0;

    [Header("Grammar")]
    [SerializeField] private SelectableButton grammarCategoryButton;
    [SerializeField] private ScrollRect grammarScrollRect;
    [SerializeField] private VerticalLayoutGroup grammarLayout;
    [SerializeField] private VocabularyButton grammarButtonPrefab;
    private readonly List<VocabularyButton> grammarButtons = new();

    [Header("Grammar Template")]
    [SerializeField] private GameObject grammarPanel;

    [Header("Debug")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private bool sortImages;

    private List<int> currentVocabularySelection;
    private Item.SymbolType searchSymbolType;
    private string lastSearch;

    void Start()
    {
        InitPath();

        InitializeVocabulary();

        if (sortImages)
        {
            SortImages();
        }

        foreach (VideoPlayer videoPlayer in kanjiRepresentations.Select(x => x.videoPlayer))
        {
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += EndReached;
        }

        currentVocabularySelection = Enumerable.Range(0, vocabulary.GetWordsCount()).ToList();

        CreateVocabularyButtons();

        currentVocab = vocabularyButtons.Count - 1;
        EnableVocabularyMode(true);
        OpenVocabularyPage(0);
    }

    static void InitPath()
    {
        vocabImagesFolder = Path.Combine(Application.dataPath, "Resources", "RawData", "VocabImages");
        usedVocabImagesFolder = Path.Combine(Application.dataPath, "Resources", "RawData", "VocabImages", "Used");
    }

    #region Vocabulary

    #region Init
    private void InitializeVocabulary()
    {
        string rawData = System.IO.File.ReadAllText(Library.GetKanjiSaveFolder());
        string[] rawVocab = rawData.Split("\n", System.StringSplitOptions.None);

        for (int i = 1; i < rawVocab.Length; i++)
        {
            string[] rawVocabArray = rawVocab[i].Split(",", System.StringSplitOptions.None);

            if (!string.IsNullOrEmpty(rawVocabArray[1]))
            {
                vocabulary.AddWord(kanji: rawVocabArray[0],
                              kana: rawVocabArray[1],
                              traductions: new(rawVocabArray[2].Split(";", System.StringSplitOptions.None)),
                              examples: new());
            }
        }
    }

    private void SortImages()
    {
        Directory.CreateDirectory(usedVocabImagesFolder);

        foreach (string kanji in vocabulary.kanjis)
        {
            foreach (char c in kanji)
            {
                if (File.Exists(Path.Combine(vocabImagesFolder, c + ".mp4")))
                {
                    File.Move(Path.Combine(vocabImagesFolder, c + ".mp4"), Path.Combine(usedVocabImagesFolder, c + ".mp4"));
                }
                if (File.Exists(Path.Combine(vocabImagesFolder, c + ".png")))
                {
                    File.Move(Path.Combine(vocabImagesFolder, c + ".png"), Path.Combine(usedVocabImagesFolder, c + ".png"));
                }
                if (File.Exists(Path.Combine(vocabImagesFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(vocabImagesFolder, c + ".gif"), Path.Combine(usedVocabImagesFolder, c + ".gif"));
                }
            }
        }

        foreach (string file in Directory.GetFiles(vocabImagesFolder))
        {
            File.Delete(file);
        }

        foreach (string kanji in vocabulary.kanjis)
        {
            foreach (char c in kanji)
            {
                if (File.Exists(Path.Combine(usedVocabImagesFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(usedVocabImagesFolder, c + ".gif"), Path.Combine(vocabImagesFolder, c + ".gif"));
                }
                if (File.Exists(Path.Combine(usedVocabImagesFolder, c + ".png")))
                {
                    File.Move(Path.Combine(usedVocabImagesFolder, c + ".png"), Path.Combine(vocabImagesFolder, c + ".png"));
                }
                if (File.Exists(Path.Combine(usedVocabImagesFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(usedVocabImagesFolder, c + ".gif"), Path.Combine(vocabImagesFolder, c + ".gif"));
                }
            }
        }
    }

    private void CreateVocabularyButtons()
    {
        for (int i = 0; i < vocabulary.GetWordsCount(); i++)
        {
            CreateVocabularyButton(i, vocabulary.GetWord(i));
        }
    }

    private void CreateVocabularyButton(int index, Word word)
    {
        VocabularyButton button = GameObject.Instantiate<VocabularyButton>(vocabularyButtonPrefab);
        button.transform.SetParent(vocabularyLayout.transform);
        button.onClick.AddListener(delegate { OpenVocabularyPage(index); });

        button.SetKanji(word.kanji);
        button.SetKana(word.kana);
        button.SetRomanji(word.romanji);
        button.SetTraduction(word.traductions[0]);
        button.TriggerAlternative(index % 2 != 0);

        vocabularyButtons.Add(button);
    }
    #endregion Init

    #region Show in dictionnary
    private void OpenVocabularyPage(int index)
    {
        if (index == currentVocab) { return; }

        // Select Button
        vocabularyButtons[currentVocab].SelectButton(false);
        currentVocab = index;
        vocabularyButtons[index].SelectButton(true);

        // Show informations
        currentVideoPlayer = 0;
        string kanji = vocabulary.kanjis[currentVocab];

        for (int i = 0; i < kanji.Length; i++)
        {
            readingTextMesh.text = $"<b><u>Kana</u></b> : {vocabulary.kanas[currentVocab]}\n<b><u>Romaji</b></u> : {vocabulary.romanjis[currentVocab]}";
            traductionsTextMesh.text = $"<b><u>Traductions</b></u> :\n - {string.Join("\n - ", vocabulary.traductionss[currentVocab])}";

            // Kanji reprensentation
            kanjiRepresentations[i].image.gameObject.SetActive(true);
            kanjiRepresentations[i].videoPlayer.gameObject.SetActive(true);

            kanjiRepresentations[i].image.sprite = LoadSprite(Path.Combine(vocabImagesFolder, kanji[i] + ".png"));

            if (File.Exists(Path.Combine(vocabImagesFolder, kanji[i] + ".mp4")))
            {
                kanjiRepresentations[i].videoPlayer.url = Path.Combine(vocabImagesFolder, kanji[i] + ".mp4");
            }

            kanjiRepresentations[i].videoPlayer.Prepare();
            kanjiRepresentations[i].videoPlayer.Pause();
        }

        for (int i = kanji.Length; i < kanjiRepresentations.Count; i++)
        {
            kanjiRepresentations[i].image.gameObject.SetActive(false);
            kanjiRepresentations[i].videoPlayer.gameObject.SetActive(false);
        }

        kanjiRepresentations[0].videoPlayer.Play();
    }

    private Sprite LoadSprite(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning(filePath + " does not exist");
            return defaultSprite;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(filePath);

        Texture2D texture = new(1, 1);
        texture.LoadImage(bytes);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static void OnPrepared(VideoPlayer videoPlayer)
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.frame = 0;
        }
    }

    private void EndReached(VideoPlayer videoPlayer)
    {
        currentVideoPlayer = (currentVideoPlayer + 1) % vocabulary.kanjis[currentVocab].Length;

        if (currentVideoPlayer == 0)
        {
            foreach (KanjiRepresentation kanjiRepresentation in kanjiRepresentations)
            {
                kanjiRepresentation.videoPlayer.frame = 0;
            }
        }

        kanjiRepresentations[currentVideoPlayer].videoPlayer.frame = 0;
        kanjiRepresentations[currentVideoPlayer].videoPlayer.Play();
    }
    #endregion Show in dictionnary

    private void VocabularySearch(string search)
    {
        search = search.ToLower();
        
        if (string.IsNullOrEmpty(search))
        {
            currentVocabularySelection = Enumerable.Range(0, vocabulary.GetWordsCount()).ToList();
        }
        else if (search[0..^1] != lastSearch)
        {
            Debug.Log("Other");
            searchSymbolType = DetectSymbolType(search);

            if (searchSymbolType == Item.SymbolType.None) { return; }

            currentVocabularySelection = Enumerable.Range(0, vocabulary.GetWordsCount()).ToList();

            currentVocabularySelection = vocabulary.Search(searchSymbolType, currentVocabularySelection, search); // Update the selection
        }
        else
        {
            if (string.IsNullOrEmpty(lastSearch))
            {
                searchSymbolType = DetectSymbolType(search);
            }

            currentVocabularySelection = vocabulary.Search(searchSymbolType, currentVocabularySelection, search); // Update the selection
        }
        lastSearch = search;

        ShowVocabularyButtons();
    }

    private void ShowVocabularyButtons()
    {
        int select = 0;
        int item = 0;
        while (select < currentVocabularySelection.Count)
        {
            vocabularyButtons[item].gameObject.SetActive(item == currentVocabularySelection[select]);
            if (item == currentVocabularySelection[select])
            {
                vocabularyButtons[item].TriggerAlternative(select % 2 != 0);
                select++;
            }

            item++;
        }

        for (int i = item; i < vocabularyButtons.Count; i++)
        {
            vocabularyButtons[i].gameObject.SetActive(false);
        }
    }
    #endregion Vocabulary

    public void EnableVocabularyMode(bool enable)
    {
        vocabularyCategoryButton.SelectButton(enable);
        grammarCategoryButton.SelectButton(!enable);

        vocabularyScrollRect.gameObject.SetActive(enable);
        vocabularyPanel.SetActive(enable);

        grammarScrollRect.gameObject.SetActive(!enable);
        grammarPanel.SetActive(!enable);
    }

    public void Search()
    {
        VocabularySearch(searchField.text);
    }

    private static Item.SymbolType DetectSymbolType(string input)
    {
        if (Regex.IsMatch(input, @"^[\u0020-\u007E]+$")) // Romaji (ASCII range)
        {
            return Item.SymbolType.Romaji;
        }
        else if (Regex.IsMatch(input, @"^[\u3040-\u309F]+$")) // Hiragana (ASCII range)
        {
            return Item.SymbolType.Hiragana;
        }
        else if (Regex.IsMatch(input, @"^[\u30A0-\u30FF]+$")) // Katakana (ASCII range)
        {
            return Item.SymbolType.Katakana;
        }
        else if (Regex.IsMatch(input, @"^[\u4E00-\u9FBF]+$")) // Kanji (ASCII range)
        {
            return Item.SymbolType.Kanji;
        }
        else
        {
            return Item.SymbolType.None;
        }
    }
}
