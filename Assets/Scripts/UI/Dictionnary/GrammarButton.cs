using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GrammarButton : DictionaryButton
{
    [Header("Grammar")]
    [SerializeField] private string title;
    [SerializeField] private string subTitle;

    private TextMeshProUGUI titleTextMesh;
    private TextMeshProUGUI subTitleTextMesh;

    override protected void Awake()
    {
        base.Awake();

        titleTextMesh = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        subTitleTextMesh = transform.Find("Subtitle").GetComponent<TextMeshProUGUI>();
    }

    public void SetTitle(string title)
    {
        this.title = title;
        titleTextMesh.text = title;
    }

    public void SetSubTitle(string subTitle)
    {
        this.subTitle = subTitle;
        subTitleTextMesh.text = subTitle;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        base.OnValidate();

        Awake();

        SetTitle(title);
        SetSubTitle(subTitle);
    }
#endif
}
