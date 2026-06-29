using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class CompleteView : DefaultView
{
    [SerializeField] private GameObject particles;

    [SerializeField] private TMP_Text coins;

    [SerializeField] private TMP_Text title;

    [SerializeField] private Button btnCollect;

    [SerializeField] private Button btnCollect2x;

    [SerializeField] private Transform coinsIcon;

    [SerializeField] private GameObject overlayCollected;
    [SerializeField] private GameObject overlayCollected2x;
    private bool rewardCollected;

    private int GetCompleteRewardCoins()
    {
        if (BuildingNameUtil.IsChallengeBuilding(currentBuildingName))
        {
            return RemoteConfigModel.Instance.RemoteConfig.CompleteChallengeRewardCoins;
        }
        else
        {
            return RemoteConfigModel.Instance.RemoteConfig.CompleteRewardCoins;
        }
    }

    void Start()
    {
        btnCollect.GetComponent<HoldButton>().OnClick.AddListener(OnBtnCollectClicked);
        btnCollect2x.GetComponent<HoldButton>().OnClick.AddListener(OnBtnCollect2xClicked);
    }

    private void OnBtnCollectClicked()
    {
        if (this.rewardCollected)
        {
            return;
        }
        this.overlayCollected.SetActive(true);
        this.rewardCollected = true;
        this.AddCoins(GetCompleteRewardCoins());
    }


    private void OnBtnCollect2xClicked()
    {
        if (this.rewardCollected)
        {
            Debug.Log("CompleteView: OnBtnCollect2xClicked: reward already collected, ignoring click");
            return;
        }
        Debug.Log("CompleteView: OnBtnCollect2xClicked: showing ad for double coins");
        new ShowAdCmd().Run(RewardName.DOUBLE_COINS);
    }
    void OnEnable()
    {
        this.particles.SetActive(true);
    }
    void OnDisable()
    {
        this.particles.SetActive(false);
    }
    void OnDestroy()
    {
        AdModel.Instance.OnRewardEarned -= OnRewardEarned;
    }

    public override void OnHidden()
    {
        this.particles.SetActive(false);
        AdModel.Instance.OnRewardEarned -= OnRewardEarned;
    }

    private BuildingName currentBuildingName => PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;

    public override void OnShown()
    {
        AdModel.Instance.OnRewardEarned += OnRewardEarned;
        this.rewardCollected = false;
        this.overlayCollected.SetActive(false);
        this.overlayCollected2x.SetActive(false);
        this.particles.SetActive(true);
        this.coinsIcon.gameObject.SetActive(true);
        this.coins.gameObject.SetActive(true);
        this.coins.text = "Bonus: " + GetCompleteRewardCoins() + "";
        this.title.text = Loca.GetBuildingNameTranslation(currentBuildingName);
        var canvasGroup = this.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, Durations.ViewFadeIn);

        SoundModel.Instance.Stop(SoundModel.Instance.MUSIC1);
        new SoundCmd(SoundModel.Instance.MAGIC_LIGHT).Run();
    }

    private void OnRewardEarned(AdRewardData rewardData)
    {
        if (this.rewardCollected)
        {
            return;
        }
        if (rewardData.rewardName == RewardName.DOUBLE_COINS)
        {
            this.overlayCollected2x.SetActive(true);
            this.rewardCollected = true;
            this.AddCoins(GetCompleteRewardCoins() * 2);
        }
    }

    private void AddCoins(int n)
    {
        this.coinsIcon.gameObject.SetActive(false);
        this.coins.gameObject.SetActive(false);
        new AddCoinsCmd(n, this.coinsIcon.position).Run();

        var canvasGroup = this.GetComponent<CanvasGroup>();
        canvasGroup.DOFade(0, Durations.ViewFadeOut).OnComplete(new HideViewCmd(ViewName.CompleteView).Run);
    }




}