using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum IapResponse
{
    Undefined,
    Success,
    Restore,
    Failed,
}

public class CompleteIapCmd
{
    private string productId;
    private string payload;

    public bool report = true;

    public CompleteIapCmd(string p, string payload = null)
    {
        this.productId = p;
        this.payload = payload;
    }

    public void Run(IapResponse response, bool report = true)
    {

        if (productId == IAPModel.AdditionalSpace)
        {
            if (response == IapResponse.Success || response == IapResponse.Restore)
            {
                PlayerModel.Instance.UnlockAdditionalEmitter();
                SlotModel.Instance.UnlockAdditionalEmitter();
                ViewModel.Instance.ResetOutOfSpaceCounter();
            }
        }

        if (productId == IAPModel.GoldenTicket || productId == IAPModel.GoldenTicketTemp)
        {
            if (response == IapResponse.Success || response == IapResponse.Restore)
            {
                PlayerModel.Instance.UnlockAdditionalEmitter();
                SlotModel.Instance.UnlockAdditionalEmitter();
                ViewModel.Instance.ResetOutOfSpaceCounter();
            }

            if (response == IapResponse.Success)
            {
                var rc = RemoteConfigModel.Instance.RemoteConfig;
                if (productId == IAPModel.GoldenTicketTemp)
                {
                    PlayerModel.Instance.AddDailyRewardCoins(rc.DailyRewardCoinsGoldenTicketTemp);
                }
                if (productId == IAPModel.GoldenTicket)
                {
                    PlayerModel.Instance.AddDailyRewardCoins(rc.DailyRewardCoinsGoldenTicket);
                }
            }
        }

        if (response == IapResponse.Success || response == IapResponse.Restore)
        {
            IAPModel.Instance.SetPurchaseCompleted(productId);
        }

    }


}
