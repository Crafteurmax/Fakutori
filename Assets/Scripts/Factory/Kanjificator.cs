using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

public class Kanjificator : Factory
{
    [SerializeField] protected List<Item.Symbol> producedItemCharacters = new List<Item.Symbol>();
    [SerializeField] private KanjificatorOutputSelection outputSelection;
    private string stringOutput = string.Empty;

    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Kanjificator);
    }
    private void Start()
    {
        outputSelection = FindAnyObjectByType<KanjificatorOutputSelection>();
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item inputedItem = inputs[0].GetItem();
        List<string> cachedInput = new List<string>();
        List<Item.Symbol> characters;
        cachedInput.Add(inputedItem.ToString());

        if (!TryGetOutputsInCache(cachedInput, out characters))
        {
            List<string> kanjiOuput = new List<string>();
            if (Library.TryGetKanjiFromKana(inputedItem.ToString(), out kanjiOuput))
            {
                // pour l'instant on prend que le premier mot que l'on trouve a voir si l'on a besoin d'autre chose plus tard
                if (kanjiOuput.Count > 1)
                {
                    //Debug.Log("Number of choices: " + kanjiOuput.Count);
                    //for (int i = 0; i < kanjiOuput.Count; i++) { 
                    //    Debug.Log(kanjiOuput[i].ToString());
                    //}

                    Debug.Log(kanjiOuput.Count);
                    outputSelection.SetButtons(kanjiOuput.Count);
                    List<string> choices = new List<string>();
                    for (int i = 0; i < kanjiOuput.Count; i++)
                    {
                        choices.Add(kanjiOuput[i]);
                    }
                    outputSelection.SetButtonsName(choices);
                    outputSelection.SetRequestor(this);
                    outputSelection.TogglePanel(true);

                    while (stringOutput == string.Empty) yield return null;
                    Debug.Log(stringOutput);
                }
                else if (kanjiOuput.Count == 1)
                {
                    //Debug.Log("Single kanji output: " + kanjiOuput[0].ToString());
                    stringOutput = kanjiOuput[0];
                }

                foreach (char c in stringOutput)
                {
                    Item.Symbol symbole = new Item.Symbol();
                    symbole.character = c;
                    if (Regex.IsMatch(c.ToString(), @"([ぁ-ん])")) symbole.type = Item.SymbolType.Hiragana;
                    else symbole.type = Item.SymbolType.Kanji;
                    characters.Add(symbole);
                }

                AddToCache(cachedInput, characters);
            }
            else
            {
                // le mot n'existe pas, on produit du papier froissé
                Debug.Log("kanji doesn't exist");
            }
        }

        yield return new WaitForSeconds(productionTime / productionSpeed);


        BuildingOutput output = outputs[0];

        Item spawnedItem = SpawnItem(output.transform.position);
        spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());

        foreach (var symbol in characters)
        {
            spawnedItem.AddCharacter(symbol);
        }

        bool isPureKanji = !Regex.IsMatch(spawnedItem.ToString(), @"([ぁ-ん])");
        spawnedItem.transform.Find("Base").GetComponent<MeshRenderer>().material.color = isPureKanji ? Color.yellow : Color.white;

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }

    public void ReceiveChoice(string message)
    {
        outputSelection.TogglePanel(false);
        stringOutput = message;
    }
}
