using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Kanjificator : Factory
{
    [SerializeField] private string kanjiCharacter;

    private void Awake() {
        kanjiCharacter = "漢";
        producedItemCharcters.Add(kanjiCharacter);
        producedItemCharcters.Add(kanjiCharacter + kanjiCharacter);
    }
}
