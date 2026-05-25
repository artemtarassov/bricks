using DG.Tweening;
using TMPro;
using UnityEngine;

public class ToastController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject contents;


    private Tween hideTween;
    void Start()
    {
        ViewModel.Instance.OnToastMsg += ShowToast;
        contents.SetActive(false);
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnToastMsg -= ShowToast;
    }

    private void ShowToast(string msg)
    {
        if (hideTween != null)
        {
            hideTween.Kill();
        }
        hideTween = DOTween.Sequence(this).AppendInterval(2f)
           .AppendCallback(() => contents.SetActive(false));
        text.text = msg;
        contents.SetActive(true);
    }
}