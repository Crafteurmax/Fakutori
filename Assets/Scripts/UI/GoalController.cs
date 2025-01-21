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
        public int id;
        public string display;
        public string description;
        public int count;
        public bool isStop;
    }

    [SerializeField] GameObject prefab;
    Dictionary<string, IndividualGoalController> displayedGoals = new Dictionary<string, IndividualGoalController>();
    private int actualDisplayedGoalID = -1;

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] Story story;
    [SerializeField] PanelManger panelManger;
    [SerializeField] GameObject dictionaryPanel;

    // Start is called before the first frame update
    void Start()
    {
        actualDisplayedGoalID = -1;
        if (LevelData.goalFileName != null)
            ReadGoalsFile(LevelData.goalFileName + "_goal");
        else
            ReadGoalsFile("default_goal");
        loadNextGoalsSet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseScore(string key)
    {

        if (displayedGoals.ContainsKey(key))
        {
            displayedGoals[key].Increase();
            //Debug.Log("Key :" + key);
        }
        if (IsAllDisplayedGoalsAreCompleted()) loadNextGoalsSet();
    }

    private void AddGoal(goal goal)
    {
        GameObject go = Instantiate(prefab, transform);
        IndividualGoalController igc = go.GetComponent<IndividualGoalController>();
        igc.panelManager = panelManger;
        igc.dictionaryPanel = dictionaryPanel;
        igc.Setup(goal.id,goal.display,goal.description,goal.count,goal.isStop);
        displayedGoals.Add(goal.description,igc);
    }

    private void ReadGoalsFile(string path)
    {
        string rawData = System.IO.File.ReadAllText(levelsPrefabFolder + path + ".csv");
        rawData = rawData.Replace("\r", "");
        string[] rawDataArray = rawData.Split(new string[] { ",", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        goals.Add(new List<goal>());
        for (int i = 4; i < rawDataArray.Length; i += 4)
        {
            if (rawDataArray[i] == "STOP")
            {
                if (i +  1 < rawDataArray.Length) goals[goals.Count - 1].Add(new goal { description = rawDataArray[i + 1], isStop = true });
                goals.Add(new List<goal>());
                continue;
            }
            goals[goals.Count-1].Add(new goal { display = rawDataArray[i], description =rawDataArray[i+1], count = int.Parse(rawDataArray[i+2]), id = int.Parse(rawDataArray[i+3])});
            
        }
    }

    private bool IsAllDisplayedGoalsAreCompleted()
    {
        foreach (var goals in displayedGoals)
        {
            if(!goals.Value.IsGoalComplete()) return false;
        }
        //Debug.Log("All goal completed");
        return true;
    }

    private void loadNextGoalsSet()
    {
        //Debug.Log("Next goal is loading");
        if(actualDisplayedGoalID >= goals.Count) return;
        if(actualDisplayedGoalID != -1)
        {
            if(goals[actualDisplayedGoalID].Count == 0) return;
            goal stopGoal = goals[actualDisplayedGoalID][goals[actualDisplayedGoalID].Count - 1];
            if (stopGoal.isStop && stopGoal.description != "STOP")
            {
                story.SetVariable("redo", stopGoal.description);
                panelManger.TogglePanel(dialoguePanel);
            }
        }
        actualDisplayedGoalID++;
        foreach (var goals in displayedGoals) Destroy(goals.Value.gameObject);
        displayedGoals.Clear();
        if (actualDisplayedGoalID < goals.Count)
        {
            foreach (goal goal in goals[actualDisplayedGoalID])
            {
                if(!goal.isStop)
                {
                    //Debug.Log(goal.display);
                    AddGoal(goal);
                }
            }
        }

    }
}
