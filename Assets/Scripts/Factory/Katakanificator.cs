using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Katakanificator : Factory
{
    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Katanificator);
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item inputedItem = inputs[0].GetItem();
        List<Item.Symbol> characters = new List<Item.Symbol>();

        foreach(Item.Symbol item in inputedItem.GetSymbols())
        {
            Item.Symbol temp = new Item.Symbol();
     
            if (!(item.character >= 0x3041 && item.character <= 0x3096)) // range des hiraganas
                break;

            temp.type = Item.SymbolType.Katakana;
            temp.character = (char)(item.character + 0x60);
            characters.Add(temp);
        }

        yield return new WaitForSeconds(productionTime / productionSpeed);


        BuildingOutput output = outputs[0];

        Item spawnedItem = SpawnItem(output.transform.position);
        spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());

        foreach (var symbol in characters)
        {
            spawnedItem.AddCharacter(symbol);
        }

        spawnedItem.transform.Find("Base").GetComponent<MeshRenderer>().material.color = Color.magenta;

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }
}
