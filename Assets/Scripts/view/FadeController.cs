using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    private Image cover;
    void Start()
    {
        this.cover = this.GetComponentInChildren<Image>(true);
        ViewModel.Instance.OnFade += OnFade;
    }

    private void OnFade(FadeType fadeType)
    {
        cover.DOKill();
        if (fadeType == FadeType.In)
        {
            if (Durations.FadeIn == 0)
            {
                cover.gameObject.SetActive(false);
                return;
            }
            cover.DOFade(0, Durations.FadeIn).OnComplete(() =>
            {
                cover.gameObject.SetActive(false);
            });
            return;
        }
        if (fadeType == FadeType.Out)
        {
            if (Durations.FadeOut == 0)
            {
                cover.gameObject.SetActive(true);
                cover.color = new Color(cover.color.r, cover.color.g, cover.color.b, 1);
                return;
            }
            cover.gameObject.SetActive(true);
            cover.color = new Color(cover.color.r, cover.color.g, cover.color.b, 0);
            cover.DOFade(1, Durations.FadeOut);
            return;
        }

        if (fadeType == FadeType.Flash)
        {
            cover.gameObject.SetActive(true);
            cover.color = new Color(cover.color.r, cover.color.g, cover.color.b, 0);
            var sequence = DOTween.Sequence();
            sequence = sequence.Append(cover.DOFade(1, Durations.FadeOut));
            sequence = sequence.Append(cover.DOFade(0, Durations.FadeIn));
            sequence.OnComplete(() =>
            {
                cover.gameObject.SetActive(false);
            }).SetTarget(cover);
            return;
        }

    }
    void OnDestroy()
    {
        ViewModel.Instance.OnFade -= OnFade;
    }

}