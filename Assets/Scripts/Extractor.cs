using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor : Building
{
    [SerializeField] private string extractedCharacter;

    [SerializeField] private BuildingOutput output;

    [SerializeField] private float productionTime = 2.0f;
    [SerializeField] private float productionSpeed = 1.0f;
    private bool isProducing = false;

    private void Update() {
        if (!isProducing && !output.IsOccupied()) StartCoroutine(ProduceItem());
    }

    private IEnumerator ProduceItem() {
        isProducing = true;
        yield return new WaitForSeconds(productionTime / productionSpeed);
        isProducing = false;

        Item item = ItemFactory.Instance.GetItem();
        item.transform.position = output.GetItemPosition(item.GetItemHeightOffset());
        item.SetCharacters(extractedCharacter);

        output.SetItem(item);
    }
}
