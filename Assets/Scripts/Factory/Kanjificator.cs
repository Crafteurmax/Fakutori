using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kanjificator : Factory
{
    [SerializeField] protected List<Item.Symbol> producedItemCharacters = new List<Item.Symbol>();

    private void Awake() {
        char kanjiCharacter = '漢';
        producedItemCharacters.Add(new Item.Symbol { character = kanjiCharacter, type = Item.SymbolType.Kanji });
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;
        
        yield return new WaitForSeconds(productionTime / productionSpeed);

        for (int i = 0; i < outputs.Count; i++) {
            Item spawnedItem = SpawnItem(outputs[i].transform.position);
            spawnedItem.SetCharacters(producedItemCharacters);
            spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());
            spawnedItem.transform.Find("Base").GetComponent<MeshRenderer>().material.color = Color.yellow;
            outputs[i].SetItem(spawnedItem);
        }

        state = BuildingState.IDLE;
    }
}
