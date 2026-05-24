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
    public const string UnlockRoom = "com.badmonkee.roommates.unlockroom";
    public const string IncomeSpeedup = "com.badmonkee.roommates.incomespeedup";
    public const string GoldenTicketOffer = "com.badmonkee.roommates.removeadsoffer"; //not implemented yet
    public const string ItemsPack1 = "com.badmonkee.roommates.itemspack1";
    public const string ItemsPack2 = "com.badmonkee.roommates.itemspack2";//same price as ItemsPack1, but different items
    public const string ItemsPack3 = "com.badmonkee.roommates.itemspack3";
    public const string GoldenTicket = "com.badmonkee.roommates.removeads";
    public const string PersonalPet = "com.badmonkee.roommates.personalpet";

    public const string CashPack1 = "com.badmonkee.roommates.smallcashpack";//Small cash pack: Receive a small cash pack immediately
    public const string CashPack2 = "com.badmonkee.roommates.mediumcashpack";//Medium cash pack: Receive a medium cash pack immediately
    public const string CashPack3 = "com.badmonkee.roommates.largecashpack";//Large cash pack: Receive a large cash pack immediately

    public const string KeepUpOffer = "com.badmonkee.roommates.keepupoffer";

    public static bool IsItemPack(string productId)
    {
        return productId == ItemsPack1 || productId == ItemsPack2 || productId == ItemsPack3;
    }

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
        productIds.Add(UnlockRoom);
        productIds.Add(IncomeSpeedup);
        productIds.Add(GoldenTicket);
        productIds.Add(GoldenTicketOffer);
        productIds.Add(ItemsPack1);
        productIds.Add(ItemsPack2);
        productIds.Add(ItemsPack3);
        productIds.Add(PersonalPet);
        productIds.Add(CashPack1);
        productIds.Add(CashPack2);
        productIds.Add(CashPack3);
        productIds.Add(KeepUpOffer);
        var pricesJson = FilePrefs.GetString("IAPModel.prices", "{}");
        prices = JsonUtility.FromJson<Dictionary<string, PriceData>>(pricesJson);
    }

    public bool IsGoldenTicket(string p)
    {
        return p == GoldenTicket || p == GoldenTicketOffer;
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
