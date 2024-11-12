using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Exchangificator : Factory
{
    private HiraganaTable hiraganaTable = new HiraganaTable();

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item leftItem = inputs[0].GetItem();
        Item rightItem = inputs[1].GetItem();
        Item.Symbol leftCharacters = leftItem.GetSymbols()[0];
        Item.Symbol rightCharacters = rightItem.GetSymbols()[0];

        SwitchSymbolVowel(ref leftCharacters, ref rightCharacters);

        yield return new WaitForSeconds(productionTime / productionSpeed);

        BuildingOutput postLeftoutput = outputs[0];
        BuildingOutput postRightoutput = outputs[1];

        Item spawnedItemLeft = SpawnItem(postLeftoutput.transform.position);
        spawnedItemLeft.transform.Translate(Vector3.up * spawnedItemLeft.GetItemHeightOffset());
        spawnedItemLeft.AddCharacter(rightCharacters);

        Item spawnedItemRight = SpawnItem(postRightoutput.transform.position);
        spawnedItemRight.transform.Translate(Vector3.up * spawnedItemRight.GetItemHeightOffset());
        spawnedItemRight.AddCharacter(leftCharacters);

        postLeftoutput.SetItem(spawnedItemLeft);
        postRightoutput.SetItem(spawnedItemRight);

        state = BuildingState.IDLE;
    }

    bool SwitchSymbolVowel(ref Item.Symbol leftCharacters, ref Item.Symbol rightCharacters)
    {
        int consonantIndexLeft, consonantIndexRight, vowelIndexLeft, vowelIndexRight;
        hiraganaTable.GetSymbolPosition(leftCharacters.character, out consonantIndexLeft, out vowelIndexLeft);
        hiraganaTable.GetSymbolPosition(rightCharacters.character, out consonantIndexRight, out vowelIndexRight);

        leftCharacters.character = hiraganaTable.GetSymbol(consonantIndexLeft, vowelIndexRight);
        rightCharacters.character = hiraganaTable.GetSymbol(consonantIndexRight, vowelIndexLeft);

        return true; //#TODO_N deal with failure if incompatible (te -> wa for instance) + add coresponding if
    }
}
