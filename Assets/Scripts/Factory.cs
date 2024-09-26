using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{
    [SerializeField] private float productionTime = 2.0f;
    [SerializeField] private float productionSpeed = 1.0f;

    [SerializeField] protected List<BuildingInput> inputs = new List<BuildingInput>();
    [SerializeField] protected List<BuildingOutput> outputs = new List<BuildingOutput>();

    [SerializeField] protected List<string> producedItemCharcters = new List<string>();

    public enum BuildingState {
        IDLE,
        RUNNING,
        OUTPUTFULL
    }

    private BuildingState state;

    public override void OnEnable() {
        base.OnEnable();
        
        foreach (var input in inputs) {
            input.Initialize();
            BuildingManager.Instance.AddBuildingInput(input.GetPosition(), input);
        }
    }

    public override void OnDisable() {
        base.OnDisable();

        foreach (var input in inputs) {
            BuildingManager.Instance.RemoveBuildingInput(input.GetPosition());
        }
        ClearInputs();
        ClearOutputs();
    }
    
    void Update()
    {
        if (state == BuildingState.IDLE) CheckInputsAndOutputs();
    }

    private void ClearInputs() {
        foreach (var input in inputs) {
            input.ClearItem();
            input.ClearIncomingItem();
        }
    }

    private void ClearOutputs() {
        foreach (var output in outputs) {
            output.ClearItem();
            output.ClearOutgoingItem();
        }
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
            Item spawnedItem = SpawnItem(outputs[i].transform.position);
            spawnedItem.SetCharacters(producedItemCharcters[i]);
            spawnedItem.transform.Translate(Vector3.up * spawnedItem.GetItemHeightOffset());
            spawnedItem.transform.Find("Model").GetComponent<MeshRenderer>().material.color = Color.black;
            outputs[i].SetItem(spawnedItem);
        }

        state = BuildingState.IDLE;
    }

    private Item SpawnItem(Vector3 spawnPosition) {
        Item spawnedItem = ItemFactory.Instance.GetItem();
        spawnedItem.transform.position = spawnPosition;
        return spawnedItem;
    }
}
