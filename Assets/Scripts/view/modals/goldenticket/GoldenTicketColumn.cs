using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class GoldenTicketColumn : MonoBehaviour
{
    [SerializeField] private GameObject coinsIcon;
    [SerializeField] private GameObject spaceIcon;

    void OnEnable()
    {
        var iapModel = IAPModel.Instance;
        if (iapModel.DidPurchaseComplete(IAPModel.AdditionalSpace))
        {
            coinsIcon.SetActive(true);
            spaceIcon.SetActive(false);
        }
        else
        {
            coinsIcon.SetActive(false);
            spaceIcon.SetActive(true);
        }
    }


}