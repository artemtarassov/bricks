using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class ChallengeButton : MonoBehaviour
{
    [SerializeField] private ChallengeIcon challengeIcon;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject checkmark;

    public ChallengeData challengeData { get; private set; }
    public void Setup(ChallengeData cd)
    {
        this.challengeData = cd;
        if (cd.isLocked)
        {
            this.challengeIcon.Setup(cd.buildingName, Color.black);
        }
        else
        {
            this.challengeIcon.Setup(cd.buildingName, Color.white);
        }
        this.checkmark.SetActive(cd.completedCount > 0);
    }

    public void SetSelected(bool s)
    {
        this.selected.SetActive(s);
    }

}