using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class JapaneseLesson { }

public interface IJapaneseLessons<Lesson> where Lesson : JapaneseLesson
{
    abstract public void AddLesson(Lesson lesson);

    abstract public void TrimData();

    abstract public List<int> Search(List<int> currentSelection, string value);

    abstract public int GetCount();

    abstract public Lesson GetLesson(int index);

    public static List<int> SearchInLists(List<List<string>> searchLists, List<int> currentSelection, string value)
    {
        return currentSelection
            .Where(selected =>
                searchLists.Any(list =>
                    list[selected].ToLower().Contains(value))
                )
            .ToList();
    }

    public static List<int> SearchInLists(List<List<List<string>>> searchSuperLists, List<int> currentSelection, string value)
    {
        return currentSelection
            .Where(selected =>
                searchSuperLists.Any(listOfList =>
                    listOfList[selected].Any(s => s.ToLower().Contains(value))
                )
            )
            .ToList();
    }

    public static List<int> SearchInLists(List<List<string>> searchLists, List<List<List<string>>> searchSuperLists, List<int> currentSelection, string value)
    {
        return currentSelection
            .Where(selected =>
                searchLists.Any(list =>
                    list[selected].ToLower().Contains(value))
                ||
                searchSuperLists.Any(listOfList =>
                    listOfList[selected].Any(s => s.ToLower().Contains(value))
                )
            )
            .ToList();
    }
}

