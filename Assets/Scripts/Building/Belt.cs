using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Building
{
    private static int beltID = 0;

    private BuildingInput beltInput;
    private BuildingOutput beltOutput;

    [Header("Mesh")]
    [SerializeField] private GameObject beltMesh;

    private void Awake() {
        SetBuildingType(BuildingType.Belt);
        beltInput = GetComponentInChildren<BuildingInput>();
        beltOutput = GetComponentInChildren<BuildingOutput>();

        beltInput.SetIsBeltInput(true);

        beltID++;
        gameObject.name = "Belt" + beltID;
    }

    public override void OnEnable() {
        beltInput.Initialize();
        BuildingManager.Instance.AddBuildingInput(beltInput.GetPosition(), beltInput);

        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();
        Release();
    }

    public override void Release() {
        beltInput.Reset();
        beltOutput.Reset();
  
        BuildingManager.Instance.RemoveBuildingInput(beltInput.GetPosition());
        
        base.Release();
    }

    private void Update() {
        beltInput.SetOutputFull(beltOutput.IsOccupied());
        if (beltInput.IsOccupied() && !beltOutput.IsOccupied() && !beltOutput.IsMovingItem()) {
            beltOutput.SetItem(beltInput.GetItem());
            beltInput.SetOutputFull(true);
            beltInput.SetItem(null);
        }
        if (!beltOutput.IsOccupied()) BeltAnimation();
    }

    private void BeltAnimation() {
        float offset = BuildingManager.Instance.beltSpeed * 0.5f;
        beltMesh.GetComponent<Renderer>().material.mainTextureOffset -= new Vector2(0, offset * Time.deltaTime);
    }
}