using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filler : Building
{
    private void Awake()
    {
        SetBuildingType(BuildingType.Filler);
    }
}
