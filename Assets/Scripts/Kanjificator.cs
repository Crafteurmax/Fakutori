using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kanjificator : Factory
{
    [SerializeField] private Item kanjiPrefab;

    private void Awake() {
        producedItemPrefabs.Add(kanjiPrefab);
    }
}
