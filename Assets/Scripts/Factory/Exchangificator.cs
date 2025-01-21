using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Exchangificator : Factory
{
    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Exchangificator);
    }

    private SymbolTable symbolTable = new SymbolTable();

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item leftItem = inputs[0].GetItem();
        Item rightItem = inputs[1].GetItem();
        Item.Symbol leftCharacters = leftItem.GetSymbols()[0];
        Item.Symbol rightCharacters = rightItem.GetSymbols()[0];

        SwitchSymbolVowel(ref leftCharacters, ref rightCharacters);

        yield return new WaitForSeconds(productionTime / productionSpeed);

        ClearInputs();

        BuildingOutput postLeftoutput = outputs[0];
        BuildingOutput postRightoutput = outputs[1];

        Item spawnedItemLeft = SpawnItem(postLeftoutput.transform.position);
        spawnedItemLeft.transform.Translate(Vector3.up * spawnedItemLeft.GetItemHeightOffset());
        spawnedItemLeft.AddCharacter(leftCharacters);

        Item spawnedItemRight = SpawnItem(postRightoutput.transform.position);
        spawnedItemRight.transform.Translate(Vector3.up * spawnedItemRight.GetItemHeightOffset());
        spawnedItemRight.AddCharacter(rightCharacters);

        postLeftoutput.SetItem(spawnedItemLeft);
        postRightoutput.SetItem(spawnedItemRight);

        state = BuildingState.IDLE;
    }

    bool SwitchSymbolVowel(ref Item.Symbol leftCharacters, ref Item.Symbol rightCharacters)
    {
        int consonantIndexLeft, consonantIndexRight, vowelIndexLeft, vowelIndexRight;
        symbolTable.GetSymbolPosition(leftCharacters, out consonantIndexLeft, out vowelIndexLeft);
        symbolTable.GetSymbolPosition(rightCharacters, out consonantIndexRight, out vowelIndexRight);

        leftCharacters.character = symbolTable.GetSymbol(consonantIndexLeft, vowelIndexRight, leftCharacters.type);
        rightCharacters.character = symbolTable.GetSymbol(consonantIndexRight, vowelIndexLeft, rightCharacters.type);

        return true;
    }
}
