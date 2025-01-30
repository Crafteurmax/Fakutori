using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class VocabularyIndication : MonoBehaviour
{
    [Header("Color")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color alternativeColor;

    [Header("Vocabulary")]
    [SerializeField] private string kanji;
    [SerializeField] private string kana;

    private Image image;
    private TextMeshProUGUI kanjiTextMesh;
    private TextMeshProUGUI kanaTextMesh;

    public void Awake()
    {
        image = GetComponent<Image>();
        kanjiTextMesh = transform.Find("Kanji").GetComponent<TextMeshProUGUI>();
        kanaTextMesh = transform.Find("Kana").GetComponent<TextMeshProUGUI>();
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

    public void TriggerAlternative(bool trigger)
    {
        image.color = trigger ? baseColor : alternativeColor;
    }

    public RectTransform GetRectTransform()
    {
        return image.rectTransform;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        Awake();

        SetKanji(kanji);
        SetKana(kana);
    }
#endif
}
