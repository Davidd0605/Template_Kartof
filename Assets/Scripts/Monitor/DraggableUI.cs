using UnityEngine;
using UnityEngine.EventSystems; 

public class DraggableUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // We need the root canvas to account for scaling issues
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("DraggableUI needs to be on an object inside a Canvas!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Optional: Brings the UI element to the front so it doesn't drag behind other UI
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // We divide the mouse delta by the canvas scale factor.
        // This ensures the element stays exactly under your cursor regardless of your Canvas Scaler settings.
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // You can add logic here for when the player drops the item
        // (e.g., snapping to an inventory slot or playing a sound)
    }
}