[RequireComponent(typeof(UIDictionaryManager))]
public abstract class UIDictionary<LessonButton, Lesson, Lessons> : MonoBehaviour
    where LessonButton : DictionaryButton
    where Lesson : JapaneseLesson
    where Lessons : IJapaneseLessons<Lesson>
{
    protected Lessons lessons;
    protected UIDictionaryManager manager;

    [SerializeField] protected LessonButton lessonButtonPrefab;

    [Header("Scroll")]
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] protected SelectableButton categoryButton;
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected RectTransform mainLayout;
    [SerializeField] protected VerticalLayoutGroup pinnedLayout;
    [SerializeField] protected VerticalLayoutGroup searchLayout;
    protected RectTransform searchLayoutRectTransform;

    [Header("Template")]
    [SerializeField] protected GameObject panel;

    [Header("Debug")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private bool debugOn;

    protected readonly List<LessonButton> buttons = new();
    protected readonly List<int> pinnedIndexes = new();
    protected List<int> searchedIndexes = new();
    protected int currentIndex = 0;

    protected string lastSearch;

    virtual protected void Awake()
    {
        manager = GetComponent<UIDictionaryManager>();
        lessons = Activator.CreateInstance<Lessons>();

        searchLayoutRectTransform = searchLayout.GetComponent<RectTransform>();

        searchField.onValueChanged.AddListener(delegate { Search(); });
        categoryButton.onClick.AddListener(delegate { EnableDictionnay(); });

        AddToManager();

        OpenRessourceFile();

        CreateButtons();

        ResetSearch();

        StartCoroutine(LayoutRebuildDelayed());
    }

    private void AddToManager()
    {
        manager.CategoryButtons.Add(categoryButton);
        manager.Panels.Add(panel);
        manager.ScrollRects.Add(scrollRect);
    }

    private IEnumerator LayoutRebuildDelayed()
    {
        while (searchLayoutRectTransform.sizeDelta.y <= 0)
        {
            yield return null;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainLayout);
        OpenPage(Mathf.Max(0, currentIndex));
        GoTo(Mathf.Max(0, currentIndex));
    }

    #region Initialization
    private void OpenRessourceFile()
    {
        string[] rawData = File.ReadAllText(GetResourceFile()).Split("\n", System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rawData.Length; i++)
        {
            string[] rawDataArray = rawData[i].Split(";", System.StringSplitOptions.None);

            Lesson lesson = CreateData(rawDataArray, '|');
            if (lesson != null)
            {
                lessons.AddLesson(lesson);
            }
        }

        lessons.TrimData();
        if (debugOn)
        {
            Verification();
        }
    }

    abstract protected string GetResourceFile();

    abstract protected Lesson CreateData(string[] rawDataArray, char separator);

    virtual protected void Verification() { }

    #region Create Buttons
    protected void CreateButtons()
    {
        for (int i = 0; i < lessons.GetCount(); i++)
        {
            CreateButton(i);
        }
    }

    protected void CreateButton(int index)
    {
        LessonButton button = GameObject.Instantiate<LessonButton>(lessonButtonPrefab);
        buttons.Add(button);

        InitButton(index);

        button.transform.SetParent(searchLayoutRectTransform);
        button.transform.localScale = Vector3.one;
        button.onClick.AddListener(delegate { OpenPageWithRestriction(index); });
        button.GetPinButton().onClick.AddListener(delegate { SwitchPinButton(index); });
        buttons[index].TriggerAlternative(index % 2 != 0);
    }

    abstract protected void InitButton(int index);
    #endregion Create Buttons

    #endregion Initialization

    #region Open Page
    public void OpenPageWithRestriction(int index)
    {
        if (index == currentIndex) { return; }

        OpenPage(index);
    }

    public void OpenPage(int index)
    {
        // Select Button
        buttons[currentIndex].SelectButton(false);
        currentIndex = index;
        buttons[index].SelectButton(true);

        ApplyTemplate();
    }

    abstract protected void ApplyTemplate();

    public void GoTo(int index)
    {
        scrollRect.verticalNormalizedPosition = 1 - (float)index / lessons.GetCount();
    }
    #endregion Open Page

    #region Pin
    private void SwitchPinButton(int index)
    {
        if (pinnedIndexes.Contains(index))
        {
            UnPin(index);
        }
        else
        {
            Pin(index);
        }

        SortAndRecolorPinnedButtons();
        SortAndRecolorSearchedButtons();

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainLayout);
    }

    virtual protected void Pin(int index)
    {
        buttons[index].Pin(true);
        buttons[index].transform.SetParent(pinnedLayout.transform);
        buttons[index].transform.localScale = Vector3.one;

        searchedIndexes.Remove(index);
        pinnedIndexes.Add(index);
        pinnedIndexes.Sort();
    }

    virtual protected void UnPin(int index)
    {
        buttons[index].Pin(false);
        buttons[index].transform.SetParent(searchLayout.transform);
        buttons[index].transform.localScale = Vector3.one;

        pinnedIndexes.Remove(index);
        if (string.IsNullOrEmpty(lastSearch) || lessons.Search(new() { index }, lastSearch).Count > 0)
        {
            searchedIndexes.Add(index);
            searchedIndexes.Sort();
        }
        else
        {
            buttons[index].gameObject.SetActive(false);
        }
    }

    virtual protected void SortAndRecolorPinnedButtons()
    {
        for (int i = 0; i < pinnedIndexes.Count; i++)
        {
            buttons[pinnedIndexes[i]].TriggerAlternative(i % 2 == 0);
            buttons[pinnedIndexes[i]].transform.SetAsLastSibling();
        }
    }

    protected void SortAndRecolorSearchedButtons()
    {
        for (int i = 0; i < searchedIndexes.Count; i++)
        {
            buttons[searchedIndexes[i]].TriggerAlternative(i % 2 == 0);
            buttons[searchedIndexes[i]].transform.SetAsLastSibling();
        }
    }

    #endregion Pin

    #region Search
    private void ResetSearch()
    {
        searchedIndexes = Enumerable.Range(0, lessons.GetCount()).ToList();
        foreach (int pinned in pinnedIndexes)
        {
            searchedIndexes.Remove(pinned);
        }
    }

    public void Search()
    {
        string search = searchField.text.ToLower();

        if (string.IsNullOrEmpty(search))
        {
            ResetSearch();
        }
        else if (string.IsNullOrEmpty(lastSearch) || search[0..^1] != lastSearch)
        {
            ResetSearch();

            searchedIndexes = lessons.Search(searchedIndexes, search); // Update the selection
        }
        else
        {
            searchedIndexes = lessons.Search(searchedIndexes, search); // Update the selection
        }

        lastSearch = search;
        ShowVocabularyButtons();
    }

    private void ShowVocabularyButtons() 
    {
        int select = 0;
        int item = 0;
        while (select < searchedIndexes.Count)
        {
            buttons[item].gameObject.SetActive(item == searchedIndexes[select]);
            if (item == searchedIndexes[select])
            {
                buttons[item].TriggerAlternative(select % 2 != 0);
                select++;
            }

            item++;
        }

        for (int i = item; i < buttons.Count; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }

        foreach (int pinned in pinnedIndexes)
        {
            buttons[pinned].gameObject.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainLayout);
    }
    #endregion Search

    public void EnableDictionnay()
    {
        foreach (SelectableButton _button in manager.CategoryButtons)
        {
            _button.SelectButton(false);
        }
        categoryButton.SelectButton(true);

        foreach (GameObject _panel in manager.Panels)
        {
            _panel.SetActive(false);
        }
        panel.SetActive(true);

        foreach (ScrollRect _scrollRect in manager.ScrollRects)
        {
            _scrollRect.gameObject.SetActive(false);
        }
        scrollRect.gameObject.SetActive(true);
    }

    protected Sprite LoadSprite(string filePath)
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
}
