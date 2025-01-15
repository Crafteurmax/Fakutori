using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{
    [SerializeField] protected float productionTime = 2.0f;
    [SerializeField] protected float productionSpeed = 1.0f;

    [SerializeField] protected List<BuildingInput> inputs = new List<BuildingInput>();
    [SerializeField] protected List<BuildingOutput> outputs = new List<BuildingOutput>();

    private int cacheSize = 10;
    private Dictionary<List<string>, List<Item.Symbol>> cache = new Dictionary<List<string>, List<Item.Symbol>>();
    private Queue<List<string>> queue = new Queue<List<string>>();

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

    protected bool IsItemsInCache(List<string> items) 
    {
        // On cherche dans tous les Inputs enregistré dans le cache
        foreach (var pairs in cache)
        {
            // Si chaque input est bon
            for (var i = 0; i < pairs.Key.Count; i++)
            {
                if (!pairs.Key[i].Equals(items[i])) break;
                if (i == pairs.Key.Count - 1) return true;
            }
        }
        // si on ne le trouve pas on envoie -1
        return false;
    }

    protected bool TryGetOutputsInCache(List<string> inputs, out List<Item.Symbol> cachedOutputs)
    {
        cachedOutputs = new List<Item.Symbol>();

        // On cherche dans tous les Inputs enregistré dans le cache
        foreach (var pairs in cache)
        {
            // Si chaque input est bon
            for (var i = 0; i < pairs.Key.Count; i++)
            {
                if (!pairs.Key[i].Equals(inputs[i])) break;
                if (i == pairs.Key.Count - 1)
                {
                    cachedOutputs = cache[pairs.Key];
                    return true;
                }
            }
        }
        return false;
    }

    public void AddToCache(List<string> inputs, List<Item.Symbol> outputs) 
    { 
        // pas besoin d'ajouter l'item si il est deja dans le cache
        if (IsItemsInCache(inputs)) return;

        // si le cache est deja remplie, on vide l'item le plus vieux
        if (queue.Count > cacheSize) cache.Remove(queue.Dequeue());

        // Update du cache et de la l'historique
        queue.Enqueue(inputs);
        cache.Add(inputs, outputs);
    }

    public Dictionary<List<string>, List<Item.Symbol>> GetFactoryCache()
    {
        return cache;
    }
}
