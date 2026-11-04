using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] InventoryItem itemPrefab;

    public event System.Action<string> OnItemPicked;

    [SerializeField] bool isSelectable;

    public void UpdateDisplay(Dictionary<string, int> inventory, SerializableDictionary<string, Sprite> itemSprites)
    {
        foreach (var item in GetComponentsInChildren<InventoryItem>())
        {
            Destroy(item.gameObject);
        }

        bool anyVisible = false;

        foreach (var itemInfo in inventory.OrderBy(kv => kv.Key))
        {
            // Don't show empty amounts of an item
            if (itemInfo.Value == 0) { continue; }

            if (itemSprites.TryGetValue(itemInfo.Key, out var sprite) == false)
            {
                // No sprite available! We'll show a blank sprite.
                Debug.LogWarning("No sprite available for item type " + itemInfo.Key);
            }

            var newItem = Instantiate(itemPrefab, this.transform);
            newItem.SetItemInfo(sprite, itemInfo.Value, isSelectable);
            newItem.SetOnClicked(() => OnItemClicked(itemInfo.Key));

            anyVisible = true;
        }

        gameObject.SetActive(anyVisible);
    }

    private void OnItemClicked(string key)
    {
        Debug.Log("Inventory item clicked: " + key);
        OnItemPicked?.Invoke(key);
    }
}
