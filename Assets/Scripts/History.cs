using System.Collections.Generic;
using UnityEngine;

public class History
{
    public struct buildingAction
    {
        public Building.BuildingType buildingtype;
        public Vector3Int buildingPosition;
        public Quaternion buildingRotation;
        public bool isPlacement;

        public buildingAction(Building.BuildingType aBuildingType, Vector3Int aPosition, Quaternion aBuildingRotation, bool aBuildingPlacement)
        {
            buildingtype = aBuildingType;
            buildingPosition = aPosition;
            buildingRotation = aBuildingRotation;
            isPlacement = aBuildingPlacement;
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
            }

            currentAction.isPlacement = !currentAction.isPlacement;
            anAction[i] = currentAction;
        }   
    }

    public void AddToHistory(Building.BuildingType aType, Vector3Int aPosition, Quaternion aBuildingRotation, bool isPlacement)
    {
        buildingAction action = new buildingAction(aType, aPosition, aBuildingRotation, isPlacement);
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
