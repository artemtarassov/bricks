using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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
    private RectTransform canvasRectTransform;
    private Canvas parentCanvas;
    private Canvas overlayCanvas;
    private RectTransform overlayCanvasRectTransform;
    private Texture2D transparentCursorTexture;
    private Transform originalFingerParent;
    private int originalFingerSiblingIndex;
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
        fingerParentRectTransform = fingerRectTransform != null ? fingerRectTransform.parent as RectTransform : null;
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>() : null;

        if (fingerRectTransform == null || fingerParentRectTransform == null || parentCanvas == null || canvasRectTransform == null)
        {
            Debug.LogWarning($"{nameof(ShowMouseController)} requires FingerImg to be a UI element under a Canvas.", this);
            enabled = false;
            return;
        }

        defaultScale = fingerRectTransform.localScale;
        originalFingerParent = fingerRectTransform.parent;
        originalFingerSiblingIndex = fingerRectTransform.GetSiblingIndex();
        transparentCursorTexture = CreateTransparentCursorTexture();
        EnsureOverlayCanvas();
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

        EnforceHiddenCursor();

        AttachFingerToOverlayCanvas();
        fingerImg.SetActive(true);
        fingerRectTransform.SetAsLastSibling();
        MoveFingerToCursor();
    }

    private void Update()
    {
        if (fingerRectTransform == null || fingerParentRectTransform == null || canvasRectTransform == null)
        {
            return;
        }

        EnforceHiddenCursor();
        fingerRectTransform.SetAsLastSibling();
        MoveFingerToCursor();

        if (WasPrimaryPointerPressedThisFrame())
        {
            AnimateTap();
        }
    }

    private void LateUpdate()
    {
        if (!isSetupValid)
        {
            return;
        }

        EnforceHiddenCursor();
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

        RestoreFingerParent();

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
            EnforceHiddenCursor();
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

        RestoreFingerParent();

        if (transparentCursorTexture != null)
        {
            Destroy(transparentCursorTexture);
            transparentCursorTexture = null;
        }

        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
            overlayCanvas = null;
            overlayCanvasRectTransform = null;
        }
    }

    private void MoveFingerToCursor()
    {
        if (!TryGetPointerScreenPosition(out var screenPosition))
        {
            return;
        }

        var targetRectTransform = overlayCanvasRectTransform != null
            ? overlayCanvasRectTransform
            : (fingerParentRectTransform != null ? fingerParentRectTransform : canvasRectTransform);
        var eventCamera = overlayCanvasRectTransform != null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, screenPosition, eventCamera, out var localPoint))
        {
            fingerRectTransform.anchoredPosition = localPoint + pointerOffset;
        }
    }

    private bool TryGetPointerScreenPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        var uiPointAction = GetUiPointAction();
        if (uiPointAction != null)
        {
            screenPosition = uiPointAction.ReadValue<Vector2>();
            return true;
        }

        if (Pointer.current != null)
        {
            screenPosition = Pointer.current.position.ReadValue();
            return true;
        }

        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
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

    private bool WasPrimaryPointerPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var uiClickAction = GetUiClickAction();
        if (uiClickAction != null)
        {
            return uiClickAction.WasPressedThisFrame();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return Pen.current != null && Pen.current.tip.wasPressedThisFrame;
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

        var tappedScale = defaultScale * tapScaleMultiplier;
        DOTween.Sequence()
            .SetTarget(fingerRectTransform)
            .SetUpdate(true)
            .Append(fingerRectTransform.DOScale(tappedScale, tapDownDuration).SetEase(Ease.OutQuad))
            .Append(fingerRectTransform.DOScale(defaultScale, tapUpDuration).SetEase(Ease.OutBack));
    }

    private void RestoreCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockMode;
    }

    private void EnforceHiddenCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (transparentCursorTexture != null)
        {
            Cursor.SetCursor(transparentCursorTexture, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    private static Texture2D CreateTransparentCursorTexture()
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixel(0, 0, Color.clear);
        texture.Apply();
        return texture;
    }

    private void EnsureOverlayCanvas()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        var overlayCanvasGameObject = new GameObject("ShowMouseOverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        overlayCanvasGameObject.hideFlags = HideFlags.HideAndDontSave;

        overlayCanvasRectTransform = overlayCanvasGameObject.GetComponent<RectTransform>();
        overlayCanvasRectTransform.anchorMin = Vector2.zero;
        overlayCanvasRectTransform.anchorMax = Vector2.one;
        overlayCanvasRectTransform.offsetMin = Vector2.zero;
        overlayCanvasRectTransform.offsetMax = Vector2.zero;
        overlayCanvasRectTransform.pivot = new Vector2(0.5f, 0.5f);

        overlayCanvas = overlayCanvasGameObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = short.MaxValue;

        var overlayCanvasScaler = overlayCanvasGameObject.GetComponent<CanvasScaler>();
        var sourceCanvasScaler = parentCanvas != null ? parentCanvas.GetComponent<CanvasScaler>() : null;
        if (sourceCanvasScaler != null)
        {
            overlayCanvasScaler.uiScaleMode = sourceCanvasScaler.uiScaleMode;
            overlayCanvasScaler.referenceResolution = sourceCanvasScaler.referenceResolution;
            overlayCanvasScaler.screenMatchMode = sourceCanvasScaler.screenMatchMode;
            overlayCanvasScaler.matchWidthOrHeight = sourceCanvasScaler.matchWidthOrHeight;
            overlayCanvasScaler.referencePixelsPerUnit = sourceCanvasScaler.referencePixelsPerUnit;
            overlayCanvasScaler.scaleFactor = sourceCanvasScaler.scaleFactor;
            overlayCanvasScaler.dynamicPixelsPerUnit = sourceCanvasScaler.dynamicPixelsPerUnit;
            overlayCanvasScaler.physicalUnit = sourceCanvasScaler.physicalUnit;
            overlayCanvasScaler.fallbackScreenDPI = sourceCanvasScaler.fallbackScreenDPI;
            overlayCanvasScaler.defaultSpriteDPI = sourceCanvasScaler.defaultSpriteDPI;
        }
    }

    private void AttachFingerToOverlayCanvas()
    {
        EnsureOverlayCanvas();
        if (overlayCanvasRectTransform == null || fingerRectTransform == null || fingerRectTransform.parent == overlayCanvasRectTransform)
        {
            return;
        }

        fingerRectTransform.SetParent(overlayCanvasRectTransform, true);
        fingerRectTransform.localScale = defaultScale;
        fingerParentRectTransform = overlayCanvasRectTransform;
    }

    private void RestoreFingerParent()
    {
        if (fingerRectTransform == null || originalFingerParent == null || fingerRectTransform.parent == originalFingerParent)
        {
            return;
        }

        fingerRectTransform.SetParent(originalFingerParent, true);
        fingerRectTransform.SetSiblingIndex(originalFingerSiblingIndex);
        fingerRectTransform.localScale = defaultScale;
        fingerParentRectTransform = originalFingerParent as RectTransform;
    }

#if ENABLE_INPUT_SYSTEM
    private static InputAction GetUiPointAction()
    {
        if (EventSystem.current?.currentInputModule is not InputSystemUIInputModule inputModule)
        {
            return null;
        }

        return inputModule.point?.action;
    }

    private static InputAction GetUiClickAction()
    {
        if (EventSystem.current?.currentInputModule is not InputSystemUIInputModule inputModule)
        {
            return null;
        }

        return inputModule.leftClick?.action;
    }
#endif
}
