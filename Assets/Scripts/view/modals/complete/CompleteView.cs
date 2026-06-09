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

    private int CompleteRewardCoins => RemoteConfigModel.Instance.RemoteConfig.CompleteRewardCoins;

    void Start()
    {
        btnCollect.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnCollectClicked);
        btnCollect2x.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnCollect2xClicked);
    }

    private void OnBtnCollectClicked()
    {
        if (this.rewardCollected)
        {
            return;
        }
        this.overlayCollected.SetActive(true);
        this.rewardCollected = true;
        this.AddCoins(CompleteRewardCoins);
    }


    private void OnBtnCollect2xClicked()
    {
        if (this.rewardCollected)
        {
            return;
        }
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

    public override void OnShown()
    {
        AdModel.Instance.OnRewardEarned += OnRewardEarned;
        var currentGroupName = PlayerModel.Instance.playerData.currentGroupName;
        this.rewardCollected = false;
        this.overlayCollected.SetActive(false);
        this.overlayCollected2x.SetActive(false);
        this.particles.SetActive(true);
        this.coinsIcon.gameObject.SetActive(true);
        this.coins.gameObject.SetActive(true);
        this.coins.text = RemoteConfigModel.Instance.RemoteConfig.CompleteRewardCoins.ToString();
        this.title.text = Loca.GetThemeName(currentGroupName);

        var canvasGroup = this.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 5);
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
            this.AddCoins(CompleteRewardCoins * 2);
        }
    }

    private void AddCoins(int n)
    {
        this.coinsIcon.gameObject.SetActive(false);
        this.coins.gameObject.SetActive(false);
        new AddCoinsCmd(n, this.coinsIcon.position).Run();
        DOVirtual.DelayedCall(1.0f, new HideViewCmd(ViewName.CompleteView).Run);
    }




}