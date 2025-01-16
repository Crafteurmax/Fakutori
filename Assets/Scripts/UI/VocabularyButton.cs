using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class VocabularyButton : SelectableButton
{
    [Header("Color")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color alternativeColor;

    [Header("Vocabulary")]
    [SerializeField] private string kanji;
    [SerializeField] private string kana;
    [SerializeField] private string romaji;
    [SerializeField] private string traduction;

    [Header("Intern Objects")]
    [SerializeField] private TextMeshProUGUI kanjiTextMesh;
    [SerializeField] private TextMeshProUGUI kanaTextMesh;
    [SerializeField] private TextMeshProUGUI romajiTextMesh;
    [SerializeField] private TextMeshProUGUI traductionTextMesh;

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

    public void TriggerAlternative(bool trigger)
    {
        targetGraphic.color = trigger ? baseColor : alternativeColor;
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        base.OnValidate();

        SetKanji(kanji);
        SetKana(kana);
        SetRomanji(romaji);
        SetTraduction(traduction);
    }
#endif
}
