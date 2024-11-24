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
    private int objective;

    public void Setup(string description, int _objective)
    {
        UIdescription.text = description;
        count = 0;
        UIactualCount.text = count.ToString();
        objective = _objective;
        UIobjective.text = objective.ToString();
    }

    public void Increase()
    {
        count++;
        UIactualCount.text = count.ToString();
    }

    public bool IsGoalComplete()
    {
        return count >= objective;
    }
}
