using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ChallengeTimecountController : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private GameObject content;


    void Start()
    {
        ChallengeModel.Instance.OnStarted += OnStarted;
        ChallengeModel.Instance.OnStopped += OnStopped;
        OnStarted();
    }
    private void OnStarted()
    {
        this.content.SetActive(true);
        StartSecUpdate();
        OnSecUpdate();
    }

    private void OnStopped()
    {
        this.content.SetActive(false);
        StopSecUpdate();
    }

    private void StartSecUpdate()
    {

        DOTween.Sequence(this)
            .AppendInterval(1.0f)
            .AppendCallback(OnSecUpdate)
            .SetLoops(-1);
    }

    private void StopSecUpdate()
    {
        DOTween.Kill(this);
    }

    private void OnSecUpdate()
    {
        var secLeft = ChallengeModel.Instance.GetSecondsLeft();
        if (secLeft <= 0)
        {
            this.content.SetActive(false);
            this.StopSecUpdate();
            return;
        }
        this.timeText.text = TimeUtils.GetTimeLeft(secLeft, "en");
        if (secLeft <= 10)
        {
            this.timeText.color = Color.red;
            this.StartBlinking();
        }
        else
        {
            this.timeText.color = Color.white;
            this.StopBlinking();
        }
    }

    private void StopBlinking()
    {
        //tbd.
    }

    private void StartBlinking()
    {
        //tbd.
    }
    void OnDestroy()
    {
        ChallengeModel.Instance.OnStarted -= OnStarted;
        ChallengeModel.Instance.OnStopped -= OnStopped;
        this.StopSecUpdate();
    }

}