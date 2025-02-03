using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concatenator : Factory
{
    [SerializeField] private AudioSource anvilSound;
    [SerializeField] private float audioOffset;

    public override void OnEnable()
    {
        base.OnEnable();
        SetBuildingType(BuildingType.Concatenator);
    }

    public override IEnumerator ProduceItem()
    {
        state = BuildingState.RUNNING;

        Item leftItem = inputs[0].GetItem();
        Item rightItem = inputs[1].GetItem();

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

        StartCoroutine(AnvilSound());

        yield return new WaitForSeconds(productionTime / productionSpeed);

        ClearInputs();

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

    private IEnumerator AnvilSound() {
        yield return new WaitForSeconds(audioOffset);
        anvilSound.Play();
    }
}
