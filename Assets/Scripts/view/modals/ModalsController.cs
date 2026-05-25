using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ModalsController : MonoBehaviour
{
    [SerializeField] private GameObject movingBackgroundView;
    private Vector3 movingBackgroundInitialPos;
    void Awake()
    {
        this.movingBackgroundInitialPos = this.movingBackgroundView.transform.localPosition;
        this.movingBackgroundView.SetActive(false);
        this.movingBackgroundView.GetComponent<Button>().onClick.AddListener(OnBlockingBackgroundClicked);
        var allViews = this.GetComponentsInChildren<DefaultView>(true);
        foreach (var view in allViews)
        {
            view.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        ViewModel.Instance.OnShowView += OnShowView;
        ViewModel.Instance.OnHideView += OnHideView;
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnShowView -= OnShowView;
        ViewModel.Instance.OnHideView -= OnHideView;
    }

    private void OnBlockingBackgroundClicked()
    {
        var allViews = this.GetComponentsInChildren<DefaultView>(true);
        var active = allViews.FirstOrDefault(v => v.gameObject.activeSelf);
        if (active != null)
        {
            active.OnBackgroundTap();
        }
    }

    private void OnHideView(ViewName viewName)
    {
        var t = Durations.ModalViewTransition;
        var allViews = this.GetComponentsInChildren<DefaultView>(true);
        foreach (var view in allViews)
        {
            if (view.gameObject.name == viewName.ToString())
            {
                var wasActive = view.gameObject.activeSelf;

                if (wasActive)
                {
                    view.transform.DOKill();
                    view.gameObject.SetActive(false);
                    view.OnHidden();
                }
            }
        }

        {//animate bg
            var bgIsActive = this.movingBackgroundView.gameObject.activeSelf;
            var willBeActive = HasActiveViews();
            if (bgIsActive && !willBeActive)
            {
                this.movingBackgroundView.transform.DOKill();
                this.movingBackgroundView.SetActive(false);
            }
        }
    }
    private bool HasActiveViews()
    {
        var allViews = this.GetComponentsInChildren<DefaultView>(true);
        return allViews.Any(v => v.gameObject.activeSelf);
    }
    private void OnShowView(ViewName viewName, bool animate)
    {
        var allViews = this.GetComponentsInChildren<DefaultView>(true);
        foreach (var view in allViews)
        {
            var wasActive = view.gameObject.activeSelf;
            view.gameObject.SetActive(view.gameObject.name == viewName.ToString());
            if (view.gameObject.activeSelf && !wasActive)
            {
                view.OnShown(animate);
                //view.transform.localScale = Vector3.zero;
                var t = Durations.ModalViewTransition;
                view.transform.DOKill();
                //view.transform.DOScale(1, t).SetEase(Ease.OutBack);
                view.transform.localPosition = new Vector3(0, 50, 0);
                view.transform.DOLocalMoveY(0, t).SetEase(Ease.OutBack);
            }
            else
                if (!view.gameObject.activeSelf && wasActive)
                {
                    view.OnHidden();
                }
        }

        {   //animate bg
            var bgIsActive = this.movingBackgroundView.gameObject.activeSelf;
            var willBeActive = HasActiveViews();
            if (!bgIsActive && willBeActive)
            {
                this.movingBackgroundView.SetActive(true);
                var t = Durations.ModalViewTransition;
                this.movingBackgroundView.transform.DOKill();
                this.movingBackgroundView.transform.localPosition = new Vector3(this.movingBackgroundInitialPos.x, this.movingBackgroundInitialPos.y - 200, this.movingBackgroundInitialPos.z);
                this.movingBackgroundView.transform.DOLocalMoveY(this.movingBackgroundInitialPos.y, t).SetEase(Ease.OutBack);
            }
        }
    }




}