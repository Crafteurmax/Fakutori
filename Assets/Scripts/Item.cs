using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject item;

    private float heightOffset;

    private void Awake()
    {
        item = gameObject;
        heightOffset = item.transform.localScale.y / 2;
    }

    public float GetItemHeightOffset()
    {
        return heightOffset;
    }
}
