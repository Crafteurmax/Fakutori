using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance { get; private set; }

    private readonly Stack<Item> itemPool = new Stack<Item>();

    [Header("Prefabs")]
    [SerializeField] private Item itemPrefab;

    private void Awake() {
        Instance = this;
    }

    public Item GetItem() {
        Item item;

        itemPool.TryPop(out item);

        if (item != null) {
            item.gameObject.SetActive(true);
            return item;
        }

        return InstantiateItem();
    }

    private Item InstantiateItem() {
        Item item = Instantiate(itemPrefab, transform);
        return item;
    }

    public void Release(Item item) {
        item.gameObject.SetActive(false);
        itemPool.Push(item);
    }
}
