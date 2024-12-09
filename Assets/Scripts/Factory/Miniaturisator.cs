using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Miniaturisator : Factory
{
    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Miniaturisator);
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item inputedItem = inputs[0].GetItem();
        List<Item.Symbol> characters = new List<Item.Symbol>();

        foreach(Item.Symbol item in inputedItem.GetSymbols())
        {
            Item.Symbol temp = new Item.Symbol();
     
            if (!((((item.character >= 0x30A1 && item.character <= 0x30AA) // range des voyelles katakana
                || (item.character >= 0x3041 && item.character <= 0x304C)) // range des voyelles hiragana 
                && (item.character % 2 != 0))                              // n'est pas deja petit
                
                || item.character == 0x30C4 || item.character == 0x3064))  // cas a part des tsu
                break;

            temp.type = item.type;
            temp.character = (char)(item.character - 0x01);
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

        output.SetItem(spawnedItem);

        state = BuildingState.IDLE;
    }
}
