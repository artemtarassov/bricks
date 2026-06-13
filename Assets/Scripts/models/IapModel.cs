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

[Serializable]
public class CompletedPurchase
{
    public string productId;
    public int timestamp;
}


[Serializable]
public class CompletedPurchaseContainer
{
    public List<CompletedPurchase> purchases = new List<CompletedPurchase>();

    [System.NonSerialized]
    public bool dirty = false;
}



public class IAPModel
{
    public static IAPModel Instance;

    public readonly List<string> productIds = new List<string>();
    private Dictionary<string, PriceData> prices = new Dictionary<string, PriceData>();

    public const string GoldenTicketTemp = "de.badmonkee.solari.goldentickettemp";
    public const string GoldenTicket = "de.badmonkee.solari.goldenticket";
    public const string AdditionalSpace = "de.badmonkee.solari.morespace";

    public const string CashPack1 = "de.badmonkee.solari.smallcashpack";//Small cash pack: Receive a small cash pack immediately
    public const string CashPack2 = "de.badmonkee.solari.mediumcashpack";//Medium cash pack: Receive a medium cash pack immediately
    public const string CashPack3 = "de.badmonkee.solari.largecashpack";//Large cash pack: Receive a large cash pack immediately

    public static bool IsCashPack(string productId)
    {
        return productId == CashPack1 || productId == CashPack2 || productId == CashPack3;
    }

    public Action<string> OnPurchaseSuccess;
    public Action<string> OnPurchaseFailed;
    public Action<string> OnPurchaseRequest;
    public Action OnRestorePurchases;

    public Action<string> OnPricesSet;


    private CompletedPurchaseContainer completedPurchaseContainer;

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

    public static string GetProductIdByIAPProductName(IAPProductName productName)
    {
        switch (productName)
        {
            case IAPProductName.GoldenTicket:
                return GoldenTicket;
            case IAPProductName.GoldenTicketTemp:
                return GoldenTicketTemp;
            case IAPProductName.AdditionalSpace:
                return AdditionalSpace;
            default:
                return null;
        }
    }

    public bool HasGoldenTicket()
    {
        return DidPurchaseComplete(GoldenTicket) || DidPurchaseComplete(GoldenTicketTemp);
    }

    public void Load()
    {
        if (FilePrefs.HasKey("IAPModel.completedPurchases"))
        {
            try
            {
                completedPurchaseContainer = JsonUtility.FromJson<CompletedPurchaseContainer>(FilePrefs.GetString("IAPModel.completedPurchases"));
            }
            catch (Exception e)
            {
                Debug.LogError("IAPModel Load: " + e.Message);
                completedPurchaseContainer = new CompletedPurchaseContainer();
            }
        }
        else
        {
            completedPurchaseContainer = new CompletedPurchaseContainer();
        }
    }

    public void Save()
    {
        if (this.completedPurchaseContainer.dirty == false)
        {
            return;
        }
        FilePrefs.SetString("IAPModel.completedPurchases", JsonUtility.ToJson(completedPurchaseContainer));
        this.completedPurchaseContainer.dirty = false;
    }

    public bool DidPurchaseComplete(string productId)
    {
        return completedPurchaseContainer.purchases.Exists(p => p.productId == productId);
    }

    public bool DidPurchaseComplete(IAPProductName productName)
    {
        var productId = GetProductIdByIAPProductName(productName);
        if (string.IsNullOrEmpty(productId))
        {
            return false;
        }
        return DidPurchaseComplete(productId);
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
        this.completedPurchaseContainer.purchases.Add(new CompletedPurchase()
        {
            productId = productUd,
            timestamp = TimeUtils.GetUnixTimestamp()
        });
        this.completedPurchaseContainer.dirty = true;
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

    public string GetProductIdByIndex(int index)
    {
        return productIds[index];
    }
}
