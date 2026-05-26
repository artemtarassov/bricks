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
                ViewModel.Instance.OutOfSpaceFlag = false;
            }
        }

    }


}
