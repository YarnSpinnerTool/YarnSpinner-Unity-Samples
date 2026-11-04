using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#nullable enable

public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image? image;
    [SerializeField] GameObject? countContainer;
    [SerializeField] TMPro.TMP_Text countText;
    private Action? onClicked;

    [SerializeField] Image? selectionHover;
    [SerializeField] Color normalColour = Color.clear;
    [SerializeField] Color hoverColour = Color.white;
    [SerializeField] Color clickColour = Color.red;

    public bool Selectable { get; private set; }

    void Awake()
    {
        if (selectionHover != null) { selectionHover.color = normalColour; }
    }

    internal void SetItemInfo(Sprite sprite, int value, bool selectable)
    {
        if (image != null)
        {
            image.sprite = sprite;
        }

        if (countContainer != null && countText != null)
        {
            countContainer.SetActive(value >= 2);
            countText.text = value.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        this.Selectable = selectable;
    }

    internal void SetOnClicked(System.Action onClicked)
    {
        this.onClicked = onClicked;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (Selectable && selectionHover != null) { selectionHover.color = hoverColour; }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (Selectable && selectionHover != null) { selectionHover.color = normalColour; }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (Selectable && selectionHover != null) { selectionHover.color = clickColour; }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Selectable && selectionHover != null) { selectionHover.color = normalColour; }
    }
}
