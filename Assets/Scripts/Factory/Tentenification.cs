using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public class Tentenification : Factory
{
    private SymbolTable symbolTable = new SymbolTable();

    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Tentenificator);
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item item = inputs[0].GetItem();
        Item.Symbol characters = new Item.Symbol();
        if (item.GetSymbols().Count > 0)
        {
            characters = item.GetSymbols()[0];
            symbolTable.AddTenten(ref characters);
        }

        yield return new WaitForSeconds(productionTime / productionSpeed);

        ClearInputs();

        BuildingOutput output = outputs[0];

        Item outputItem = SpawnItem(output.transform.position);
        outputItem.transform.Translate(Vector3.up * outputItem.GetItemHeightOffset());
        outputItem.AddCharacter(characters);

        output.SetItem(outputItem);

        state = BuildingState.IDLE;
    }
}
