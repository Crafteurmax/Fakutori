using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class History
{
    public struct buildingAction
    {
        public Building.BuildingType buildingtype;
        public Vector3Int buildingPosition;
        public Quaternion buildingRotation;
        public bool isPlacement;
        public Dictionary<List<string>, List<Item.Symbol>> machineCache;

        public buildingAction(Building.BuildingType aBuildingType, Vector3Int aPosition, Quaternion aBuildingRotation, bool aBuildingPlacement, [Optional] Dictionary<List<string>, List<Item.Symbol>> aMachineCache)
        {
            buildingtype = aBuildingType;
            buildingPosition = aPosition;
            buildingRotation = aBuildingRotation;
            isPlacement = aBuildingPlacement;
            machineCache = aMachineCache;
        }
    }

    public static History Instance { get; private set; }

    private LimitedSizeStack<List<buildingAction>> history = new LimitedSizeStack<List<buildingAction>>(20);
    private LimitedSizeStack<List<buildingAction>> undone = new LimitedSizeStack<List<buildingAction>>(20);

    private BuildingPlacer buildingPlacer;

    static History()
    {
        Instance = new History();
    }

    //History is initialized by BuildingPlacer
    public void Initialize(BuildingPlacer aBuildingPlacer)
    {
        //#TODO_N temporary solution to the fact that building placer is continuously reset during remove period
        if (buildingPlacer == null)
            buildingPlacer = aBuildingPlacer;
    }

    private History() {}

    //If Redo or Ctrl-Y is pressed, Redo and add the operation to the history pile
    public void Redo()
    {
        if (undone.Count() == 0)
            return;

        List<buildingAction> action = undone.Pop();
        UndoAction(ref action);
        history.Push(action);
    }

    //If Undo or Ctrl-Z is pressed, Undo and add an operation to the undone pile
    public void Undo()
    {
        if (history.Count() == 0)
            return;

        List<buildingAction> action = history.Pop();
        UndoAction(ref action);
        undone.Push(action);
    }

    //Undo action: place building if previous action was a remove, remove if it was a placement
    private void UndoAction(ref List<buildingAction> anAction)
    {
        if (buildingPlacer == null)
        {
            UnityEngine.Debug.LogError("BuildingPlacer is not initialized correctly");
            return;
        }

        for (int i = 0; i < anAction.Count; i++)
        {

            buildingAction currentAction = anAction[i];

            if (currentAction.isPlacement)
            {
                buildingPlacer.RemoveBuildingAtPosition(currentAction.buildingPosition);
            }
            else
            {
                buildingPlacer.PlaceBuildingAtPosition(currentAction.buildingtype, currentAction.buildingPosition, currentAction.buildingRotation);

                if ((int)currentAction.buildingtype >= 8 && (int)currentAction.buildingtype < 17)
                {
                    BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(currentAction.buildingPosition);
                    Factory factory = buildingTile.building.GetComponent<Factory>();

                    foreach (KeyValuePair<List<string>, List<Item.Symbol>> entry in currentAction.machineCache)
                    {
                        factory.AddToCache(entry.Key, entry.Value);
                    }
                }
            }

            currentAction.isPlacement = !currentAction.isPlacement;
            anAction[i] = currentAction;
        }   
    }

    public void AddToHistory(Building.BuildingType aType, Vector3Int aPosition, Quaternion aBuildingRotation, bool isPlacement, [Optional] Dictionary<List<string>, List<Item.Symbol>> aMachineCache)
    {
        buildingAction action = new buildingAction(aType, aPosition, aBuildingRotation, isPlacement, aMachineCache);
        List<buildingAction> actionList = new List<buildingAction>();
        actionList.Add(action);
        history.Push(actionList);
        undone.Clear();
    }

    public void AddListToHistory(List<buildingAction> actionList)
    {
        history.Push(actionList);
    }
}
