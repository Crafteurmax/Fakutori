using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{
    [SerializeField] protected float productionTime = 2.0f;
    [SerializeField] protected float productionSpeed = 1.0f;

    [SerializeField] protected List<BuildingInput> inputs = new List<BuildingInput>();
    [SerializeField] protected List<BuildingOutput> outputs = new List<BuildingOutput>();

    public enum BuildingState {
        IDLE,
        RUNNING,
        OUTPUTFULL
    }

    protected BuildingState state;

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
        state = BuildingState.IDLE;
    }
    
    void Update()
    {
        if (state == BuildingState.IDLE) CheckInputsAndOutputs();
    }

    private void ClearInputs() {
        foreach (var input in inputs) {
            input.Reset();
        }
    }
    private void ClearOutputs() {
        foreach (var output in outputs) {
            output.Reset();
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
        ItemFactory.Instance.Release(input.GetItem());
        input.SetItem(null);
    }

    public virtual IEnumerator ProduceItem() {
        yield return null;
    }

    protected Item SpawnItem(Vector3 spawnPosition) {
        Item spawnedItem = ItemFactory.Instance.GetItem();
        spawnedItem.transform.position = spawnPosition;
        return spawnedItem;
    }
}
