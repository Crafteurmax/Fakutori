using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
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

    // Start is called before the first frame update
    void Start()
    {
        SetUpNode();
    }

    public void NextAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Canceled) return;
        if (currentNode.GetNextNodes().Count == 1)
        {
            story.NextNode();
            SetUpNode();
        }
    }

    public void NextChoice(int id)
    {
        story.ChooseNextNode(id);
        SetUpNode();
    }

    private void SetUpNode()
    {
        currentNode = story.GetCurrentNode();
        text.text = currentNode.getText();
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
}
