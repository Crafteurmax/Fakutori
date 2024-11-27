using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tentenification : Factory
{
    private SymbolTable symbolTable = new SymbolTable();

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item item = inputs[0].GetItem();
        Item.Symbol characters = item.GetSymbols()[0];

        symbolTable.AddTenten(ref characters);

        yield return new WaitForSeconds(productionTime / productionSpeed);

        BuildingOutput output = outputs[0];

        Item outputItem = SpawnItem(output.transform.position);
        outputItem.transform.Translate(Vector3.up * outputItem.GetItemHeightOffset());
        outputItem.AddCharacter(characters);

        output.SetItem(outputItem);

        state = BuildingState.IDLE;
    }
}