using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] Story story;

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI pnjName;

    [SerializeField] GameObject choicesBox;
    [SerializeField] Image image;

    [SerializeField] GameObject choiceButtonPrefab;

    StoryNode currentNode;

    [SerializeField] PanelManger panelManger;
    [SerializeField] WorldSaver worldSaver;

    [SerializeField] Image actualFace;
    [SerializeField] List<Sprite> faces;

    [SerializeField] GameObject victoryPanel;
    [SerializeField] Button nextLevelButton;

    private bool isFirstTime = true;

    // Start is called before the first frame update
    void Start()
    {
        if (LevelData.dialoguename != null) story.SetNextNode(LevelData.dialoguename);
        text.text = "it's good";
        SetUpNode();
    }

    public void OnEnable()
    {
        if (isFirstTime)
        {
            isFirstTime = false;
            return; 
        }
        story.SetNextNode(story.GetVariable("redo"));
        SetUpNode();
    }

    public void NextAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Canceled) return;

        List<NextNode> nextNodes = currentNode.GetNextNodes();
        if (nextNodes.Count == 1)
        {
            story.NextNode();
            SetUpNode();
        }
        else if (nextNodes.Count == 0)
        {
            if (currentNode.HasTag("END"))
            {
                //Debug.Log("End of dialogue");
                worldSaver.WriteData(LevelData.mapName);
                panelManger.TogglePanel(victoryPanel);
                return;
                //SceneManager.LoadScene("Menu"); 
            }
            panelManger.ReturnToPreviousPanel();
        }
    }

    public void NextChoice(int id)
    {
        story.ChooseNextNode(id);
        SetUpNode();
    }

    public void NextLevel()
    {
        if (LevelData.currentLevelIndex + 1 >= LevelData.currentPanel.levelList.Count)
        {
            nextLevelButton.interactable = false;
        }
        else
        {
            nextLevelButton.interactable = true;
            LevelData.currentLevelIndex += 1;
            
            LevelData.mapName = LevelData.currentPanel.levelList[LevelData.currentLevelIndex].map;
            LevelData.goalFileName = LevelData.currentPanel.levelList[LevelData.currentLevelIndex].goal;
            LevelData.dialoguename = LevelData.currentPanel.levelList[LevelData.currentLevelIndex].dialogue;

            SceneManager.LoadScene("PlayGround");
        }
    }

    private void SetUpNode()
    {
        currentNode = story.GetCurrentNode();
        text.text = currentNode.getText();
        Sprite sprite = currentNode.GetSprite();
        setFace();


        if (sprite != null)
        {
            Debug.Log("inside");
            image.enabled = true;
            image.sprite = sprite;
        }
        else image.enabled = false;

        pnjName.text = ChoosePNJName();

        List<NextNode> nextNodes = currentNode.GetNextNodes();
        if (nextNodes.Count > 1)
        {
            choicesBox.SetActive(true);
            foreach (Transform transfom in choicesBox.transform) Destroy(transfom.gameObject);
            for(int i = 0; i < nextNodes.Count;i++)
            {
                GameObject go = Instantiate(choiceButtonPrefab, choicesBox.transform);
                go.GetComponent<DialogueChoice>().SetupButton(i,nextNodes[i].display,this);
            }
        }
        else choicesBox.SetActive(false);
    }

    private string ChoosePNJName()
    {
        if (currentNode.HasTag("N1P0N")) return "N1P0N";
        else return "UNKNOWN";
    }

    private void setFace()
    {
        if (currentNode.HasTag("UwU")) actualFace.sprite = faces[0];
        else if (currentNode.HasTag("OnO")) actualFace.sprite = faces[1];
        else if (currentNode.HasTag(">-<")) actualFace.sprite = faces[2];
        else if (currentNode.HasTag("=w=")) actualFace.sprite = faces[3];
        else if (currentNode.HasTag("T_T")) actualFace.sprite = faces[4];
        else if (currentNode.HasTag(":3")) actualFace.sprite = faces[5];
        else if (currentNode.HasTag("<3")) actualFace.sprite = faces[6];
        else if (currentNode.HasTag("x)")) actualFace.sprite = faces[7];
        else if (currentNode.HasTag("\\\\o/")) actualFace.sprite = faces[8];
        else if (currentNode.HasTag("$_$")) actualFace.sprite = faces[9];
        else if (currentNode.HasTag("O.O")) actualFace.sprite = faces[10];
        else if (currentNode.HasTag("X_X")) actualFace.sprite = faces[11];
        else if (currentNode.HasTag("OwO")) actualFace.sprite = faces[12];
        else if (currentNode.HasTag("?_?")) actualFace.sprite = faces[13];
        else actualFace.sprite = faces[0];
    }
}
