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

    [Header("Mesh")]
    [SerializeField] private GameObject beltMeshLeft;
    [SerializeField] private GameObject beltMeshRight;

    private bool takeFromLeft;

    [SerializeField] private BuildingOutput leftOutput;
    private bool isSendingToLeft;
    [SerializeField] private BuildingOutput rightOutput;
    private bool isSendingToRight;

    private void Awake() {
        SetBuildingType(BuildingType.Splitter);
    }

    public override void OnEnable() {
        base.OnEnable();
        leftInput.Initialize();
        rightInput.Initialize();
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

        leftInputState = InputState.LeftOutput;
        rightInputState = InputState.RightOutput;

        base.OnDisable();
    }

    private void Update() {
        HandleMoveItems();
    }

    private void HandleMoveItems() {
        if (!(leftInput.IsOccupied() && leftOutput.IsOccupied())) {
            LeftBeltAnimation();
        }
        if (!(rightInput.IsOccupied() && rightOutput.IsOccupied())) {
            RightBeltAnimation();
        }
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

        float movingTime = Vector3.Distance(leftOutput.transform.position, leftInput.transform.position) / BuildingManager.Instance.beltSpeed;

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

        output.SetItem(movingItem);
    }

    private void RightBeltAnimation() {
        float offset = BuildingManager.Instance.beltSpeed * 0.5f;
        beltMeshRight.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, offset * Time.deltaTime);
    }

    private void LeftBeltAnimation() {
        float offset = BuildingManager.Instance.beltSpeed * 0.5f;
        beltMeshLeft.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, offset * Time.deltaTime);
    }
}
