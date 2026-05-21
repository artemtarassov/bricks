using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent OnHold;

    [SerializeField] private float holdIntervalSeconds = 0.2f;

    private Button button;
    private Coroutine holdCoroutine;
    private bool isHolding;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable || !isActiveAndEnabled)
        {
            return;
        }

        isHolding = true;
        holdCoroutine ??= StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
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

            if (!isHolding || !button.interactable)
            {
                continue;
            }

            OnHold?.Invoke();
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
}
