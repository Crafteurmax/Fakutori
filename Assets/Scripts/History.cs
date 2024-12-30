using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;

public class History
{
    struct buildingAction
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

    private LimitedSizeStack<buildingAction> history = new LimitedSizeStack<buildingAction>(20);
    private LimitedSizeStack<buildingAction> undone = new LimitedSizeStack<buildingAction>(20);

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

        buildingAction action = undone.Pop();
        UndoAction(ref action);
        history.Push(action);
    }

    //If Undo or Ctrl-Z is pressed, Undo and add an operation to the undone pile
    public void Undo()
    {
        if (history.Count() == 0)
            return;

        buildingAction action = history.Pop();
        UndoAction(ref action);
        undone.Push(action);
    }

    //Undo action: place building if previous action was a remove, remove if it was a placement
    private void UndoAction(ref buildingAction anAction)
    {
        if (buildingPlacer == null)
        {
            UnityEngine.Debug.LogError("BuildingPlacer is not initialized correctly");
            return;
        }

        if (anAction.isPlacement)
            buildingPlacer.RemoveBuildingAtPosition(anAction.buildingPosition);
        else
            buildingPlacer.PlaceBuildingAtPosition(anAction.buildingtype, anAction.buildingPosition, anAction.buildingRotation);

        anAction.isPlacement = !anAction.isPlacement;
    }

    public void AddToHistory(Building.BuildingType aType, Vector3Int aPosition, Quaternion aBuildingRotation, bool isPlacement)
    {
        history.Push(new buildingAction(aType, aPosition, aBuildingRotation, isPlacement));
        undone.Clear();
    }
}
