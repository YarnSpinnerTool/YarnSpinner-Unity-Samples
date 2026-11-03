using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image image;
    [SerializeField] GameObject countContainer;
    [SerializeField] TMPro.TMP_Text countText;
    private Action onClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke();
    }

    internal void SetItemInfo(Sprite sprite, int value)
    {
        image.sprite = sprite;
        countContainer.SetActive(value >= 2);
        countText.text = value.ToString(System.Globalization.CultureInfo.CurrentCulture);
    }

    internal void SetOnClicked(System.Action onClicked)
    {
        this.onClicked = onClicked;
    }
}
