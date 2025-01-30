using TMPro;
using UnityEngine;
using UnityEditor;

public class VocabularyButton : DictionaryButton
{
    [Header("Vocabulary")]
    [SerializeField] private string kanji;
    [SerializeField] private string kana;
    [SerializeField] private string romaji;
    [SerializeField] private string traduction;

    private TextMeshProUGUI kanjiTextMesh;
    private TextMeshProUGUI kanaTextMesh;
    private TextMeshProUGUI romajiTextMesh;
    private TextMeshProUGUI traductionTextMesh;

    override protected void Awake()
    {
        base.Awake();

        kanjiTextMesh = transform.Find("Kanji").GetComponent<TextMeshProUGUI>();
        kanaTextMesh = transform.Find("Kana").GetComponent<TextMeshProUGUI>();
        romajiTextMesh = transform.Find("Romaji").GetComponent<TextMeshProUGUI>();
        traductionTextMesh = transform.Find("Traduction").GetComponent<TextMeshProUGUI>();
    }

    public void SetKanji(string kanji)
    {
        this.kanji = kanji;
        kanjiTextMesh.text = kanji;
    }

    public void SetKana(string kana)
    {
        this.kana = kana;
        kanaTextMesh.text = kana;
    }

    public void SetRomanji(string romanji)
    {
        this.romaji = romanji;
        romajiTextMesh.text = romanji;
    }

    public void SetTraduction(string traduction)
    {
        this.traduction = traduction;
        traductionTextMesh.text = traduction;
    }
       

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        base.OnValidate();

        Awake();

        SetKanji(kanji);
        SetKana(kana);
        SetRomanji(romaji);
        SetTraduction(traduction);
    }
#endif
}
