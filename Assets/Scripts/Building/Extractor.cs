using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class Extractor : Building
{
    [Header("Extractor settings")]
    [SerializeField] private float productionTime = 2.0f;
    [SerializeField] private float productionSpeed = 1.0f;
    [SerializeField] private List<Item.Symbol> extractedCharacters;

    [Header("References")]
    [SerializeField] private Animator extractorAnimator;

    private BuildingOutput output;
    
    private bool isProducing = false;
    private bool isSetup;

    private void Awake() {
        output = GetComponentInChildren<BuildingOutput>();
        SetBuildingType(BuildingType.Extractor);
    }

    public void Update() {
        if (isSetup && !isProducing && !output.IsOccupied()) StartCoroutine(ProduceItem());
    }

    public override void OnEnable() 
    {
        base.OnEnable();
        StartCoroutine(SetUpWhenWorldIsFinishBeingBuild());
    }

    private IEnumerator ProduceItem() {
        isProducing = true;
        extractorAnimator.SetTrigger("Produce");
        extractorAnimator.SetFloat("Speed", productionSpeed);
        yield return new WaitForSeconds(productionTime / productionSpeed);
        isProducing = false;

        Item item = ItemFactory.Instance.GetItem();
        item.transform.position = output.GetItemPosition(item.GetItemHeightOffset());
        foreach (var character in extractedCharacters) {
            item.AddCharacter(character);
        }

        output.SetItem(item);
    }

    private IEnumerator SetUpWhenWorldIsFinishBeingBuild()
    {
        while (!WorldBuilder.Instance) yield return null;
        while (!WorldBuilder.Instance.isTheWorldComplete) yield return null;
        Vector3Int pos = Vector3Int.CeilToInt(transform.position);
        pos -= Vector3Int.one;
        pos.y = pos.z;
        pos.z = 0;
        Tile tile = WorldBuilder.Instance.map.GetTile<Tile>(pos);
        if (tile)
        {
            extractedCharacters.Clear();
            char c = tile.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text[0];
            extractedCharacters.Add(new Item.Symbol{character = c, type = Item.SymbolType.Hiragana });
        }
        isSetup = true;
    }
}
