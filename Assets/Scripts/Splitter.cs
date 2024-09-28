using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : Building
{
    public enum InputState {
        RightOutput,
        LeftOutput
    }

    [SerializeField] private BuildingInput leftInput;
    private InputState leftInputState = InputState.LeftOutput;
    [SerializeField] private BuildingInput rightInput;
    private InputState rightInputState = InputState.RightOutput;

    private bool takeFromLeft;
    private float movingTime;

    [SerializeField] private BuildingOutput leftOutput;
    private bool isSendingToLeft;
    [SerializeField] private BuildingOutput rightOutput;
    private bool isSendingToRight;

    private void Awake() {
        movingTime = Vector3.Distance(leftOutput.transform.position, leftInput.transform.position) / BuildingManager.Instance.beltSpeed;
    }

    public override void OnEnable() {
        BuildingManager.Instance.AddBuildingInput(leftInput.GetPosition(), leftInput);
        BuildingManager.Instance.AddBuildingInput(rightInput.GetPosition(), rightInput);
    }

    public override void OnDisable() {
        BuildingManager.Instance.RemoveBuildingInput(leftInput.GetPosition());
        BuildingManager.Instance.RemoveBuildingInput(rightInput.GetPosition());

        leftInput.Reset();
        rightInput.Reset();
        leftOutput.Reset();
        rightOutput.Reset();

        base.OnDisable();
    }

    private void Update() {
        HandleMoveItems();
    }

    private void HandleMoveItems() {
        if (leftOutput.IsOccupied() && rightOutput.IsOccupied()) {
            return;
        }
        if (!leftInput.IsOccupied() && rightInput.IsOccupied()) {
            MoveRightItem();
        } else if (leftInput.IsOccupied() && !rightInput.IsOccupied()) {
            MoveLeftItem();
        } else if (leftInput.IsOccupied() && rightInput.IsOccupied()) {
            if (takeFromLeft) {
                MoveLeftItem();
                takeFromLeft = false;
            } else {
                MoveRightItem();
                takeFromLeft = true;
            }
        }
    }

    private void MoveLeftItem() {
        if (!leftOutput.IsOccupied() && rightOutput.IsOccupied()) {
            if (isSendingToLeft) return;
            StartCoroutine(MoveItem(leftInput, leftOutput));
            isSendingToLeft = true;
            leftInputState = InputState.RightOutput;
        } else if (leftOutput.IsOccupied() && !rightOutput.IsOccupied()) {
            if (isSendingToRight) return;
            StartCoroutine(MoveItem(leftInput, rightOutput));
            isSendingToRight = true;
            leftInputState = InputState.LeftOutput;
        } else {
            if (leftInputState == InputState.RightOutput) {
                if (isSendingToRight) return;
                StartCoroutine(MoveItem(leftInput, rightOutput));
                isSendingToRight = true;
                leftInputState = InputState.LeftOutput;
            } else {
                if (isSendingToLeft) return;
                StartCoroutine(MoveItem(leftInput, leftOutput));
                isSendingToLeft = true;
                leftInputState = InputState.RightOutput;
            }
        }
    }
    private void MoveRightItem() {
        if (!leftOutput.IsOccupied() && rightOutput.IsOccupied()) {
            if (isSendingToLeft) return;
            StartCoroutine(MoveItem(rightInput, leftOutput));
            isSendingToLeft = true;
            rightInputState = InputState.RightOutput;
        } else if (leftOutput.IsOccupied() && !rightOutput.IsOccupied()) {
            if (isSendingToRight) return;
            StartCoroutine(MoveItem(rightInput, rightOutput));
            isSendingToRight = true;
            rightInputState = InputState.LeftOutput;
        } else {
            if (rightInputState == InputState.RightOutput) {
                if (isSendingToRight) return;
                StartCoroutine(MoveItem(rightInput, rightOutput));
                isSendingToRight = true;
                rightInputState = InputState.LeftOutput;
            } else {
                if (isSendingToLeft) return;
                StartCoroutine(MoveItem(rightInput, leftOutput));
                isSendingToLeft = true;
                rightInputState = InputState.RightOutput;
            }
        }
    }

    private IEnumerator MoveItem(BuildingInput input, BuildingOutput output) {
        Item movingItem = input.GetItem();
        input.SetItem(null);

        Vector3 initialPosition = movingItem.transform.position;
        Vector3 targetPosition = output.GetItemPosition(movingItem.GetItemHeightOffset());

        float t = 0;

        while (t < movingTime && movingItem != null && movingItem.gameObject.activeSelf) {
            t += Time.deltaTime;
            movingItem.transform.position = Vector3.Lerp(initialPosition, targetPosition, t / movingTime);

            yield return null;
        }

        movingItem.transform.position = targetPosition;

        if (output == leftOutput) {
            isSendingToLeft = false;
        } else {
            isSendingToRight = false;
        }

        // while (movingItem != null && movingItem.gameObject.activeSelf && movingItem.transform.position != targetPosition) {
        //     movingItem.transform.position = Vector3.MoveTowards(movingItem.transform.position, targetPosition, BuildingManager.Instance.beltSpeed * Time.deltaTime);

        //     yield return null;
        // }

        output.SetItem(movingItem);
    }
}
