using DG.Tweening;
using TMPro;
using UnityEngine;

public class ToastController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject contents;
    [SerializeField] private GameObject challengeContainer;
    [SerializeField] private ChallengeIcon challengeIcon;

    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject particlesImage;
    private Tween hideTween;
    void Start()
    {
        ViewModel.Instance.OnToastMsg += ShowToast;
        contents.SetActive(false);
        particlesImage.SetActive(false);
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnToastMsg -= ShowToast;
    }

    private void ShowToast(ToastMsg msg)
    {
        if (hideTween != null)
        {
            hideTween.Kill();
        }
        var g = this.contents.GetComponent<CanvasGroup>();

        hideTween = DOTween.Sequence(this).AppendInterval(5f)
           .AppendCallback(FadeOut);

        this.FadeIn();
        text.text = msg.text;

        if (msg.challenge != BuildingName.Undefined)
        {
            challengeIcon.Setup(msg.challenge);
            challengeContainer.SetActive(true);
            challengeContainer.transform.localScale = Vector3.zero;
            challengeContainer.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            this.particles.gameObject.SetActive(true);
            this.particlesImage.gameObject.SetActive(true);

            DOVirtual.DelayedCall(4, () =>
            {
                challengeContainer.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    challengeContainer.SetActive(false);
                });
            });
        }
        else
        {
            challengeContainer.SetActive(false);
            this.particles.gameObject.SetActive(false);
            this.particlesImage.gameObject.SetActive(false);
        }
    }

    private void FadeOut()
    {
        var g = this.contents.GetComponent<CanvasGroup>();

        g.DOFade(0, Durations.ToastFade).OnComplete(() =>
        {
            contents.SetActive(false);
            particles.gameObject.SetActive(false);
            particlesImage.gameObject.SetActive(false);
            challengeContainer.SetActive(false);
            g.alpha = 1;
        });

    }

    private void FadeIn()
    {
        this.contents.SetActive(true);
        var g = this.contents.GetComponent<CanvasGroup>();
        g.alpha = 0;
        g.DOFade(1, Durations.ToastFade);
    }
}