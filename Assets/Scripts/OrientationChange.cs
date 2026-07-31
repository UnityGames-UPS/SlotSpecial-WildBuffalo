using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class OrientationChange : MonoBehaviour
{
  [SerializeField] private RectTransform UIWrapper;
  [SerializeField] private CanvasScaler CanvasScaler;
  [SerializeField] private float transitionDuration = 0.5f;
  [SerializeField] private float waitForRotation = 1f;
  [SerializeField] private float tabletScale = 0.9f;   // scale for tablets

  private Vector2 ReferenceAspect;
  private Tween matchTween;
  private Tween rotationTween;
  private Tween scaleTween;
  private Coroutine rotationRoutine;
  private bool isLandscape;

  private void Awake()
  {
    ReferenceAspect = CanvasScaler.referenceResolution;
  }

  private void Start()
  {
    ApplyMatch(Screen.width, Screen.height);
  }

  void SwitchDisplay(string dimensions)
  {
    if (rotationRoutine != null) StopCoroutine(rotationRoutine);
    rotationRoutine = StartCoroutine(RotationCoroutine(dimensions));
  }

  IEnumerator RotationCoroutine(string dimensions)
  {
    yield return new WaitForSecondsRealtime(waitForRotation);

    string[] parts = dimensions.Split(',');
    if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height) && width > 0 && height > 0)
    {
      ApplyMatch(width, height);
    }
    else
    {
      Debug.LogWarning("Unity: Invalid format received in SwitchDisplay");
    }
  }

  private void ApplyMatch(int width, int height)
  {
    Debug.Log($"Unity: Applying match - Width: {width}, Height: {height}");

    isLandscape = width > height;

    // 🎯 ROTATION
    Quaternion targetRotation = isLandscape ? Quaternion.identity : Quaternion.Euler(0, 0, -90);
    if (rotationTween != null && rotationTween.IsActive()) rotationTween.Kill();
    rotationTween = UIWrapper.DOLocalRotateQuaternion(targetRotation, transitionDuration).SetEase(Ease.OutCubic);

    // 🎯 CONTINUOUS MATCH WIDTH/HEIGHT (log-interpolated — covers every aspect ratio,
    // not just a hand-tuned bucket)
    float refW = ReferenceAspect.x;
    float refH = ReferenceAspect.y;

    float widthScale = (float)width / refW;
    float heightScale = (float)height / refH;

    float targetScaleForMatch;
    if (isLandscape)
    {
      targetScaleForMatch = Mathf.Min(widthScale, heightScale);
    }
    else
    {
      float portraitWidthScale = (float)height / refW;
      float portraitHeightScale = (float)width / refH;
      targetScaleForMatch = Mathf.Min(portraitWidthScale, portraitHeightScale);
    }

    float targetMatch;
    if (Mathf.Abs(heightScale - widthScale) < 0.0001f)
    {
      targetMatch = 0.5f;
    }
    else
    {
      float logRatio = Mathf.Log(heightScale / widthScale);
      targetMatch = Mathf.Log(targetScaleForMatch / widthScale) / logRatio;
      targetMatch = Mathf.Clamp01(targetMatch);
    }

    if (matchTween != null && matchTween.IsActive()) matchTween.Kill();
    matchTween = DOTween.To(() => CanvasScaler.matchWidthOrHeight, x => CanvasScaler.matchWidthOrHeight = x, targetMatch, transitionDuration).SetEase(Ease.InOutQuad);

    // 🛡 TABLET / IPAD SAFE SCALE
    float aspect = (float)Screen.height / Screen.width;
    aspect = Mathf.Max(aspect, 1f / aspect);  // Normalize (always >= 1)

    // Tablet if aspect ratio is around 4:3 or slightly wider (1.3 to 1.6)
    bool isTablet = aspect >= 1.3f && aspect <= 1.6f;

    float targetScale = isTablet ? tabletScale : 1f;

    if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
    scaleTween = UIWrapper.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);

    Debug.Log($"matchWidthOrHeight set to: {targetMatch}, UI Scale: {targetScale}");
  }

#if UNITY_EDITOR
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      SwitchDisplay(Screen.width + "," + Screen.height);
    }
  }
#endif
}
