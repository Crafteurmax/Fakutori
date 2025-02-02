using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using System.Text.RegularExpressions;

[System.Serializable]
public class KanjiRepresentation
{
    public Image image;
    public VideoPlayer videoPlayer;
}

public class Word : JapaneseLesson
{
    public string Kanji { get; }
    public string Kana { get; }
    public string Romanji { get; }
    public List<string> Traductions { get; }
    public List<string> Examples { get; }

    public Word(string kanji, string kana, string romanji, List<string> traductions, List<string> examples)
    {
        this.Kanji = kanji;
        this.Kana = kana;
        this.Romanji = romanji;
        this.Traductions = traductions;
        this.Examples = examples;
    }
}

public class Vocabulary : IJapaneseLessons<Word>
{
    private readonly List<string> kanjis = new();
    private readonly List<string> kanas = new();
    private readonly List<string> romanjis = new();
    private readonly List<List<string>> traductionss = new();
    private readonly List<List<string>> exampless = new();

    public void AddLesson(Word word)
    {
        kanjis.Add(word.Kanji);
        kanas.Add(word.Kana);
        romanjis.Add(word.Romanji);
        traductionss.Add(word.Traductions);
        exampless.Add(word.Examples);
    }

    public void TrimData()
    {
        for (int i = 0; i < kanjis.Count; i++)
        {
            kanjis[i] = kanjis[i].Trim(' ');
        }
        for (int i = 0; i < kanas.Count; i++)
        {
            kanas[i] = kanas[i].Trim(' ');
        }
        foreach (List<string> traductions in traductionss)
        {
            for (int i = 0; i < traductions.Count; i++)
            {
                traductions[i] = traductions[i].Trim(' ');
            }
        }
        foreach (List<string> examples in exampless)
        {
            for (int i = 0; i < examples.Count; i++)
            {
                examples[i] = examples[i].Trim(' ');
            }
        }
    }

    public int GetCount()
    {
        return kanjis.Count;
    }

    public Word GetLesson(int index)
    {
        return new Word(kanji: kanjis[index],
                        kana: kanas[index],
                        romanji: romanjis[index],
                        traductions: traductionss[index],
                        examples: exampless[index]);
    }

    public List<string> GetKanjis()
    {
        return kanjis;
    }

    public List<int> Search(List<int> currentSelection, string value)
    {
        return DetectSymbolType(value) switch
        {
            Item.SymbolType.Hiragana => IJapaneseLessons<Word>.SearchInLists(new List<List<string>>() { kanas, kanjis }, currentSelection, value),
            Item.SymbolType.Katakana => IJapaneseLessons<Word>.SearchInLists(new List<List<string>>() { kanas, kanjis }, currentSelection, value),
            Item.SymbolType.Kanji => IJapaneseLessons<Word>.SearchInLists(new List<List<string>>() { kanjis }, currentSelection, value),
            Item.SymbolType.Romaji => IJapaneseLessons<Word>.SearchInLists(new List<List<string>>() { romanjis }, new List<List<List<string>>>() { traductionss, exampless }, currentSelection, value),
            _ => new(),
        };
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

public class VocabularyDictionary : UIDictionary<VocabularyButton, Word , Vocabulary>
{
    public static string VocabularyFile { get; private set; }
    public static string VocabularyImageFolder { get; private set; }
    public static string UsedVocabularyImagesFolder { get; private set; }

    public static VocabularyDictionary Instance { get; private set; }

    private readonly SymbolTable symbolTable = new();

    [Header("Vocabulary Template")]
    [SerializeField] private List<KanjiRepresentation> kanjiRepresentations;
    [SerializeField] private TextMeshProUGUI readingTextMesh;
    [SerializeField] private TextMeshProUGUI traductionsTextMesh;
    [SerializeField] private TextMeshProUGUI examplesTextMesh;
    private int currentVideoPlayer = 0;

    [Header("Vocabulary Pin in game")]
    [SerializeField] private RectTransform pinInGameRect;
    [SerializeField] private LayoutGroup pinInGameLayout;
    [SerializeField] private VocabularyIndication vocabularyIndicationPrefab;
    [SerializeField] private SortedDictionary<int, VocabularyIndication> vocabularyIndications = new();
    private float pinInGameLayoutMaxHeight;
    private float vocabularyIndicationHeight;

    [Header("Debug")]
    [SerializeField] private bool sortImages;

    override protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GameObject.Destroy(this);
            return;
        }

        InitPath();

        if (sortImages)
        {
            SortImages();
        }

        foreach (VideoPlayer videoPlayer in kanjiRepresentations.Select(x => x.videoPlayer))
        {
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += EndReached;
        }

        pinInGameLayoutMaxHeight = pinInGameRect.sizeDelta.y;

        VocabularyIndication indication = GameObject.Instantiate<VocabularyIndication>(vocabularyIndicationPrefab);
        vocabularyIndicationHeight = indication.GetRectTransform().sizeDelta.y;
        GameObject.Destroy(indication.gameObject);
        RescaleIndicationRect();

        base.Awake();

        EnableDictionnay();
    }

