using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{
    [SerializeField] private float productionTime = 2.0f;
    [SerializeField] private float productionSpeed = 1.0f;

    [SerializeField] protected List<BuildingInput> inputs = new List<BuildingInput>();
    [SerializeField] protected List<BuildingOutput> outputs = new List<BuildingOutput>();

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
                Destroy(incomingItem.gameObject);
            }
        }
        foreach (var output in outputs) {
            Item outgoingItem = output.GetOutgoingItem();
            if (outgoingItem != null) {
                Destroy(outgoingItem.gameObject);
            }
        }
    }
    
    void Update()
    {
        if (state == BuildingState.IDLE) CheckInputs();
    }

    private void CheckInputs() {
        foreach (var input in inputs) {
            if (input.IsOccupied()) {
                StartCoroutine(ProduceItem());
                break;
            }
        }
    }

    // TODO
    // Faire avancer l'output vers le prochain building

    private IEnumerator ProduceItem() {
        BuildingState state = BuildingState.RUNNING;

        yield return new WaitForSeconds(productionTime / productionSpeed);

        foreach (var output in outputs) {
            if (!output.IsOccupied()) {
                output.SetItem(new Item()); // Assuming Item has a parameterless constructor
                break;
            } else {
                state = BuildingState.OUTPUTFULL;
            }
        }

        if (state == BuildingState.OUTPUTFULL) {
            // Handle output full case (e.g., stop production, notify player, etc.)
        }
    }
}
