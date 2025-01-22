using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Timeline;
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
        Debug.Log(string.Join(", ", currentSelection));

        return symbolType switch
        {
            Item.SymbolType.Hiragana => SearchInList(kanas, kanjis, currentSelection, value),
            Item.SymbolType.Katakana => SearchInList(kanas, kanjis, currentSelection, value),
            Item.SymbolType.Kanji => SearchInList(kanjis, currentSelection, value),
            Item.SymbolType.Romaji => SearchInList(romanjis, traductionss, currentSelection, value),
            _ => new(),
        };
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
            if (firstList[currentSelection[i]].ToLower().Contains(value) || secondList[currentSelection[i]].ToLower().Contains(value))
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
            if (firstList[currentSelection[i]].ToLower().Contains(value) || secondList[currentSelection[i]].Any(s => s.ToLower().Contains(value)))
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
    public static UIDictionnary Instance { get; private set; }

    /*TODO:
     * - Grammar
     * - Limit the size of image
     */

    [SerializeField] private TMP_InputField searchField;

    [Header("Vocabulary General")]
    [SerializeField] private VocabularyButton vocabularyButtonPrefab;

    [Header("Vocabulary Scroll")]
    [SerializeField] private SelectableButton vocabularyCategoryButton;
    [SerializeField] private ScrollRect vocabularyScrollRect;
    [SerializeField] private RectTransform vocabularyLayout;
    [SerializeField] private VerticalLayoutGroup pinnedVocabularyLayout;
    [SerializeField] private VerticalLayoutGroup searchVocabularyLayout;

    static private string vocabImagesFolder;
    static private string usedVocabImagesFolder;

    private readonly Vocabulary vocabulary = new();
    private readonly List<VocabularyButton> vocabularyButtons = new();
    private readonly List<int> pinnedVocabulary = new();
    private List<int> searchedVocabulary = new();
    private int currentVocab;

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

    private Item.SymbolType searchSymbolType;
    private string lastSearch;

    void Start()
    {
        Instance = this;
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

        CreateVocabularyButtons();
        LayoutRebuilder.ForceRebuildLayoutImmediate(vocabularyLayout);

        ResetSearch();

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
        button.transform.SetParent(searchVocabularyLayout.transform);
        button.onClick.AddListener(delegate { OpenVocabularyPage(index); });
        button.GetPinButton().onClick.AddListener(delegate { PinVocabularyButton(index); });

        button.SetKanji(word.kanji);
        button.SetKana(word.kana);
        button.SetRomanji(word.romanji);
        button.SetTraduction(word.traductions[0]);
        button.TriggerAlternative(index % 2 != 0);

        vocabularyButtons.Add(button);
    }
    #endregion Init

    #region Show in dictionnary
    public void OpenVocabularyPage(int index)
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

    #region Pin
    private void PinVocabularyButton(int index)
    {
        bool pinned = pinnedVocabulary.Contains(index);

        vocabularyButtons[index].Pin(!pinned);
        vocabularyButtons[index].transform.SetParent(pinned ? searchVocabularyLayout.transform : pinnedVocabularyLayout.transform);
        vocabularyButtons[index].transform.localScale = Vector3.one;

        if (pinned)
        {
            pinnedVocabulary.Remove(index);
            if (string.IsNullOrEmpty(lastSearch) || vocabulary.Search(searchSymbolType, new() { index }, lastSearch).Count > 0)
            {
                searchedVocabulary.Add(index);
                searchedVocabulary.Sort();

                vocabularyButtons[index].gameObject.SetActive(true);
            }
            else
            {
                vocabularyButtons[index].gameObject.SetActive(false);
            }
        }
        else
        {
            pinnedVocabulary.Add(index);
            pinnedVocabulary.Sort();
            searchedVocabulary.Remove(index);
        }

        foreach (VocabularyButton button in vocabularyButtons)
        {
            button.transform.SetAsLastSibling();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(vocabularyLayout);

        UpdateVocabularyColors();
    }

    private void UpdateVocabularyColors()
    {
        for (int i = 0; i < pinnedVocabulary.Count; i++)
        {
            vocabularyButtons[pinnedVocabulary[i]].TriggerAlternative(i % 2 == 0);
        }

        for (int i = 0; i < searchedVocabulary.Count; i++)
        {
            vocabularyButtons[searchedVocabulary[i]].TriggerAlternative(i % 2 == 0);
        }
    }
    #endregion Pin

    private void VocabularySearch(string search)
    {
        /*if (string.IsNullOrEmpty(search))
        {
            searchSymbolType = Item.SymbolType.None;
            ResetSearch();
        }
        else if (search[0..^1] != lastSearch)
        {
            Debug.Log("Other");
            searchSymbolType = DetectSymbolType(search);

            if (searchSymbolType == Item.SymbolType.None) { return; }

            ResetSearch();

            searchedVocabulary = vocabulary.Search(searchSymbolType, searchedVocabulary, search); // Update the selection
        }
        else
        {
            searchedVocabulary = vocabulary.Search(searchSymbolType, searchedVocabulary, search); // Update the selection
        }*/
        searchedVocabulary = vocabulary.Search(searchSymbolType, searchedVocabulary, search); // Update the selection

        ShowVocabularyButtons();
    }

    private void ShowVocabularyButtons()
    {
        int select = 0;
        int item = 0;
        while (select < searchedVocabulary.Count)
        {
            vocabularyButtons[item].gameObject.SetActive(item == searchedVocabulary[select]);
            if (item == searchedVocabulary[select])
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

        foreach (int pinnedButton in pinnedVocabulary)
        {
            vocabularyButtons[pinnedButton].gameObject.SetActive(true);
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
        string search = searchField.text.ToLower();

        if (string.IsNullOrEmpty(search))
        {
            searchSymbolType = Item.SymbolType.None;
            ResetSearch();

            ShowVocabularyButtons();
        }
        else if (string.IsNullOrEmpty(lastSearch) || search[0..^1] != lastSearch)
        {
            searchSymbolType = DetectSymbolType(search);

            if (searchSymbolType == Item.SymbolType.None) { return; }

            ResetSearch();

            VocabularySearch(search);
        }
        else
        {
            VocabularySearch(search);
        }

        lastSearch = search;
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

    private void ResetSearch()
    {
        searchedVocabulary = Enumerable.Range(0, vocabulary.GetWordsCount()).ToList();
        foreach (int pinned in pinnedVocabulary)
        {
            searchedVocabulary.Remove(pinned);
        }
    }
}
