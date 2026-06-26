using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;

public class FinishElementSlideDownController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private SlideDown slideDown;

    private Vector3 startPos;

    private float currentPercent;
    void Start()
    {
        this.startPos = this.content.transform.localPosition;
        this.slideDown.OnScroll += OnSlideDownScroll;
        this.content.SetActive(false);
        var br = this.content.AddComponent<BlinkingArrow>();
        br.fadeDuration = 0.15f;
        br.SetArrowImages(GetArrowImages());
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
    }
    private List<Image> GetArrowImages()
    {
        var c = slideDown.GetContent();
        var images = c.GetComponentsInChildren<Image>(true).ToList().FindAll(i => i.gameObject.name == "Arrow");
        images.Reverse();
        return images;
    }
    private void OnBottomNavChange(BottomNav nav)
    {
        UpdateVisibility();
        if (nav == BottomNav.FinishElement)
        {
            this.slideDown.Reset();
            this.currentPercent = 0;
        }
    }
    private void OnSlideDownScroll()
    {
        var percent = slideDown.GetPercentScrolled();
        if (percent - this.currentPercent > 0.08f)
        {
            this.currentPercent = percent;
            var last = percent > 0.9f;
            new NextExplosionStepCmd().Run(last);
        }
    }
    void OnDestroy()
    {
        this.slideDown.OnScroll -= OnSlideDownScroll;
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
    }

    private void UpdateVisibility()
    {
        var nav = ViewModel.Instance.CurrentBottomNav;
        if (nav == BottomNav.FinishElement)
        {
            this.AnimateIn();
        }
        else
        {
            this.content.SetActive(false);
        }
    }

    private void AnimateIn()
    {
        if (this.content.activeSelf)
        {
            return;
        }
        this.content.SetActive(true);
        this.content.transform.localPosition = this.startPos - new Vector3(0, 500, 0);
        this.content.transform.DOLocalMove(this.startPos, Durations.NavTransition).SetEase(Ease.OutSine);
    }


}