using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Kanjificator : Factory
{
    [SerializeField] protected List<Item.Symbol> producedItemCharacters = new List<Item.Symbol>();

    private void Awake() {
        /*
        char kanjiCharacter = '漢';
        producedItemCharacters.Add(new Item.Symbol { character = kanjiCharacter, type = Item.SymbolType.Kanji });
        */
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
            List<string> kanjiOuput;
            if(Library.TryGetKanjiFromKana(inputedItem.ToString(), out kanjiOuput))
            {
                // pour l'instant on prend que le premier mot que l'on trouve a voir si l'on a besoin d'autre chose plus tard
                foreach(char c in kanjiOuput[0])
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
}
