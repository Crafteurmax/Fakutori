using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelOutput : MonoBehaviour
{
    [SerializeField] private BuildingOutput buildingOutput;

    public BuildingOutput GetBuildingOutput() {
        return buildingOutput;
    }
}
