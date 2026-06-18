using UnityEngine;
using DG.Tweening;

public class ShowMouseController : MonoBehaviour
{
    [SerializeField] private GameObject fingerImg;
    [SerializeField] private Vector2 pointerOffset;
    [SerializeField] private float tapScaleMultiplier = 0.9f;
    [SerializeField] private float tapDownDuration = 0.05f;
    [SerializeField] private float tapUpDuration = 0.08f;

    private RectTransform fingerRectTransform;
    private RectTransform canvasRectTransform;
    private Canvas parentCanvas;
    private Vector3 defaultScale;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private bool isSetupValid;
    private bool hasCapturedCursorState;

    private void Awake()
    {
        #if !UNITY_EDITOR
        Destroy(this.gameObject);
        return;
        #endif
        if (fingerImg == null)
        {
            Debug.LogWarning($"{nameof(ShowMouseController)} requires a FingerImg reference.", this);
            enabled = false;
            return;
        }

        fingerRectTransform = fingerImg.GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>() : null;

        if (fingerRectTransform == null || parentCanvas == null || canvasRectTransform == null)
        {
            Debug.LogWarning($"{nameof(ShowMouseController)} requires FingerImg to be a UI element under a Canvas.", this);
            enabled = false;
            return;
        }

        defaultScale = fingerRectTransform.localScale;
        isSetupValid = true;
    }

    private void OnEnable()
    {
        if (!isSetupValid)
        {
            return;
        }

        previousCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;
        hasCapturedCursorState = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        fingerImg.SetActive(true);
        MoveFingerToCursor();
    }

    private void Update()
    {
        if (fingerRectTransform == null || canvasRectTransform == null)
        {
            return;
        }

        if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        MoveFingerToCursor();

        if (Input.GetMouseButtonDown(0))
        {
            AnimateTap();
        }
    }

    private void OnDisable()
    {
        if (hasCapturedCursorState)
        {
            RestoreCursor();
            hasCapturedCursorState = false;
        }

        if (fingerImg != null)
        {
            fingerImg.SetActive(false);
        }

        if (fingerRectTransform != null)
        {
            DOTween.Kill(fingerRectTransform);
            fingerRectTransform.localScale = defaultScale;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!enabled)
        {
            return;
        }

        if (hasFocus)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (hasCapturedCursorState)
        {
            RestoreCursor();
        }
    }

    private void OnDestroy()
    {
        if (hasCapturedCursorState)
        {
            RestoreCursor();
            hasCapturedCursorState = false;
        }
    }

    private void MoveFingerToCursor()
    {
        var screenPosition = (Vector2)Input.mousePosition;
        var eventCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, eventCamera, out var localPoint))
        {
            fingerRectTransform.anchoredPosition = localPoint + pointerOffset;
        }
    }

    private void AnimateTap()
    {
        DOTween.Kill(fingerRectTransform);
        fingerRectTransform.localScale = defaultScale;

        var tappedScale = defaultScale * tapScaleMultiplier;
        DOTween.Sequence()
            .SetTarget(fingerRectTransform)
            .SetUpdate(true)
            .Append(fingerRectTransform.DOScale(tappedScale, tapDownDuration).SetEase(Ease.OutQuad))
            .Append(fingerRectTransform.DOScale(defaultScale, tapUpDuration).SetEase(Ease.OutBack));
    }

    private void RestoreCursor()
    {
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockMode;
    }
}
