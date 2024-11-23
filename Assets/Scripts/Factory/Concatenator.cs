using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concatenator : Factory
{
    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item leftItem = inputs[1].GetItem();
        Item rightItem = inputs[0].GetItem();

        List<string> cachedInput = new List<string>();
        List<Item.Symbol> characters;
        cachedInput.Add(leftItem.ToString());
        cachedInput.Add(rightItem.ToString());
        
        if(!TryGetOutputsInCache(cachedInput, out characters))
        {
            // we merge both inputs together
            List<Item.Symbol> leftCharacters = leftItem.GetSymbols();
            List<Item.Symbol> rightCharacters = rightItem.GetSymbols();

            foreach (var symbol in leftCharacters)
            {
                characters.Add(symbol);
            }
            foreach (var symbol in rightCharacters)
            {
                characters.Add(symbol);
            }

            // we add result to the cache to not do it again
            AddToCache(cachedInput, characters);
        }

        yield return new WaitForSeconds(productionTime / productionSpeed);

        BuildingOutput output = outputs[0];

        Item spawnedItem = SpawnItem(output.transform.position);
        spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());

        foreach (var symbol in characters)
        {
            spawnedItem.AddCharacter(symbol);
        }

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }
}
