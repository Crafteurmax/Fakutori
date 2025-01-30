using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class IndividualGoalController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private int goalId;
    [SerializeField] TextMeshProUGUI UIdescription;
    [SerializeField] TextMeshProUGUI UIactualCount;
    private int count;
    [SerializeField] TextMeshProUGUI UIobjective;
    [SerializeField] TextMeshProUGUI UIseparator;
    [SerializeField] public PanelManger panelManager;
    [SerializeField] public GameObject dictionaryPanel;
    private int objective;
    private bool isCompleted;
    public Color goalColor;

    public void Setup(int id, string display, string description, int _objective, bool isStop)
    {
        goalId = id;
        UIdescription.text = display;
        count = 0;
        UIactualCount.text = count.ToString();
        objective = _objective;
        UIobjective.text = objective.ToString();
        if (isStop) isCompleted = true;
        goalColor = Color.white;
    }

    public void Increase()
    {
        count++;
        UIactualCount.text = count.ToString();
    }

    public bool IsGoalComplete()
    {
        if (isCompleted) return true;
        if(count >= objective)
        {
            ChangeTexteColor(Color.green);
            isCompleted = true;
        }
        return isCompleted;
    }

    private void ChangeTexteColor(Color texteColor)
    {
        UIdescription.color = texteColor;
        UIactualCount.color = texteColor;
        UIobjective.color = texteColor;
        UIseparator.color = texteColor;
        goalColor = texteColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (goalId == -1) return;
        panelManager.TogglePanel(dictionaryPanel);
        VocabularyDictionary.Instance.EnableDictionnay();
        VocabularyDictionary.Instance.OpenPage(goalId);
        VocabularyDictionary.Instance.GoTo(goalId);
        UIdescription.color = goalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIdescription.color = Color.gray;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        UIdescription.color = goalColor;
    }
}
