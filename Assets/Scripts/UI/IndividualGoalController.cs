using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndividualGoalController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI UIdescription;
    [SerializeField] TextMeshProUGUI UIactualCount;
    private int count;
    [SerializeField] TextMeshProUGUI UIobjective;
    [SerializeField] TextMeshProUGUI UIseparator;
    private int objective;
    private bool isCompleted;

    public void Setup(string display, string description, int _objective, bool isStop)
    {
        UIdescription.text = display;
        count = 0;
        UIactualCount.text = count.ToString();
        objective = _objective;
        UIobjective.text = objective.ToString();
        if (isStop) isCompleted = true;
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
    }
}
