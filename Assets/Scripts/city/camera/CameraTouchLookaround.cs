using UnityEngine;
using DG.Tweening;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputSystemTouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

public class CameraTouchLookaround : MonoBehaviour
{
    [SerializeField] private float yawSensitivity = 0.075f;
    [SerializeField] private float pitchSensitivity = 0.05f;
    [SerializeField] private float maxYawOffset = 18f;
    [SerializeField] private float maxPitchOffset = 12f;
    [SerializeField] private float followFingerDuration = 0.12f;
    [SerializeField] private float returnToSetupDuration = 0.35f;

    private Quaternion setupRotation;
    private Vector3 setupPosition;
    private bool hasExplicitSetup;
    private int activeTouchId = -1;
    private Vector2 touchStartPosition;
    private Quaternion activeHomeRotation;
    private Quaternion lastTweenTargetRotation;
    private Tween rotationTween;

    private Transform TargetTransform => transform;

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Enable();
#endif
    }

    void OnDisable()
    {
        rotationTween?.Kill();
        activeTouchId = -1;
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Disable();
#endif
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
        if (!Application.isMobilePlatform)
        {
            return;
        }

#if !ENABLE_INPUT_SYSTEM
        return;
#else
        if (activeTouchId == -1 && !hasExplicitSetup)
        {
            setupPosition = TargetTransform.position;
            setupRotation = TargetTransform.rotation;
            lastTweenTargetRotation = setupRotation;
        }

        var activeTouches = EnhancedTouch.activeTouches;
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
#endif
    }

    void OnDestroy()
    {
        rotationTween?.Kill();
    }

#if ENABLE_INPUT_SYSTEM
    private void TryBeginLookaround(ReadOnlyArray<EnhancedTouch> activeTouches)
    {
        for (var i = 0; i < activeTouches.Count; i++)
        {
            var touch = activeTouches[i];
            if (touch.phase != InputSystemTouchPhase.Began)
            {
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
        var targetRotation = activeHomeRotation * Quaternion.Euler(pitch, yaw, 0f);
        TweenToRotation(targetRotation, followFingerDuration);
    }
#endif

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
}
