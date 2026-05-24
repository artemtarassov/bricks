using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class InitIAPCmd
{
    public void Run()
    {
        InitIap();
    }

    private static MyIAPManager manager;

    private void InitIap()
    {
        manager = new MyIAPManager();
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(IAPModel.IncomeSpeedup, ProductType.NonConsumable);
        builder.AddProduct(IAPModel.GoldenTicket, ProductType.NonConsumable);
        builder.AddProduct(IAPModel.GoldenTicketOffer, ProductType.NonConsumable);
        builder.AddProduct(IAPModel.UnlockRoom, ProductType.Consumable);
        builder.AddProduct(IAPModel.ItemsPack1, ProductType.Consumable);
        builder.AddProduct(IAPModel.ItemsPack2, ProductType.Consumable);
        builder.AddProduct(IAPModel.ItemsPack3, ProductType.Consumable);
        builder.AddProduct(IAPModel.PersonalPet, ProductType.Consumable);
        builder.AddProduct(IAPModel.CashPack1, ProductType.Consumable);
        builder.AddProduct(IAPModel.CashPack2, ProductType.Consumable);
        builder.AddProduct(IAPModel.CashPack3, ProductType.Consumable);
        builder.AddProduct(IAPModel.KeepUpOffer, ProductType.Consumable);
        UnityPurchasing.Initialize(manager, builder);
    }
}

class MyIAPManager : IDetailedStoreListener
{
    private IStoreController controller;
    private IExtensionProvider extensions;

    private bool isRestoring = false;
    private HashSet<string> restoredProductIds = new HashSet<string>();

    private void OnRestorePurchases()
    {
        var e = extensions.GetExtension<IAppleExtensions>(); 

        this.isRestoring = true;
        new HideViewCmd().Run();
        new ShowViewCmd().Run(ViewName.LoadIapView);

        e.RestoreTransactions(
            (bool b, string error) =>
            {
                isRestoring = false;
                new HideViewCmd().Run(ViewName.LoadIapView);
                if (b)
                {
                    foreach (var productId in restoredProductIds)
                    {
                        //Debug.Log("MyIAPManager OnRestorePurchases restored: " + productId);
                        new CompleteIapCmd(productId).Run(IapResponse.Restore); 
                    }
                }
                else
                {
                    foreach (var productId in restoredProductIds)
                    {
                        //Debug.Log("MyIAPManager OnRestorePurchases restored: " + productId);
                        new CompleteIapCmd(productId).Run(IapResponse.Failed);
                    }
                    Debug.LogError("MyIAPManager OnRestorePurchases failed: " + error);
                }
            }
        );
        //add listener
    }

    private void OnPurchaseRequest(string p)
    {
        //Debug.Log("InitIap OnPurchaseRequest: " + p);
        //new HideViewCmd().Run();
        new ShowViewCmd().Run(ViewName.LoadIapView); 
        this.controller.InitiatePurchase(p);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        //Debug.Log("MyIAPManager OnInitialized");
        this.controller = controller;
        this.extensions = extensions;

        IAPModel.Instance.OnPurchaseRequest += OnPurchaseRequest;
        IAPModel.Instance.OnRestorePurchases += OnRestorePurchases;

        foreach (var product in controller.products.all)
        {
            var localizedPrice = (float)(product.metadata.localizedPrice);
            var localizedPriceString = product.metadata.localizedPriceString;
            if (!string.IsNullOrEmpty(localizedPriceString))
            {
                IAPModel.Instance.SetPriceForProduct(
                    product.definition.id,
                    localizedPriceString,
                    localizedPrice,
                    product.metadata.isoCurrencyCode
                );
            }
        }

        if (controller.products.all.Length == 0)
        {
            Debug.LogError("MyIAPManager no products found");
        }
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning("MyIAPManager OnInitializeFailed InitializationFailureReason:" + error);
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        //Debug.Log("MyIAPManager ProcessPurchase: " + e.purchasedProduct.definition.id);
        var productId = e.purchasedProduct.definition.id.ToString();
        var transactionID = e.purchasedProduct.transactionID;


        var validated = true;
        #if !UNITY_EDITOR
        validated = Validate(e);
        #endif

        if (!validated)
        {
            var p = e.purchasedProduct;
            new CompleteIapCmd(productId, transactionID).Run(IapResponse.Failed);
            new HideViewCmd().Run(ViewName.LoadIapView);
            return PurchaseProcessingResult.Complete;
        }

        if (isRestoring)
        {
            restoredProductIds.Add(productId);
            return PurchaseProcessingResult.Complete;
        }

        new CompleteIapCmd(productId, transactionID).Run(IapResponse.Success);
        new HideViewCmd().Run(ViewName.LoadIapView);
        return PurchaseProcessingResult.Complete;
    }

    private bool Validate(PurchaseEventArgs e)
    {
        bool validPurchase = true;

        try
        {
            #if UNITY_ANDROID
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), null, Application.identifier);
            #elif UNITY_IOS || UNITY_STANDALONE_OSX
            var validator = new CrossPlatformValidator(null, AppleTangle.Data(), Application.identifier);
            #else
            return true;
            #endif
            // On Google Play, result has a single product ID.
            var result = validator.Validate(e.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate.ToString());
                Debug.Log(productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException ex)
        {
            Debug.LogError("Invalid receipt, not unlocking content. " + ex);
            validPurchase = false;
        }
        return validPurchase;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// IStoreListener.OnPurchaseFailed is deprecated,
    /// use IDetailedStoreListener.OnPurchaseFailed instead.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        //IAPModel.Instance.SetPurchaseFailed(i.definition.id);
        new CompleteIapCmd(i.definition.id, p.ToString()).Run(IapResponse.Failed);
        new HideViewCmd().Run(ViewName.LoadIapView);
        Debug.LogWarning(
            "MyIAPManager OnPurchaseFailed: " + i.definition.id + " PurchaseFailureReason:" + p
        );
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        new HideViewCmd().Run(ViewName.LoadIapView);
        var urlEncodedMsg = System.Uri.EscapeDataString(failureDescription.message);
        new CompleteIapCmd(product.definition.id, urlEncodedMsg).Run(IapResponse.Failed);
        //IAPModel.Instance.SetPurchaseFailed(product.definition.id);
        Debug.LogWarning(
            "MyIAPManager OnPurchaseFailed: "
                + product.definition.id
                + " PurchaseFailureDescription:"
                + failureDescription
        );
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning("MyIAPManager InitializationFailureReason:" + error + " message:" + message);
    }
}
