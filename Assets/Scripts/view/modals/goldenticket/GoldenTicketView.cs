using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class GoldenTicketView : DefaultView
{
    void Start()
    {
        var iapButtons = this.GetComponentsInChildren<BtnIAP>();
        foreach (var btn in iapButtons)
        {
            btn.onClicked += OnIapClicked;
        }
    }

    public override void OnHidden()
    {
    }

    public override void OnShown()
    {
    }


    private void OnIapClicked(IAPProductName productName)
    {
        new RequestPurchaseCmd(productName).Run();
    }
    void OnDestroy()
    {
    }

}