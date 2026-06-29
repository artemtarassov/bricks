using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotElement : MonoBehaviour
{
    [SerializeField] public GameObject addMoreBricks;
    [SerializeField] public GameObject coins;
    [SerializeField] public GameObject hiddenBricks;
    [SerializeField] public GameObject explosion;

    [SerializeField] public GameObject ad;

    [SerializeField] public GameObject death;

    [SerializeField] public GameObject seconds;


    [SerializeField] private TMP_Text count;
    [SerializeField] private UIBrick uiBrick;
    [SerializeField] private ChallengeIcon challengeIcon;

    [HideInInspector]
    public SlotElementData slotElementData;

    private List<Color> brickColors;
    private int index;

    public void Setup(int index, SlotElementData data, List<Color> brickColors)
    {
        this.index = index;
        this.brickColors = brickColors;
        this.slotElementData = data;
        if (data == null)
        {
            SetupAsEmpty();
            return;
        }
        if (data.type == SlotElementType.AddSecondsInChallenge)
        {
            SetupWithSecondsInChallenge();
            return;
        }
        if (data.type == SlotElementType.UnlockChallenge)
        {
            SetupWithChallenge();
            return;
        }
        if (data.type == SlotElementType.Bricks)
        {
            SetupWithBricks();
            return;
        }
        if (data.type == SlotElementType.AddMoreBricks)
        {
            SetupWithAddMoreBricks();
            return;
        }
        if (data.type == SlotElementType.Coins)
        {
            SetupWithCoins();
            return;
        }
        if (data.type == SlotElementType.HiddenBricks)
        {
            SetupWithHiddenBricks();
            return;
        }
        if (data.type == SlotElementType.FinalExplosion)
        {
            SetupWithExplosion();
            return;
        }
        if (data.type == SlotElementType.Ad)
        {
            SetupWithAd();
            return;
        }
        if (data.type == SlotElementType.EmitterDeathWaiting)
        {
            SetupWithDeath();
            return;
        }
    }

    public void UpdateIndex(int newIndex)
    {
        if (this.index == newIndex)
        {
            return;
        }
        this.index = newIndex;
        this.uiBrick.ShowGloss(this.index == 0);
    }

    private void SetupWithAd()
    {
        this.SetupAsEmpty();
        this.ad.SetActive(true);
    }

    private void SetupWithExplosion()
    {
        this.SetupAsEmpty();
        this.explosion.SetActive(true);
    }

    private void SetupWithDeath()
    {
        this.SetupAsEmpty();
        this.death.SetActive(true);
        this.count.text = this.slotElementData.deadCounter.ToString();
        this.count.gameObject.SetActive(true);
    }

    private void SetupWithHiddenBricks()
    {
        this.SetupAsEmpty();
        this.ShowAmount();
        this.hiddenBricks.SetActive(true);
    }

    private void ShowColor()
    {
        this.uiBrick.gameObject.SetActive(true);
        var clrIndex = this.slotElementData.BrickData.color;
        this.uiBrick.SetColor(this.brickColors[(int)clrIndex], clrIndex);
        this.uiBrick.ShowGloss(this.index == 0);
    }

    private void ShowAmount()
    {
        this.count.text = this.slotElementData.BrickData.coloredAmount.ToString();
        this.count.gameObject.SetActive(true);
    }

    private void SetupWithCoins()
    {
        this.SetupAsEmpty();
        this.coins.SetActive(true);
        this.count.text = RemoteConfigModel.Instance.RemoteConfig.ColumnCoins.ToString();
        this.count.gameObject.SetActive(true);
    }

    private void SetupWithBricks()
    {
        this.SetupAsEmpty();
        this.ShowColor();
        this.ShowAmount();
    }

    private void SetupWithAddMoreBricks()
    {
        this.SetupAsEmpty();
        this.addMoreBricks.SetActive(true);
    }

    private void SetupWithChallenge()
    {
        this.SetupAsEmpty();
        this.challengeIcon.gameObject.SetActive(true);
        this.challengeIcon.Setup(this.slotElementData.challenge);
    }

    private void SetupWithSecondsInChallenge()
    {
        this.SetupAsEmpty();
        this.seconds.gameObject.SetActive(true);
        this.count.text = this.slotElementData.secondsToAdd.ToString();
        this.count.gameObject.SetActive(true);
    }

    private void SetupAsEmpty()
    {
        this.uiBrick.gameObject.SetActive(false);
        this.addMoreBricks.SetActive(false);
        this.coins.SetActive(false);
        this.hiddenBricks.SetActive(false);
        this.count.gameObject.SetActive(false);
        this.ad.SetActive(false);
        this.explosion.SetActive(false);
        this.death.SetActive(false);
        this.challengeIcon.gameObject.SetActive(false);
        this.seconds.gameObject.SetActive(false);
    }
}