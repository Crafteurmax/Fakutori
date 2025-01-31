using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.LookDev;
using UnityEngine.Video;

[System.Serializable]
public class ColorTag
{
    public string tag;
    public Color color;
}

public class GrammarLesson : JapaneseLesson
{
    public string Title { get; }
    public string Subtitle { get; }
    public string LessonBody { get; }
    public List<string> Examples { get; }

    public GrammarLesson(string title, string subtitle, string lesson, List<string> examples)
    {
        this.Title = title;
        this.Subtitle = subtitle;
        this.LessonBody = lesson;
        this.Examples = examples;
    }
}

public class GrammarLessons : IJapaneseLessons<GrammarLesson>
{
    private readonly List<string> titles = new();
    private readonly List<string> subtitles = new();
    private readonly List<string> lessonsBodies = new();
    private readonly List<List<string>> exampless = new();

    public void AddLesson(GrammarLesson lesson)
    {
        titles.Add(lesson.Title);
        subtitles.Add(lesson.Subtitle);
        lessonsBodies.Add(lesson.LessonBody);
        exampless.Add(lesson.Examples);
    }

    public void TrimData()
    {
        for (int i = 0; i < titles.Count; i++)
        {
            titles[i] = titles[i].Trim(' ');
        }
        for (int i = 0; i < subtitles.Count; i++)
        {
            subtitles[i] = subtitles[i].Trim(' ');
        }
        for (int i = 0; i < lessonsBodies.Count; i++)
        {
            lessonsBodies[i] = lessonsBodies[i].Trim(' ');
        }
        foreach (List<string> examples in exampless)
        {
            for (int i = 0; i < examples.Count; i++)
            {
                examples[i] = examples[i].Trim(' ');
            }
        }
    }

    public List<int> Search(List<int> currentSelection, string value)
    {
        return IJapaneseLessons<GrammarLesson>.SearchInLists(new List<List<string>>() { titles, subtitles, lessonsBodies }, new List<List<List<string>>>() { exampless }, currentSelection, value);
    }

    public int GetCount()
    {
        return titles.Count;
    }

    public GrammarLesson GetLesson(int index)
    {
        return new GrammarLesson(title: titles[index],
                                subtitle: subtitles[index],
                                lesson: lessonsBodies[index],
                                examples: exampless[index]);
    }
}

public class GrammarDictionary : UIDictionary<GrammarButton, GrammarLesson, GrammarLessons>
{
    public static string GrammarFile { get; private set; }

    [Header("Grammar Template")]
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI subTitleTextMesh;
    [SerializeField] private TextMeshProUGUI lessonTextMesh;
    [SerializeField] private TextMeshProUGUI examplesTextMesh;

    [Header("Tags")]
    [SerializeField] private string closeTag = "</>";
    [SerializeField] private List<ColorTag> colorTags;

    override protected void Awake()
    {
        InitPath();

        base.Awake();
    }

    static private void InitPath()
    {
        GrammarFile = Path.Combine(Application.dataPath, "Resources", "RawData", "grammar.csv");
    }

    #region Initialization
    override protected string GetResourceFile()
    {
        return GrammarFile;
    }

    override protected GrammarLesson CreateData(string[] rawDataArray, char separator)
    {
        string lesson = rawDataArray[2].Replace(closeTag, "</color>");
        string examples = rawDataArray[3].Replace(closeTag, "</color>");

        foreach (ColorTag colorTag in colorTags)
        {
            lesson = lesson.Replace(colorTag.tag, "<color=#" + colorTag.color.ToHexString() + ">");
            examples = examples.Replace(colorTag.tag, "<color=#" + colorTag.color.ToHexString() + ">");
        }
        return new(title: rawDataArray[0],
                    subtitle: rawDataArray[1],
                    lesson: lesson,
                    examples: new(examples.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)));
    }

    override protected void InitButton(int index)
    {
        GrammarLesson lesson = lessons.GetLesson(index);

        buttons[index].SetTitle(lesson.Title);
        buttons[index].SetSubTitle(lesson.Subtitle);
    }
    #endregion Initialization

    override protected void ApplyTemplate()
    {
        GrammarLesson lesson = lessons.GetLesson(currentIndex);

        titleTextMesh.text = lesson.Title;
        subTitleTextMesh.text = lesson.Subtitle;
        lessonTextMesh.text = lesson.LessonBody;

        StringBuilder examples = new();

        foreach (string example in lesson.Examples)
        {
            examples.Append("- " + example + "\n");
        }   
        
        examplesTextMesh.text = examples.ToString();
    }
}