    static private void InitPath()
    {
        VocabularyFile = Path.Combine(Application.dataPath, "Resources", "RawData", "vocab.csv");
        VocabularyImageFolder = Path.Combine(Application.dataPath, "Resources", "RawData", "VocabImages");
        UsedVocabularyImagesFolder = Path.Combine(Application.dataPath, "Resources", "RawData", "VocabImages", "Used");
    }

    private void SortImages()
    {
        Directory.CreateDirectory(UsedVocabularyImagesFolder);

        foreach (string kanji in lessons.GetKanjis())
        {
            foreach (char c in kanji)
            {
                if (File.Exists(Path.Combine(VocabularyImageFolder, c + ".mp4")))
                {
                    File.Move(Path.Combine(VocabularyImageFolder, c + ".mp4"), Path.Combine(UsedVocabularyImagesFolder, c + ".mp4"));
                }
                if (File.Exists(Path.Combine(VocabularyImageFolder, c + ".png")))
                {
                    File.Move(Path.Combine(VocabularyImageFolder, c + ".png"), Path.Combine(UsedVocabularyImagesFolder, c + ".png"));
                }
                if (File.Exists(Path.Combine(VocabularyImageFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(VocabularyImageFolder, c + ".gif"), Path.Combine(UsedVocabularyImagesFolder, c + ".gif"));
                }
            }
        }

        foreach (string file in Directory.GetFiles(VocabularyImageFolder))
        {
            File.Delete(file);
        }

        foreach (string kanji in lessons.GetKanjis())
        {
            foreach (char c in kanji)
            {
                if (File.Exists(Path.Combine(UsedVocabularyImagesFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(UsedVocabularyImagesFolder, c + ".gif"), Path.Combine(VocabularyImageFolder, c + ".gif"));
                }
                if (File.Exists(Path.Combine(UsedVocabularyImagesFolder, c + ".png")))
                {
                    File.Move(Path.Combine(UsedVocabularyImagesFolder, c + ".png"), Path.Combine(VocabularyImageFolder, c + ".png"));
                }
                if (File.Exists(Path.Combine(UsedVocabularyImagesFolder, c + ".gif")))
                {
                    File.Move(Path.Combine(UsedVocabularyImagesFolder, c + ".gif"), Path.Combine(VocabularyImageFolder, c + ".gif"));
                }
            }
        }
    }

    #region Initialization
    override protected string GetResourceFile()
    {
        return VocabularyFile;
    }

    override protected Word CreateData(string[] rawDataArray, char separator)
    {
        if (string.IsNullOrEmpty(rawDataArray[1])) { return null; }

        return new(kanji: rawDataArray[0],
                    kana: rawDataArray[1],
                    romanji: symbolTable.KanaToRomaji(rawDataArray[1]),
                    traductions: new(rawDataArray[2].Split(separator, System.StringSplitOptions.None)),
                    examples: new());
    }

    override protected void Verification()
    {
        foreach(string kanjis in lessons.GetKanjis())
        {
            foreach (char kanji in kanjis)
            {
                if (!File.Exists(Path.Combine(VocabularyImageFolder, kanji + ".mp4")))
                {
                    Debug.LogError($"File {kanji}.mp4 does not exist");
                }
                if (!File.Exists(Path.Combine(VocabularyImageFolder, kanji + ".png")))
                {
                    Debug.LogError($"File {kanji}.png does not exist");
                }
            }
        }       
    }

    override protected void InitButton(int index)
    {
        Word word = lessons.GetLesson(index);

        buttons[index].SetKanji(word.Kanji);
        buttons[index].SetKana(word.Kana);
        buttons[index].SetRomanji(word.Romanji);
        buttons[index].SetTraduction(word.Traductions[0]);
    }
    #endregion Initialization

    #region Open Page
    override protected void ApplyTemplate()
    {
        // Show informations
        currentVideoPlayer = 0;
        Word word = lessons.GetLesson(currentIndex);

        readingTextMesh.text = $"<b><u>Kana</u></b> : {word.Kana}\n<b><u>Romaji</b></u> : {word.Romanji}";
        traductionsTextMesh.text = $"<b><u>Traductions</b></u> :\n - {string.Join("\n - ", word.Traductions)}";

        for (int i = 0; i < word.Kanji.Length; i++)
        {
            kanjiRepresentations[i].image.gameObject.SetActive(true);
            kanjiRepresentations[i].videoPlayer.gameObject.SetActive(true);

            kanjiRepresentations[i].image.sprite = LoadSprite(Path.Combine(VocabularyImageFolder, word.Kanji[i] + ".png"));

            if (File.Exists(Path.Combine(VocabularyImageFolder, word.Kanji[i] + ".mp4")))
            {
                kanjiRepresentations[i].videoPlayer.url = Path.Combine(VocabularyImageFolder, word.Kanji[i] + ".mp4");
            }

            kanjiRepresentations[i].videoPlayer.Prepare();
            kanjiRepresentations[i].videoPlayer.Pause();
        }

        for (int i = word.Kanji.Length; i < kanjiRepresentations.Count; i++)
        {
            kanjiRepresentations[i].image.gameObject.SetActive(false);
            kanjiRepresentations[i].videoPlayer.Stop();
            kanjiRepresentations[i].videoPlayer.gameObject.SetActive(false);
        }

        kanjiRepresentations[0].videoPlayer.Play();
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
        currentVideoPlayer = (currentVideoPlayer + 1) % lessons.GetKanjis()[currentIndex].Length;

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

    public void ReOpenVocabularyPage()
    {
        foreach (VideoPlayer videoPlayer in kanjiRepresentations.Select(videoPlayer => videoPlayer.videoPlayer))
        {
            videoPlayer.frame = 0;
            videoPlayer.Pause();
        }
        currentVideoPlayer = 0;
        kanjiRepresentations[0].videoPlayer.Play();
    }
    #endregion Open Page

    #region Pin
    override protected void Pin(int index)
    {
        CreateVocabularyIndication(index);

        base.Pin(index);
    }

    private void CreateVocabularyIndication(int index)
    {
        VocabularyIndication vocabularyIndication = GameObject.Instantiate<VocabularyIndication>(vocabularyIndicationPrefab);
        vocabularyIndication.transform.SetParent(pinInGameLayout.transform);
        vocabularyIndication.transform.localScale = Vector3.one;

        Word word = lessons.GetLesson(index);

        vocabularyIndication.SetKana(word.Kana);
        vocabularyIndication.SetKanji(word.Kanji);

        vocabularyIndications.Add(index, vocabularyIndication);
        RescaleIndicationRect();  
    }

    override protected void UnPin(int index)
    {
        DestroyVocabularyIndication(index);

        base.UnPin(index);
    }

    private void DestroyVocabularyIndication(int index)
    {
        VocabularyIndication vocabularyIndication = vocabularyIndications[index];
        vocabularyIndications.Remove(index);

        GameObject.Destroy(vocabularyIndication.gameObject);
        RescaleIndicationRect();
    }

    private void RescaleIndicationRect()
    {
        pinInGameRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Min(pinInGameLayoutMaxHeight, vocabularyIndications.Count * vocabularyIndicationHeight));
    }

    override protected void SortAndRecolorPinnedButtons()
    {
        for (int i = 0; i < pinnedIndexes.Count; i++)
        {
            buttons[pinnedIndexes[i]].TriggerAlternative(i % 2 == 0);
            buttons[pinnedIndexes[i]].transform.SetAsLastSibling();

            vocabularyIndications[pinnedIndexes[i]].TriggerAlternative(i % 2 == 0);
            vocabularyIndications[pinnedIndexes[i]].transform.SetAsLastSibling();
        }
    }
    #endregion Pin
}
