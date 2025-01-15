using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private string levelName;
    [SerializeField] private TMP_Text buttonText;


    private void Start()
    {
        buttonText.text = levelName;
    }

    public void OnClick()
    {
        Debug.Log("Button " + levelName + "clicked");

        LevelData.levelName = levelName;
        LevelData.goalFileName = levelName + "_goal";

        SceneManager.LoadScene("PlayGround");
    }
}
