using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Troncator : Factory
{
    private SymbolTable symbolTable = new SymbolTable();

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item inputedItem = inputs[0].GetItem();
        List<Item.Symbol> leftCharacters = inputedItem.GetSymbols().GetRange(0,inputedItem.GetSymbols().Count - 1);
        Item.Symbol rightCharacter = inputedItem.GetSymbols()[inputedItem.GetSymbols().Count - 1];

        yield return new WaitForSeconds(productionTime / productionSpeed);

        BuildingOutput postLeftoutput = outputs[1];
        BuildingOutput postRightoutput = outputs[0];

        Item spawnedItemLeft = SpawnItem(postLeftoutput.transform.position);
        spawnedItemLeft.transform.Translate(Vector3.up * spawnedItemLeft.GetItemHeightOffset());
        foreach (var item in leftCharacters)
        {
            spawnedItemLeft.AddCharacter(item);
        }

        Item spawnedItemRight = SpawnItem(postRightoutput.transform.position);
        spawnedItemRight.transform.Translate(Vector3.up * spawnedItemRight.GetItemHeightOffset());
        spawnedItemRight.AddCharacter(rightCharacter);    

        postLeftoutput.SetItem(spawnedItemLeft);
        postRightoutput.SetItem(spawnedItemRight);

        state = BuildingState.IDLE;
    }
}
