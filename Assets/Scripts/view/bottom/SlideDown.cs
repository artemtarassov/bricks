using System;
using UnityEngine;
using UnityEngine.UI;

public class SlideDown : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float decelerationRate = 0.135f;

    private float highestReachedVerticalPosition;
    private float lastNotifiedVerticalPosition;
    private bool suppressScrollCallback;

    public Action OnScroll;

    private void Awake()
    {
        if (this.scrollRect == null)
        {
            this.scrollRect = this.GetComponentInChildren<ScrollRect>();
        }
    }

    public GameObject GetContent()
    {
        return this.scrollRect.content.gameObject;
    }

    private void OnEnable()
    {
        if (this.scrollRect == null)
        {
            return;
        }

        this.scrollRect.horizontal = false;
        this.scrollRect.movementType = ScrollRect.MovementType.Clamped;
        this.scrollRect.inertia = true;
        this.scrollRect.decelerationRate = this.decelerationRate;
        this.scrollRect.onValueChanged.AddListener(this.HandleScrollChanged);

        this.StartCoroutine(this.InitializeAtBottom());
    }

    private void OnDisable()
    {
        if (this.scrollRect != null)
        {
            this.scrollRect.onValueChanged.RemoveListener(this.HandleScrollChanged);
        }

        this.suppressScrollCallback = false;
    }

    private void LateUpdate()
    {
        if (this.scrollRect == null || this.suppressScrollCallback)
        {
            return;
        }

        float currentVerticalPosition = Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition);
        if (currentVerticalPosition <= this.lastNotifiedVerticalPosition)
        {
            return;
        }

        this.lastNotifiedVerticalPosition = currentVerticalPosition;
        this.OnScroll?.Invoke();
    }

    private System.Collections.IEnumerator InitializeAtBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollRect.viewport);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollRect.content);

        this.suppressScrollCallback = true;
        this.scrollRect.StopMovement();
        this.scrollRect.velocity = Vector2.zero;
        this.scrollRect.verticalNormalizedPosition = 0f;
        this.highestReachedVerticalPosition = 0f;
        this.lastNotifiedVerticalPosition = 0f;
        this.suppressScrollCallback = false;
    }

    private void HandleScrollChanged(Vector2 normalizedPosition)
    {
        if (this.suppressScrollCallback)
        {
            return;
        }


        if (normalizedPosition.y < this.highestReachedVerticalPosition)
        {
            this.suppressScrollCallback = true;
            this.scrollRect.StopMovement();
            this.scrollRect.velocity = Vector2.zero;
            this.scrollRect.verticalNormalizedPosition = this.highestReachedVerticalPosition;
            this.suppressScrollCallback = false;
            return;
        }

        this.highestReachedVerticalPosition = normalizedPosition.y;
    }

    public void Reset()
    {
        this.suppressScrollCallback = true;
        this.scrollRect.StopMovement();
        this.scrollRect.velocity = Vector2.zero;
        this.scrollRect.verticalNormalizedPosition = 0f;
        this.highestReachedVerticalPosition = 0f;
        this.lastNotifiedVerticalPosition = 0f;
        this.suppressScrollCallback = false;
    }

    public float GetPercentScrolled()
    {
        if (this.scrollRect == null)
        {
            return 0f;
        }

        return Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition);
    }
}
