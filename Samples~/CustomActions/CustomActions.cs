using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Attributes;
using Yarn.Unity.Samples;

#nullable enable

public class CustomFunctions : MonoBehaviour
{
    [SerializeField] InventoryPanel? displayPanel;
    [SerializeField] InventoryPanel? selectionPanel;

    [SerializeField] SerializableDictionary<string, Sprite> itemSprites = new();

    [SerializeField] AudioSource? audioSource;

    private readonly Dictionary<string, int> inventory = new();

    bool isSelecting = false;

    public static CustomFunctions Instance
    {
        get
        {
            return FindAnyObjectByType<CustomFunctions>() switch
            {
                CustomFunctions instance => instance,
                _ => throw new System.InvalidOperationException($"No {nameof(CustomFunctions)} present in the scene!")
            };
        }
    }

    void Awake()
    {
        UpdateInventoryPanels();
    }

    private void UpdateInventoryPanels()
    {
        Debug.Log("Inventory: " + string.Join(", ", inventory.Select(kv => $"{kv.Key}: {kv.Value}")));
        if (displayPanel != null)
        {
            displayPanel.UpdateDisplay(inventory, itemSprites);
            if (isSelecting)
            {
                displayPanel.gameObject.SetActive(false);
            }
        }

        if (selectionPanel != null)
        {
            selectionPanel.UpdateDisplay(inventory, itemSprites);
            if (!isSelecting)
            {
                selectionPanel.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="name">The type of the item.</param>
    /// <param name="count">How many to add.</param>
    [YarnCommand("add_inventory_item")]
    public static void AddInventoryItem(string name, int count = 1)
    {

        if (Instance.inventory.TryGetValue(name, out var currentCount) == false)
        {
            currentCount = 0;
        }

        Instance.inventory[name] = Mathf.Max(currentCount + count, 0);
        Instance.UpdateInventoryPanels();
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="name">The type of the item.</param>
    /// <param name="count">How many to remove.</param>
    [YarnCommand("remove_inventory_item")]
    public static void RemoveInventoryItem(string name, int count = 1)
    {
        if (Instance.inventory.TryGetValue(name, out var currentCount) == false)
        {
            currentCount = 0;
        }

        Instance.inventory[name] = Mathf.Max(currentCount - count, 0);
        Instance.UpdateInventoryPanels();
    }

    /// <summary>
    /// Checks to see if the inventory contains any items of the given type.
    /// </summary>
    /// <param name="itemName">The name of the item to check for.</param>
    /// <returns>True if the inventory has any of this item; false otherwise.</returns>
    [YarnFunction("has_inventory_item")]
    public static bool HasInventoryItem([YarnEnumParameter("FoodType")] string itemName)
    {
        return Instance.inventory.TryGetValue(itemName, out var count) && count > 0;
    }

    /// <summary>
    /// Checks to see if the inventory contains any items at all.
    /// </summary>
    /// <returns>True if the inventory contains anything.</returns>
    [YarnFunction("has_any_inventory_items")]
    public static bool HasAnyInventoryItems()
    {
        return Instance.inventory.Any(kv => kv.Value > 0);
    }

    /// <summary>
    /// Returns the number of items of a given type present in the inventory.
    /// </summary>
    /// <param name="itemName">The type of the item to check for.</param>
    /// <returns>The number of items of that type.</returns>
    [YarnFunction("inventory_count")]
    public static int GetInventoryCount(string itemName)
    {
        return Instance.inventory.TryGetValue(itemName, out var count)
            ? count
            : 0;
    }

    /// <summary>
    /// Shows an on-screen UI element for picking a UI element.
    /// </summary>
    /// <param name="cancellationToken">A token to signal that picking should be cancelled.</param>
    /// <returns>The type of the item that the user picked, or "none" if picking was cancelled.</returns>
    [YarnFunction("pick_inventory_item")]
    public static async YarnTask<string> PickInventoryItem(CancellationToken cancellationToken)
    {
        const string DefaultValue = "none";

        // If we don't have a panel to select from, or we don't have any items we could select, or we're already cancelled, return our default value
        if (Instance.selectionPanel == null
            || cancellationToken.IsCancellationRequested
            || !Instance.inventory.Any(kv => kv.Value > 0))
        {
            return DefaultValue;
        }

        // Create a completion source, which we use to signal that we have a result (and also to get a task we can await)
        var completionSource = new YarnTaskCompletionSource<string>();

        // Also, register that we want to know when we've been cancelled. When we do, signal that we've selected the default value.
        var registration = cancellationToken.Register(() => completionSource.TrySetResult(DefaultValue));

        // A small local function that signals that we're done. (We'll store it in a local method)
        System.Action<string> ItemPicked = (string item) => completionSource.TrySetResult(item);

        // When the selection panel makes a selection, run this ItemPicked handler.
        Instance.selectionPanel.OnItemPicked += ItemPicked;
        try
        {
            // Show the right inventory panel
            Instance.isSelecting = true;
            Instance.UpdateInventoryPanels();

            // Wait until 
            var result = await completionSource.Task;
            return result;
        }
        finally
        {
            // This runs after we either successfully select an item, or some exception gets thrown.

            // Clean up by ending selection mode, removing the on clicked handler, and disposing of our 'on cancelled' handler.
            Instance.isSelecting = false;
            Instance.UpdateInventoryPanels();

            Instance.selectionPanel.OnItemPicked -= ItemPicked;
            registration.Dispose();
        }
    }

    /// <summary>
    /// Walks a character to a named location in the scene, plays an eating sound effect, and walks them back.
    /// </summary>
    /// <param name="target">The target game object. The target must have a SimpleCharacter component.</param>
    /// <param name="destination">The location to walk to.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the entire action.</param>
    /// <returns>A task that completes when the action is over.</returns>
    [YarnCommand("walk_to_point_eat_and_return")]
    public static async YarnTask MoveToPointAndReturn(GameObject target, Transform destination, CancellationToken cancellationToken)
    {
        if (target.TryGetComponent<SimpleCharacter>(out var simpleCharacter) == false)
        {
            Debug.LogWarning($"{target.name} doesn't have a {nameof(SimpleCharacter)}");
            return;
        }

        simpleCharacter.transform.GetPositionAndRotation(out var originalPosition, out var originalLookDirection);

        // Walk to the target, pause a moment, and walk back
        await simpleCharacter.MoveTo(destination.position, cancellationToken);

        AudioSource? audioSource = Instance.audioSource;
        if (audioSource != null && audioSource.resource != null)
        {
            audioSource.Play();
        }

        await YarnTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken).SuppressCancellationThrow();
        await simpleCharacter.MoveTo(originalPosition, cancellationToken);

        // Restore our original look direction
        simpleCharacter.SetLookDirection(originalLookDirection);

        // Stop the audio in case it's still going and we got cancelled
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}