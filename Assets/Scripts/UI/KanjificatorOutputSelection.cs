using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KanjificatorOutputSelection : MonoBehaviour
{
    [SerializeField] private GameObject kanjiSelectionPanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private GridLayoutGroup buttonLayout;
    [SerializeField] private List<Button> choiceButtonList;
    [SerializeField] private int numberOfChoice = 2;
    [SerializeField] private int maxButtonNumber = 5;
    [SerializeField] private GameObject centerPoint;

    private Kanjificator requestor;

    private void Start()
    {
        CreateSelectionButtons();
    }

    private void CreateSelectionButtons()
    {
        for (int i = 0; i < maxButtonNumber; i++)
        {
            CreateSelectionButton(i);
        }
    }

    private void CreateSelectionButton(int index)
    {
        Button button = Instantiate(choiceButtonPrefab);
        button.transform.SetParent(buttonLayout.transform);
        button.onClick.AddListener(delegate { SendSelectedOutput(index); });

        button.GetComponentInChildren<TMP_Text>().text = index.ToString();
        button.gameObject.SetActive(false);

        choiceButtonList.Add(button);
    }

    #region Setter / Getter
    public void SetButtons(int numberOfChoice)
    {
        this.numberOfChoice = numberOfChoice;

        for (int i = 0; i < numberOfChoice; i++)
        {
            choiceButtonList[i].gameObject.SetActive(true);
        }
    }

    public void SetButtonsName(List<string> choiceNames)
    {
        for (int i = 0; i < numberOfChoice; i++)
        {
            choiceButtonList[i].GetComponentInChildren<TMP_Text>().text = choiceNames[i];
        }
    }

    public void SetRequestor(Kanjificator req)
    {
        this.requestor = req;
    }
    #endregion

    public void TogglePanel(bool enabled)
    {
        kanjiSelectionPanel.SetActive(enabled);
        if (enabled)
        {
            centerPoint.transform.position = requestor.transform.position;
        }
    }

    public void SendSelectedOutput(int index)
    {
        //Debug.Log("Button clicked " +  index);
        requestor.ReceiveChoice(choiceButtonList[index].GetComponentInChildren<TMP_Text>().text);
    }
}
