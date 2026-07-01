using DG.Tweening;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShowMouseController : MonoBehaviour
{
    [SerializeField] private GameObject fingerImg;
    [SerializeField] private Vector2 pointerOffset;
    [SerializeField] private float tapScaleMultiplier = 0.9f;
    [SerializeField] private float tapDownDuration = 0.05f;
    [SerializeField] private float tapUpDuration = 0.08f;

    private RectTransform fingerRectTransform;
    private RectTransform fingerParentRectTransform;
    private Canvas canvas;
    private Vector3 defaultScale;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private bool isReady;
    private bool hasSavedCursorState;

    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
        return;
#endif

        if (fingerImg == null)
        {
            Debug.LogWarning($"{nameof(ShowMouseController)} needs a FingerImg reference.", this);
            enabled = false;
            return;
        }

        fingerRectTransform = fingerImg.GetComponent<RectTransform>();
        fingerParentRectTransform = fingerRectTransform != null ? fingerRectTransform.parent as RectTransform : null;
        canvas = GetComponentInParent<Canvas>();

        if (fingerRectTransform == null || fingerParentRectTransform == null || canvas == null)
        {
            Debug.LogWarning($"{nameof(ShowMouseController)} needs FingerImg under a Canvas.", this);
            enabled = false;
            return;
        }

        defaultScale = fingerRectTransform.localScale;
        isReady = true;
    }

    private void OnEnable()
    {
        if (!isReady)
        {
            return;
        }

        previousCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;
        hasSavedCursorState = true;

        HideCursor();
        fingerImg.SetActive(true);
        fingerRectTransform.SetAsLastSibling();
        MoveFingerToMouse();
    }

    private void Update()
    {
        if (!isReady)
        {
            return;
        }

        HideCursor();
        fingerRectTransform.SetAsLastSibling();
        MoveFingerToMouse();

        if (WasLeftMousePressedThisFrame())
        {
            AnimateTap();
        }
    }

    private void OnDisable()
    {
        if (fingerImg != null)
        {
            fingerImg.SetActive(false);
        }

        if (fingerRectTransform != null)
        {
            DOTween.Kill(fingerRectTransform);
            fingerRectTransform.localScale = defaultScale;
        }

        RestoreCursor();
        hasSavedCursorState = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!isReady)
        {
            return;
        }

        if (hasFocus)
        {
            HideCursor();
        }
        else
        {
            RestoreCursor();
        }
    }

    private void OnDestroy()
    {
        RestoreCursor();
        hasSavedCursorState = false;
    }

    private void MoveFingerToMouse()
    {
        if (!TryGetMouseScreenPosition(out var screenPosition))
        {
            return;
        }

        var eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fingerParentRectTransform, screenPosition, eventCamera, out var localPoint))
        {
            return;
        }

        fingerRectTransform.anchoredPosition = localPoint + pointerOffset;
    }

    private bool TryGetMouseScreenPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        screenPosition = Input.mousePosition;
        return true;
#endif
        screenPosition = default;
        return false;
    }

    private bool WasLeftMousePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    private void AnimateTap()
    {
        DOTween.Kill(fingerRectTransform);
        fingerRectTransform.localScale = defaultScale;

        var pressedScale = defaultScale * tapScaleMultiplier;
        DOTween.Sequence()
            .SetTarget(fingerRectTransform)
            .SetUpdate(true)
            .Append(fingerRectTransform.DOScale(pressedScale, tapDownDuration).SetEase(Ease.OutQuad))
            .Append(fingerRectTransform.DOScale(defaultScale, tapUpDuration).SetEase(Ease.OutBack));
    }

    private static void HideCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void RestoreCursor()
    {
        if (!hasSavedCursorState)
        {
            return;
        }

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockMode;
    }
}
