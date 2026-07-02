using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Canvas))]
public class SwipeDetector : MonoBehaviour
{
    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public Action<SwipeDirection> OnSwipeDetected;

    [SerializeField] private float minSwipeDistancePixels = 100f;
    [SerializeField] private float maxSwipeDurationSeconds = 0.75f;
    [SerializeField] private bool limitToCanvasBounds = true;
    [SerializeField] private bool detectMouseInEditor = true;

    private Canvas targetCanvas;
    private RectTransform canvasRectTransform;
    private bool isTrackingPointer;
    private Vector2 swipeStartPosition;
    private float swipeStartTime;
    private static readonly List<RaycastResult> UiRaycastResults = new List<RaycastResult>();

    private void Awake()
    {
        targetCanvas = GetComponent<Canvas>();
        canvasRectTransform = targetCanvas.transform as RectTransform;
    }

    private void Update()
    {
        if (TryHandleTouchInput())
        {
            return;
        }

        TryHandleMouseInput();
    }

    private void OnDisable()
    {
        ResetTracking();
    }

    private void OnDestroy()
    {
        ResetTracking();
    }

    private bool TryHandleTouchInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (TryHandleInputSystemTouch())
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount <= 0)
        {
            return false;
        }

        var touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginTracking(touch.position);
                return true;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTracking(touch.position);
                return true;
            default:
                return isTrackingPointer;
        }
#else
        return false;
#endif
    }

    private bool TryHandleMouseInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!detectMouseInEditor)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            BeginTracking(mouse.position.ReadValue());
            return true;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            EndTracking(mouse.position.ReadValue());
            return true;
        }

        return mouse.leftButton.isPressed && isTrackingPointer;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            BeginTracking(Input.mousePosition);
            return true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndTracking(Input.mousePosition);
            return true;
        }

        return Input.GetMouseButton(0) && isTrackingPointer;
#else
        return false;
#endif
#else
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private bool TryHandleInputSystemTouch()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            return false;
        }

        var touch = touchscreen.primaryTouch;
        var isRelevantTouch = touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame || (touch.press.isPressed && isTrackingPointer);
        if (!isRelevantTouch)
        {
            return false;
        }

        if (touch.press.wasPressedThisFrame)
        {
            BeginTracking(touch.position.ReadValue());
            return true;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            EndTracking(touch.position.ReadValue());
            return true;
        }

        return true;
    }
#endif

    private void BeginTracking(Vector2 screenPosition)
    {
        if (!IsWithinCanvasBounds(screenPosition) || IsPointerOverBlockingUiElement(screenPosition))
        {
            ResetTracking();
            return;
        }

        isTrackingPointer = true;
        swipeStartPosition = screenPosition;
        swipeStartTime = Time.unscaledTime;
    }

    private void EndTracking(Vector2 screenPosition)
    {
        if (!isTrackingPointer)
        {
            return;
        }

        var swipeDelta = screenPosition - swipeStartPosition;
        var swipeDuration = Time.unscaledTime - swipeStartTime;
        ResetTracking();

        if (swipeDuration > maxSwipeDurationSeconds || IsPointerOverBlockingUiElement(screenPosition))
        {
            return;
        }

        if (swipeDelta.sqrMagnitude < minSwipeDistancePixels * minSwipeDistancePixels)
        {
            return;
        }

        var direction = Mathf.Abs(swipeDelta.x) >= Mathf.Abs(swipeDelta.y)
            ? (swipeDelta.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left)
            : (swipeDelta.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down);

        try
        {
            OnSwipeDetected?.Invoke(direction);
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(SwipeDetector)} failed while notifying swipe listeners.", this);
            Debug.LogException(exception, this);
        }
    }

    private bool IsWithinCanvasBounds(Vector2 screenPosition)
    {
        if (!limitToCanvasBounds || canvasRectTransform == null)
        {
            return true;
        }

        var eventCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? targetCanvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(canvasRectTransform, screenPosition, eventCamera);
    }

    private bool IsPointerOverBlockingUiElement(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        UiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, UiRaycastResults);

        for (var i = 0; i < UiRaycastResults.Count; i++)
        {
            if (IsBlockingUiRaycastTarget(UiRaycastResults[i].gameObject))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockingUiRaycastTarget(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return false;
        }

        var current = hitObject.transform;
        while (current != null && current != transform)
        {
            var currentObject = current.gameObject;
            if (ExecuteEvents.CanHandleEvent<IPointerDownHandler>(currentObject)
                || ExecuteEvents.CanHandleEvent<IPointerClickHandler>(currentObject)
                || ExecuteEvents.CanHandleEvent<IBeginDragHandler>(currentObject)
                || ExecuteEvents.CanHandleEvent<IDragHandler>(currentObject)
                || ExecuteEvents.CanHandleEvent<IEndDragHandler>(currentObject)
                || ExecuteEvents.CanHandleEvent<IScrollHandler>(currentObject))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ResetTracking()
    {
        isTrackingPointer = false;
        swipeStartPosition = default;
        swipeStartTime = 0f;
    }
}
