using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PriceDataList
{
    public List<PriceData> prices = new List<PriceData>();
}

[Serializable]
public class PriceData
{
    public string priceString;
    public string isoCurrencyCode;
    public float price;
    public string productId;
}

[Serializable]
public class CompletedPurchase
{
    public string productId;
    public string transactionId;
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
    private PriceDataList priceDataList;

    public const string GoldenTicketTemp = "de.badmonkee.solari.goldentickettemp";
    public const string GoldenTicket = "de.badmonkee.solari.goldenticket";
    public const string AdditionalSpace = "de.badmonkee.solari.morespace";
    public const string PremiumBuilding1 = "de.badmonkee.solari.premiumbuilding1";

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
        productIds.Add(GoldenTicketTemp);
        productIds.Add(AdditionalSpace);
        productIds.Add(PremiumBuilding1);
    }

    public static string GetProductIdByIAPProductName(IAPProductName productName)
    {
        switch (productName)
        {
            case IAPProductName.PremiumBuilding1:
                return PremiumBuilding1;
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

    public void Load()
    {
        var pricesJson = FilePrefs.GetString("IAPModel.prices1", "");
        if (!string.IsNullOrEmpty(pricesJson))
        {
            try
            {
                priceDataList = JsonUtility.FromJson<PriceDataList>(pricesJson);
            }
            catch (Exception e)
            {
                Debug.LogError("IAPModel constructor: failed to load prices from prefs: " + e.Message);
                priceDataList = new PriceDataList();

            }
        }
        else
        {
            priceDataList = new PriceDataList();
        }

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

        if (completedPurchaseContainer == null)
        {
            completedPurchaseContainer = new CompletedPurchaseContainer();
        }

        if (completedPurchaseContainer.purchases == null)
        {
            completedPurchaseContainer.purchases = new List<CompletedPurchase>();
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
        TrySetPurchaseCompleted(productUd, null, false);
    }

    public bool HasCompletedTransaction(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            return false;
        }

        return completedPurchaseContainer.purchases.Exists(
            p => !string.IsNullOrEmpty(p.transactionId) && p.transactionId == transactionId
        );
    }

    public bool HasCompletedProduct(string productId)
    {
        return completedPurchaseContainer.purchases.Exists(p => p.productId == productId);
    }

    public bool TrySetPurchaseCompleted(
        string productId,
        string transactionId,
        bool dedupeByProductId
    )
    {
        if (HasCompletedTransaction(transactionId))
        {
            return false;
        }

        if (dedupeByProductId && HasCompletedProduct(productId))
        {
            return false;
        }

        OnPurchaseSuccess?.Invoke(productId);
        completedPurchaseContainer.purchases.Add(
            new CompletedPurchase()
            {
                productId = productId,
                transactionId = transactionId,
                timestamp = TimeUtils.GetUnixTimestamp()
            }
        );
        completedPurchaseContainer.dirty = true;
        return true;
    }

    public bool HasTempGoldenTicket()
    {
        var purchases = completedPurchaseContainer.purchases.FindAll(p => p.productId == GoldenTicketTemp);
        if (purchases.Count == 0)
        {
            return false; //should not happen, but just in case
        }
        var currentTime = TimeUtils.GetUnixTimestamp();
        var days = 7;
        var timeout = days * 24 * 60 * 60; //7 days in seconds
        return purchases.Any(p => p.timestamp + timeout > currentTime);
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
            return this.priceDataList.prices.Find(p => p.productId == productId).priceString;
        }
        return "?";
    }

    public PriceData GetPriceDataForProduct(string productId)
    {
        if (!HasPriceForProduct(productId))
        {
            return null;
        }
        return priceDataList.prices.Find(p => p.productId == productId);
    }

    public bool HasPriceForProduct(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return false;
        }
        return priceDataList.prices.Exists(p => p.productId == productId);
    }

    public void SetPriceForProduct(
        string productId,
        string priceString,
        float price,
        string isoCurrencyCode
    )
    {
        var existingPriceData = priceDataList.prices.Find(p => p.productId == productId);
        if (existingPriceData != null)
        {
            existingPriceData.priceString = priceString;
            existingPriceData.price = price;
            existingPriceData.isoCurrencyCode = isoCurrencyCode;
        }
        else
        {
            priceDataList.prices.Add(new PriceData()
            {
                productId = productId,
                priceString = priceString,
                price = price,
                isoCurrencyCode = isoCurrencyCode
            });
        }
        FilePrefs.SetString("IAPModel.prices1", JsonUtility.ToJson(priceDataList));
        OnPricesSet?.Invoke(productId);
    }

    public string GetProductIdByIndex(int index)
    {
        return productIds[index];
    }
}
