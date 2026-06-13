using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BlinkingArrow : MonoBehaviour
{
    private List<Image> arrows = new List<Image>();
    private Sequence tween;
    void Awake()
    {
        this.arrows = new List<Image>(this.GetComponentsInChildren<Image>(true));
    }

    void OnEnable()
    {
        foreach (var arrow in arrows)
        {
            arrow.DOKill();
            arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 0);
        }
        if (tween != null)
        {
            tween.Kill();
        }

        tween = DOTween.Sequence();
        for (int i = 0; i < arrows.Count; i++)
        {
            var arrow = arrows[arrows.Count - 1 - i];
            tween = tween.Append(arrow.DOFade(1, 0.33f));
            tween = tween.Append(arrow.DOFade(0, 0.33f));
        }
        tween = tween.SetLoops(-1).SetTarget(this.gameObject).SetLink(this.gameObject);

    }

    void OnDisable()
    {
        foreach (var arrow in arrows)
        {
            arrow.DOKill();
            arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 0);
        }
        if (tween != null)
        {
            tween.Kill();
        }


    }
    void OnDestroy()
    {
        foreach (var arrow in arrows)
        {
            arrow.DOKill();
        }
        if (tween != null)
        {
            tween.Kill();
        }

    }

}