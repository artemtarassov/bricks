using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class LogoController : MonoBehaviour
{

    [SerializeField] private GameObject contents;
    [SerializeField] private GameObject palermo;
    [SerializeField] private GameObject veneto;
    [SerializeField] private GameObject tuscany;

    void Start()
    {
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        ViewModel.Instance.OnShowView += OnViewUpdate;
        ViewModel.Instance.OnHideView += OnViewUpdate;
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        this.OnBottomNavChange(BottomNav.MainNav);
    }
    void OnDestroy()
    {
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
        ViewModel.Instance.OnShowView -= OnViewUpdate;
        ViewModel.Instance.OnHideView -= OnViewUpdate;
    }

    private void OnViewUpdate(ViewName viewName)
    {
        this.OnBottomNavChange(ViewModel.Instance.CurrentBottomNav);
    }

    private void OnPlayerDataChanged()
    {
        this.OnBottomNavChange(ViewModel.Instance.CurrentBottomNav);
    }

    private void OnBottomNavChange(BottomNav tab)
    {
        var hasView = ViewModel.Instance.HasAnyView();
        if (hasView)
        {
            contents.SetActive(false);
            return;
        }
        if (tab == BottomNav.MainNav)
        {

            palermo.SetActive(false);
            veneto.SetActive(false);
            tuscany.SetActive(false);
            contents.SetActive(true);


            var currentGroup = PlayerModel.Instance.playerData.currentGroupName;
            if (currentGroup == "Ruins1_House")
            {
                palermo.SetActive(true);
            }
            else if (currentGroup == "Tower_House")
            {
                veneto.SetActive(true);
            }
            else if (currentGroup == "Preset_House_05")
            {
                tuscany.SetActive(true);
            }
        }
        else
        {
            contents.SetActive(false);
        }
    }

}