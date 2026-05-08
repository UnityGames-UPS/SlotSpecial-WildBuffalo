using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI; // For DOTween

public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform buttonTransform;
    private Button button;
    private RectTransform rectTransform;
    private float originalScale;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (buttonTransform == null)
            buttonTransform = transform;
        
        if (button == null)
            button = GetComponent<Button>();
            
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (originalScale == 0)
            originalScale = buttonTransform.localScale.x;
    }

    // Called when the button is pressed.
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            PressedAnimation();
        }
    }

    // Called when the button is released.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            // Check if the pointer is within the original bounds (ignoring the current shrunk scale)
            Vector3 currentScale = buttonTransform.localScale;
            buttonTransform.localScale = new Vector3(originalScale, originalScale, currentScale.z);
            bool insideOriginal = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera);
            
            // Check if it's currently inside the shrunk bounds
            buttonTransform.localScale = currentScale;
            bool insideNow = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera);

            // If it's inside the original area but NOT the shrunk area, force the click
            // because the standard Button component will fail to fire its OnClick event.
            if (insideOriginal && !insideNow)
            {
                button.onClick.Invoke();
            }
        }
        
        OnClickedAnimation();
    }

    void PressedAnimation()
    {
        buttonTransform.DOKill();
        buttonTransform.DOScale(originalScale * 0.8f, 0.2f); // Scale down on press.
    }

    void OnClickedAnimation()
    {
        buttonTransform.DOKill();
        buttonTransform.DOScale(originalScale, 0.2f); // Scale back to normal size after release.
    }
}
