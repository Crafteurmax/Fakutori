using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Hiraganificator : Factory
{

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item inputedItem = inputs[0].GetItem();
        List<string> cachedInput = new List<string>();
        List<Item.Symbol> characters;
        cachedInput.Add(inputedItem.ToString());

        if (!TryGetOutputsInCache(cachedInput, out characters))
        {
            List<string> kanaOuput;
            if(Library.TryGetKanaFromKanji(inputedItem.ToString(), out kanaOuput))
            {
                // pour l'instant on prend que le premier mot que l'on trouve a voir si l'on a besoin d'autre chose plus tard
                foreach(char c in kanaOuput[0])
                {
                    Item.Symbol symbole = new Item.Symbol();
                    symbole.character = c;
                    symbole.type = Item.SymbolType.Hiragana;
                    characters.Add(symbole);
                }

                AddToCache(cachedInput, characters);
            }
            else
            {
                // le mot n'existe pas, on produit du papier froissé
                Debug.Log("kana doesn't exist");
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

        spawnedItem.transform.Find("Base").GetComponent<MeshRenderer>().material.color = Color.green;

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }
}
