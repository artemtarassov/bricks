using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LogoController : MonoBehaviour
{

    [SerializeField] private GameObject contents;
    [SerializeField] private GameObject palermo;
    [SerializeField] private GameObject veneto;
    [SerializeField] private GameObject tuscany;
    [SerializeField] private GameObject baiae;

    [SerializeField] private AttemptsRow attemptsRow;

    void Start()
    {
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        ViewModel.Instance.OnShowView += OnViewUpdate;
        ViewModel.Instance.OnHideView += OnViewUpdate;
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        this.OnBottomNavChange(BottomNav.MainNav);

        var images = attemptsRow.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            image.color = Color.yellow;
        }
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
            attemptsRow.UpdateValues(PlayerModel.Instance.playerData.attempts);

            palermo.SetActive(false);
            veneto.SetActive(false);
            tuscany.SetActive(false);
            baiae.SetActive(false);
            contents.SetActive(true);


            var currentBuildingName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
            if (currentBuildingName == BuildingName.Ruins1_House)
            {
                palermo.SetActive(true);
            }
            else if (currentBuildingName == BuildingName.Tower_House)
            {
                veneto.SetActive(true);
            }
            else if (currentBuildingName == BuildingName.Preset_House_05)
            {
                tuscany.SetActive(true);
            }
            else if (currentBuildingName == BuildingName.Preset_Bath_House_01)
            {
                baiae.SetActive(true);
            }
        }
        else
        {
            contents.SetActive(false);
        }
    }

}