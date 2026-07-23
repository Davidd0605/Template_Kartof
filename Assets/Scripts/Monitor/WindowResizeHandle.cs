using UnityEngine;
using UnityEngine.EventSystems;

public class WindowResizeHandle : MonoBehaviour, IDragHandler
{
    [Header("Target Window")]
    [SerializeField] private RectTransform targetWindowRect;

    [Header("Which Edge is this?")]
    [SerializeField] private bool resizeRight;
    [SerializeField] private bool resizeBottom;
    [SerializeField] private bool resizeLeft;
    [SerializeField] private bool resizeTop;

    [Header("Constraints")]
    [SerializeField] private float minWidth = 150f;
    [SerializeField] private float minHeight = 100f;

    private Canvas canvas;

    private void Awake()
    {
        if (targetWindowRect == null)
        {
            targetWindowRect = GetComponentInParent<RectTransform>();
        }
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindowRect == null || canvas == null) return;

        Vector2 delta = eventData.delta / canvas.scaleFactor;
        Vector2 sizeDelta = targetWindowRect.sizeDelta;
        Vector2 anchoredPos = targetWindowRect.anchoredPosition;

        if (resizeRight)
        {
            sizeDelta.x += delta.x;
        }
        if (resizeBottom)
        {
            sizeDelta.y -= delta.y;
        }
        if (resizeLeft)
        {
            sizeDelta.x -= delta.x;
            anchoredPos.x += delta.x;
        }
        if (resizeTop)
        {
            sizeDelta.y += delta.y;
        }

        // Apply minimum size limits
        if (sizeDelta.x < minWidth)
        {
            if (resizeLeft) anchoredPos.x -= (minWidth - sizeDelta.x);
            sizeDelta.x = minWidth;
        }
        if (sizeDelta.y < minHeight)
        {
            sizeDelta.y = minHeight;
        }

        // Apply changes
        targetWindowRect.sizeDelta = sizeDelta;
        targetWindowRect.anchoredPosition = anchoredPos;
    }
}