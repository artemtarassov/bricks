using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RestorePurchasesCmd
{
    public void Run()
    {
        IAPModel.Instance.RequestRestore();
    }

}