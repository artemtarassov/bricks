using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputSystemTouchPhase = UnityEngine.InputSystem.TouchPhase;


public class CameraTouchLookaround : MonoBehaviour
{
    [SerializeField] private float yawSensitivity = 0.075f;
    [SerializeField] private float pitchSensitivity = 0.05f;
    [SerializeField] private float maxYawOffset = 18f;
    [SerializeField] private float maxPitchOffset = 12f;
    [SerializeField] private float followFingerDuration = 0.12f;
    [SerializeField] private float returnToSetupDuration = 0.35f;
    [SerializeField] private bool enableEditorTouchSimulation = true;
    [SerializeField] private bool verboseLogging = true;

    private Quaternion setupRotation;
    private Vector3 setupPosition;
    private bool hasExplicitSetup;
    private int activeTouchId = -1;
    private Vector2 touchStartPosition;
    private Quaternion activeHomeRotation;
    private Quaternion lastTweenTargetRotation;
    private Tween rotationTween;
    private static readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    private Transform TargetTransform => Camera.main.transform;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
#if UNITY_EDITOR || UNITY_STANDALONE
        if (enableEditorTouchSimulation)
        {
            TouchSimulation.Enable();
        }
#endif
        Log("EnhancedTouchSupport enabled");
    }

    void OnDisable()
    {
        rotationTween?.Kill();
        activeTouchId = -1;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (enableEditorTouchSimulation)
        {
            TouchSimulation.Disable();
        }
#endif
        Log("EnhancedTouchSupport disabled");
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        setupPosition = TargetTransform.position;
        setupRotation = TargetTransform.rotation;
        lastTweenTargetRotation = setupRotation;
    }

    public void Setup(Vector3 camPos, Vector3 camRot)
    {
        setupPosition = camPos;
        setupRotation = Quaternion.Euler(camRot);
        lastTweenTargetRotation = setupRotation;
        hasExplicitSetup = true;

        if (activeTouchId == -1)
        {
            TweenToRotation(setupRotation, returnToSetupDuration);
        }
    }

    void Update()
    {
        if (activeTouchId == -1 && !hasExplicitSetup)
        {
            setupPosition = TargetTransform.position;
            setupRotation = TargetTransform.rotation;
            lastTweenTargetRotation = setupRotation;
        }

        var activeTouches = EnhancedTouch.activeTouches;
        //Log($"activeTouches.Count = {activeTouches.Count}, activeTouchId = {activeTouchId}");
        
        if (activeTouches.Count == 0)
        {
            if (activeTouchId != -1)
            {
                ReleaseLookaround();
            }
            return;
        }

        if (activeTouchId == -1)
        {
            TryBeginLookaround(activeTouches);
            return;
        }

        if (!TryGetActiveTouch(activeTouches, out var touch))
        {
            ReleaseLookaround();
            return;
        }

        switch (touch.phase)
        {
            case InputSystemTouchPhase.Moved:
            case InputSystemTouchPhase.Stationary:
                UpdateLookaround(touch);
                break;
            case InputSystemTouchPhase.Ended:
            case InputSystemTouchPhase.Canceled:
                ReleaseLookaround();
                break;
        }
    }

    void OnDestroy()
    {
        rotationTween?.Kill();
    }

    private void TryBeginLookaround(ReadOnlyArray<EnhancedTouch> activeTouches)
    {
        for (var i = 0; i < activeTouches.Count; i++)
        {
            var touch = activeTouches[i];
            if (touch.phase != InputSystemTouchPhase.Began)
            {
                continue;
            }

            if (IsScreenPositionOverUI(touch.screenPosition))
            {
                Log("Ignoring lookaround touch because it started over UI");
                continue;
            }

            rotationTween?.Kill();
            activeTouchId = touch.touchId;
            touchStartPosition = touch.screenPosition;
            activeHomeRotation = hasExplicitSetup ? setupRotation : TargetTransform.rotation;
            lastTweenTargetRotation = TargetTransform.rotation;
            return;
        }
    }

    private bool TryGetActiveTouch(ReadOnlyArray<EnhancedTouch> activeTouches, out EnhancedTouch activeTouch)
    {
        for (var i = 0; i < activeTouches.Count; i++)
        {
            var touch = activeTouches[i];
            if (touch.touchId == activeTouchId)
            {
                activeTouch = touch;
                return true;
            }
        }

        activeTouch = default;
        return false;
    }

    private void UpdateLookaround(EnhancedTouch touch)
    {
        var dragDelta = touch.screenPosition - touchStartPosition;
        var yaw = Mathf.Clamp(dragDelta.x * yawSensitivity, -maxYawOffset, maxYawOffset);
        var pitch = Mathf.Clamp(-dragDelta.y * pitchSensitivity, -maxPitchOffset, maxPitchOffset);
        var targetRotation = GetLookaroundRotation(activeHomeRotation, pitch, yaw);
        TweenToRotation(targetRotation, followFingerDuration);
    }

    private void ReleaseLookaround()
    {
        activeTouchId = -1;
        TweenToRotation(activeHomeRotation, returnToSetupDuration);
    }

    private void TweenToRotation(Quaternion targetRotation, float duration)
    {
        if (Quaternion.Angle(lastTweenTargetRotation, targetRotation) < 0.01f && rotationTween != null && rotationTween.IsActive())
        {
            return;
        }

        rotationTween?.Kill();
        lastTweenTargetRotation = targetRotation;

        if (duration <= 0f)
        {
            TargetTransform.rotation = targetRotation;
            return;
        }

        var startRotation = TargetTransform.rotation;
        rotationTween = DOTween.To(
            () => 0f,
            progress => TargetTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress),
            1f,
            duration
        )
        .SetEase(Ease.OutSine)
        .SetUpdate(UpdateType.Late)
        .SetTarget(this)
        .OnKill(() => rotationTween = null);
    }

    private void Log(string message)
    {
        if (!verboseLogging)
        {
            return;
        }

        Debug.Log($"CameraTouchLookaround: {message}");
    }

    private bool IsScreenPositionOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private Quaternion GetLookaroundRotation(Quaternion homeRotation, float pitchOffset, float yawOffset)
    {
        var homeEuler = homeRotation.eulerAngles;
        return Quaternion.Euler(homeEuler.x + pitchOffset, homeEuler.y + yawOffset, homeEuler.z);
    }
}
