using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{
    [SerializeField] private float productionTime = 2.0f;
    [SerializeField] private float productionSpeed = 1.0f;

    [SerializeField] protected List<BuildingInput> inputs = new List<BuildingInput>();
    [SerializeField] protected List<BuildingOutput> outputs = new List<BuildingOutput>();

    [SerializeField] protected List<Item> producedItemPrefabs = new List<Item>();

    public enum BuildingState {
        IDLE,
        RUNNING,
        OUTPUTFULL
    }

    private BuildingState state;

    public override void OnEnable() {
        base.OnEnable();
        
        foreach (var input in inputs) {
            input.SetIncomingItem(null);
        }
    }

    public override void OnDisable() {
        base.OnDisable();

        foreach (var input in inputs) {
            Item incomingItem = input.GetIncomingItem();
            if (incomingItem != null) {
                BuildingManager.Instance.RemoveBuildingInput(input.GetPosition());
                ItemFactory.Instance.Release(incomingItem);
            }
        }
        foreach (var output in outputs) {
            Item outgoingItem = output.GetOutgoingItem();
            if (outgoingItem != null) {
                ItemFactory.Instance.Release(outgoingItem);
            }
        }
    }
    
    void Update()
    {
        if (state == BuildingState.IDLE) CheckInputsAndOutputs();
    }

    private void CheckInputsAndOutputs() {
        bool inputFull = true;
        bool outputFull = false;
        foreach (var input in inputs) {
            if (!input.IsOccupied()) {
                inputFull = false;
                break;
            }
        }
        foreach (var output in outputs) {
            if (output.IsOccupied()) {
                outputFull = true;
                break;
            }
        }
        if (inputFull && !outputFull) {
            StartCoroutine(ProduceItem());
            foreach (var input in inputs) {
                ClearInput(input);
            }
        }
    }

    private void ClearInput(BuildingInput input) {
        Item inputItem = input.GetItem();
        input.SetItem(null);
        ItemFactory.Instance.Release(inputItem);
    }

    // TODO
    // Faire avancer l'output vers le prochain building

    private IEnumerator ProduceItem() {
        state = BuildingState.RUNNING;
        
        yield return new WaitForSeconds(productionTime / productionSpeed);

        for (int i = 0; i < outputs.Count; i++) {
            Item producedItem = producedItemPrefabs[i];
            Item spawnedItem = SpawnItem(producedItem, outputs[i].transform.position);
            spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());
            outputs[i].SetItem(spawnedItem);
        }

        state = BuildingState.IDLE;
    }

    private Item SpawnItem(Item spawnedItem, Vector3 spawnPosition) {
        return Instantiate(spawnedItem, spawnPosition, Quaternion.identity);
    }
}
