using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concatenator : Factory
{
    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;
        
        Item leftItem = inputs[0].GetItem();
        Item rightItem = inputs[1].GetItem();

        List<Item.Symbol> leftCharacters = leftItem.GetSymbols();
        List<Item.Symbol> rightCharacters = rightItem.GetSymbols();

        List<Item.Symbol> characters = new List<Item.Symbol>();

        foreach (var symbol in leftCharacters) {
            characters.Add(symbol);
        }
        foreach (var symbol in rightCharacters) {
            characters.Add(symbol);
        }


        // ça marche pas car les items droite de gauche sont reconditionées dans le itempool quand

        yield return new WaitForSeconds(productionTime / productionSpeed);

        BuildingOutput output = outputs[0];

        Item spawnedItem = SpawnItem(output.transform.position);
        spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());

        foreach (var symbol in characters) {
            spawnedItem.AddCharacter(symbol);
        }

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }
}
