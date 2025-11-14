using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PaytableScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentMain;
    [SerializeField] private RectTransform content;

    [Header("Auto Detect")]
    [SerializeField] private float contentItemWidth;  
    
    [Header("Drag Settings")]
    [SerializeField] private float snapSpeed = 0.35f;
    [SerializeField] private float swipeThreshold = 80f;
    [SerializeField] private float speedThreshold = 0.25f;

    [Header("Offsets")]
    [SerializeField] private float contentHeightOffset = 0f;

    [Header("Indicators")]
    [SerializeField] private Image[] indicator;
    [SerializeField] private Sprite indicatorOn;
    [SerializeField] private Sprite indicatorOff;

    private RectTransform[] items;
    private Vector2 swipeDistance;
    private float dragDuration;
    private int closestItemIndex = 0;


    void Start()
    {
        items = new RectTransform[content.childCount];

        for (int i = 0; i < content.childCount; i++)
        {
            items[i] = content.GetChild(i).GetComponent<RectTransform>();
        }

        AutoCalculatePageWidth();

        if (indicator.Length > 0)
            indicator[0].sprite = indicatorOn;
    }


    void AutoCalculatePageWidth()
    {
        if (items.Length > 1)
        {
            float posA = items[0].anchoredPosition.x;
            float posB = items[1].anchoredPosition.x;
            contentItemWidth = Mathf.Abs(posB - posA);
        }
        else
        {
            contentItemWidth = items[0].rect.width;
        }

        Debug.Log("Auto Detected Page Width: " + contentItemWidth);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        swipeDistance = eventData.position;
        dragDuration = Time.time;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        dragDuration = Mathf.Abs(Time.time - dragDuration);
        swipeDistance = eventData.position - swipeDistance;
        SnapToClosestItem();
    }

    private void SnapToClosestItem()
    {
        scrollRect.velocity = Vector2.zero;
        RectTransform targetItem = null;

        if (swipeDistance.magnitude > swipeThreshold && dragDuration < speedThreshold)
        {  
            if (swipeDistance.x < 0)   
                closestItemIndex++;
            else                         
                closestItemIndex--;

            closestItemIndex = Mathf.Clamp(closestItemIndex, 0, items.Length - 1);
            targetItem = items[closestItemIndex];
        }
        else
        {
            float closestDist = Mathf.Infinity;

            for (int i = 0; i < items.Length; i++)
            {
                float dist = Mathf.Abs(items[i].position.x - contentMain.position.x);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetItem = items[i];
                    closestItemIndex = i;
                }
            }
        }

        if (targetItem == null)
            return;

        float targetX = -(contentItemWidth * closestItemIndex);
        Vector2 finalPos = new Vector2(targetX, contentHeightOffset);

        content.DOAnchorPos(finalPos, snapSpeed).SetEase(Ease.OutCubic);

        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        if (indicator.Length == 0) return;

        for (int i = 0; i < indicator.Length; i++)
            indicator[i].sprite = indicatorOff;

        indicator[closestItemIndex].sprite = indicatorOn;
    }
}
