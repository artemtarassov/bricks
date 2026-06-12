using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent OnHold;
    public UnityEvent OnFirstTouch;//tbd.

    public UnityEvent OnClick;

    [SerializeField] private float holdIntervalSeconds = 0.2f;
    [SerializeField] private bool triggerHoldEvent = false;
    [SerializeField] private bool scaleOnClick = true;

    private Button button;
    private Coroutine holdCoroutine;
    private bool isHolding;

    public bool TriggerHoldEvent
    {
        get => triggerHoldEvent;
        set => triggerHoldEvent = value;
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClick?.Invoke());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanProcessInput())
        {
            return;
        }

        isHolding = true;

        if (scaleOnClick)
        {
            transform.DOScale(Vector3.one * 1.15f, 0.05f).SetEase(Ease.OutSine);
        }

        if (!TryInvokeEvent(OnFirstTouch, nameof(OnFirstTouch)) || !CanContinueHolding())
        {
            StopHolding();
            return;
        }

        if (!triggerHoldEvent)
        {
            StopHolding();
            return;
        }

        holdCoroutine ??= StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleOnClick)
        {
            transform.DOScale(Vector3.one, 0.05f).SetEase(Ease.OutSine);
        }
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHolding();
    }

    private void OnDisable()
    {
        StopHolding();
    }

    private IEnumerator HoldRoutine()
    {
        var wait = new WaitForSecondsRealtime(holdIntervalSeconds);

        while (isHolding)
        {
            yield return wait;

            if (!CanContinueHolding())
            {
                StopHolding();
                continue;
            }

            if (!TryInvokeEvent(OnHold, nameof(OnHold)) || !CanContinueHolding())
            {
                StopHolding();
            }
        }

        holdCoroutine = null;
    }

    private void StopHolding()
    {
        isHolding = false;

        if (holdCoroutine == null)
        {
            return;
        }

        StopCoroutine(holdCoroutine);
        holdCoroutine = null;
    }

    private bool CanProcessInput()
    {
        return this != null
               && button != null
               && button.interactable
               && isActiveAndEnabled
               && gameObject.activeInHierarchy;
    }

    private bool CanContinueHolding()
    {
        return isHolding && triggerHoldEvent && CanProcessInput();
    }

    private bool TryInvokeEvent(UnityEvent unityEvent, string eventName)
    {
        if (unityEvent == null)
        {
            return true;
        }

        try
        {
            unityEvent.Invoke();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(HoldButton)}.{eventName} failed.", this);
            Debug.LogException(exception, this);
            return false;
        }
    }
}
