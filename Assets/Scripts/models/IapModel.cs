using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PriceData
{
    public string priceString;
    public string isoCurrencyCode;
    public float price;
}

public class IAPModel
{
    public static IAPModel Instance;

    public readonly List<string> productIds = new List<string>();
    private Dictionary<string, PriceData> prices = new Dictionary<string, PriceData>();

    public const string GoldenTicket = "com.badmonkee.stones.goldenticket";
    public const string AdditionalSpace = "com.badmonkee.stones.morespace";

    public const string CashPack1 = "com.badmonkee.stones.smallcashpack";//Small cash pack: Receive a small cash pack immediately
    public const string CashPack2 = "com.badmonkee.stones.mediumcashpack";//Medium cash pack: Receive a medium cash pack immediately
    public const string CashPack3 = "com.badmonkee.stones.largecashpack";//Large cash pack: Receive a large cash pack immediately

    public static bool IsCashPack(string productId)
    {
        return productId == CashPack1 || productId == CashPack2 || productId == CashPack3;
    }

    public Action<string> OnPurchaseSuccess;
    public Action<string> OnPurchaseFailed;
    public Action<string> OnPurchaseRequest;
    public Action OnRestorePurchases;

    public Action<string> OnPricesSet;

    public IAPModel()
    {
        productIds.Add(GoldenTicket);
        productIds.Add(CashPack1);
        productIds.Add(CashPack2);
        productIds.Add(CashPack3);
        productIds.Add(AdditionalSpace);
        var pricesJson = FilePrefs.GetString("IAPModel.prices", "{}");
        prices = JsonUtility.FromJson<Dictionary<string, PriceData>>(pricesJson);
    }

    public void RequestRestore()
    {
        //Debug.Log("IAPModel.RequestRestore");
        OnRestorePurchases?.Invoke();
    }

    public void RequestPurchase(string productId)
    {
        //Debug.Log("IAPModel.RequestPurchase: " + productId);
        OnPurchaseRequest?.Invoke(productId);
    }

    public void SetPurchaseCompleted(string productUd)
    {
        //Debug.Log("IAPModel.SetPurchaseCompleted: " + productUd);
        OnPurchaseSuccess?.Invoke(productUd);
    }

    public void SetPurchaseFailed(string productUd)
    {
        //Debug.Log("IAPModel.SetPurchaseFailed: " + productUd);
        OnPurchaseFailed?.Invoke(productUd);
    }

    public string GetPriceForProduct(string productId)
    {
        if (HasPriceForProduct(productId))
        {
            return prices[productId].priceString;
        }
        return "?";
    }

    public PriceData GetPriceDataForProduct(string productId)
    {
        if (!HasPriceForProduct(productId))
        {
            return null;
        }
        return prices[productId];
    }

    public bool HasPriceForProduct(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return false;
        }
        return prices.ContainsKey(productId);
    }

    public void SetPriceForProduct(
        string productId,
        string priceString,
        float price,
        string isoCurrencyCode
    )
    {
        /*Debug.Log(
            "IAPModel.SetPriceForProduct: "
                + productId
                + "; priceString "
                + priceString
                + "; price "
                + price
                + "; isoCurrencyCode "
                + isoCurrencyCode
        );*/
        prices[productId] = new PriceData()
        {
            priceString = priceString,
            price = price,
            isoCurrencyCode = isoCurrencyCode
        };
        FilePrefs.SetString("IAPModel.prices", JsonUtility.ToJson(prices));
        OnPricesSet?.Invoke(productId);
    }

    public int GetKeepUpOfferCash()
    {
        return 50000;
    }

    public string GetProductIdByIndex(int index)
    {
        return productIds[index];
    }
}
