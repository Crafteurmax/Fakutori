using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GoalController : MonoBehaviour
{

    private void Awake()
    {
        Instance = this;
    }

    public static GoalController Instance { get; private set; }
    private string levelsPrefabFolder = Application.dataPath + "/Resources/Levels/";
    private List<List<goal>> goals = new List<List<goal>>();

    struct goal
    {
        public string description;
        public int count;
    }

    [SerializeField] GameObject prefab;
    Dictionary<string, IndividualGoalController> displayedGoals = new Dictionary<string, IndividualGoalController>();
    private int actualDisplayedGoalID = -1;


    // Start is called before the first frame update
    void Start()
    {
        if (LevelData.goalFileName != null)
            ReadGoalsFile(LevelData.goalFileName);
        else
            ReadGoalsFile("gls_test");
        loadNextGoalsSet();

        /*AddGoals(new goal { description = "水曜日", count = 20 });
        IncreaseScore("水曜日");*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseScore(string key)
    {
        if (displayedGoals.ContainsKey(key)) displayedGoals[key].Increase();
        if (IsAllDisplayedGoalsAreCompleted()) loadNextGoalsSet();
    }

    private void AddGoals(goal goal)
    {
        GameObject go = Instantiate(prefab, transform);
        IndividualGoalController igc = go.GetComponent<IndividualGoalController>();
        igc.Setup(goal.description,goal.count);
        displayedGoals.Add(goal.description,igc);
    }

    private void ReadGoalsFile(string path)
    {
        string rawData = System.IO.File.ReadAllText(levelsPrefabFolder + path + ".csv");
        string[] rawDataArray = rawData.Split(new string[] { ",", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        goals.Add(new List<goal>());
        for (int i = 2; i < rawDataArray.Length; i += 2)
        {
            if (rawDataArray[i] == "STOP")
            {
                goals.Add(new List<goal>());
                continue;
            }
            goals[goals.Count-1].Add(new goal { description=rawDataArray[i], count = int.Parse(rawDataArray[i+1]) });
            
        }
    }

    private bool IsAllDisplayedGoalsAreCompleted()
    {
        foreach (var goals in displayedGoals)
        {
            if(!goals.Value.IsGoalComplete()) return false;
        }
        return true;
    }

    private void loadNextGoalsSet()
    {
        actualDisplayedGoalID++;
        foreach (var goals in displayedGoals) Destroy(goals.Value.gameObject);
        displayedGoals.Clear();
        if (actualDisplayedGoalID < goals.Count) foreach (goal goals in goals[actualDisplayedGoalID]) AddGoals(goals);

    }
}
