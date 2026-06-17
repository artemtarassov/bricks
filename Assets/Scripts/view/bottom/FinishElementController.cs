using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;

public class FinishElementController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private SlideDown slideDown;
    [SerializeField] private Button nextButton;

    private float currentPercent;
    void Start()
    {
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
        return images;
    }
    private void OnBottomNavChange(BottomNav nav)
    {
        if (nav == BottomNav.FinishElement)
        {
            this.content.SetActive(true);
            this.slideDown.Reset();
            this.currentPercent = 0;
            return;
        }
        this.content.SetActive(false);
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

}