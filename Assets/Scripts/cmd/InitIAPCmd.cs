using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class InitIAPCmd
{
    private static MyIAPManager manager;

    public void Run()
    {
        if (manager == null)
        {
            manager = new MyIAPManager();
        }

        manager.Initialize();
    }
}

class MyIAPManager
{
    private readonly List<ProductDefinition> productDefinitions = new List<ProductDefinition>()
    {
        new ProductDefinition(IAPModel.AdditionalSpace, ProductType.NonConsumable),
        new ProductDefinition(IAPModel.GoldenTicket, ProductType.NonConsumable),
        new ProductDefinition(IAPModel.CashPack1, ProductType.Consumable),
        new ProductDefinition(IAPModel.CashPack2, ProductType.Consumable),
        new ProductDefinition(IAPModel.CashPack3, ProductType.Consumable),
    };

    private StoreController storeController;
    private bool isInitialized;
    private bool isInitializing;
    private bool modelEventsSubscribed;
    private bool isRestoring = false;
    private readonly HashSet<string> restoredProductIds = new HashSet<string>();

    public void Initialize()
    {
        if (isInitialized || isInitializing)
        {
            return;
        }

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        isInitializing = true;
        storeController = UnityIAPServices.StoreController();
        SubscribeToStoreEvents();
        SubscribeToModelEvents();

        try
        {
            await storeController.Connect();
            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("MyIAPManager Connect failed: " + e.Message);
        }
        finally
        {
            isInitializing = false;
        }
    }

    private void SubscribeToStoreEvents()
    {
        storeController.OnStoreConnected += OnStoreConnected;
        storeController.OnStoreDisconnected += OnStoreDisconnected;
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
    }

    private void SubscribeToModelEvents()
    {
        if (modelEventsSubscribed || IAPModel.Instance == null)
        {
            return;
        }

        IAPModel.Instance.OnPurchaseRequest += OnPurchaseRequest;
        IAPModel.Instance.OnRestorePurchases += OnRestorePurchases;
        modelEventsSubscribed = true;
    }

    private void OnStoreConnected()
    {
        SubscribeToModelEvents();
        storeController.FetchProducts(productDefinitions);
    }

    private void OnRestorePurchases()
    {
        if (storeController == null)
        {
            Debug.LogWarning("MyIAPManager OnRestorePurchases called before store initialization");
            return;
        }

        isRestoring = true;
        restoredProductIds.Clear();
        new HideViewCmd().Run();
        new ShowViewCmd().Run(ViewName.LoadingView);

        storeController.RestoreTransactions(
            (bool b, string error) =>
            {
                if (!b)
                {
                    FinishRestore(IapResponse.Failed, error);
                }
            }
        );
    }

    private void FinishRestore(IapResponse response, string message = null)
    {
        isRestoring = false;
        new HideViewCmd().Run(ViewName.LoadingView);

        foreach (var productId in restoredProductIds)
        {
            new CompleteIapCmd(productId, message).Run(response);
        }

        if (response == IapResponse.Failed)
        {
            Debug.LogError("MyIAPManager OnRestorePurchases failed: " + message);
        }
    }

    private void OnPurchaseRequest(string productId)
    {
        if (storeController == null)
        {
            Debug.LogWarning("MyIAPManager OnPurchaseRequest called before store initialization");
            return;
        }

        new ShowViewCmd().Run(ViewName.LoadingView);

        try
        {
            storeController.PurchaseProduct(productId);
        }
        catch (Exception e)
        {
            new HideViewCmd().Run(ViewName.LoadingView);
            Debug.LogWarning("MyIAPManager PurchaseProduct failed: " + e.Message);
        }
    }

    private void OnProductsFetched(List<Product> products)
    {
        foreach (var product in products)
        {
            var localizedPrice = (float)product.metadata.localizedPrice;
            var localizedPriceString = product.metadata.localizedPriceString;
            if (!string.IsNullOrEmpty(localizedPriceString) && IAPModel.Instance != null)
            {
                IAPModel.Instance.SetPriceForProduct(
                    product.definition.id,
                    localizedPriceString,
                    localizedPrice,
                    product.metadata.isoCurrencyCode
                );
            }
        }

        if (products.Count == 0)
        {
            Debug.LogError("MyIAPManager no products found");
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError("MyIAPManager OnProductsFetchFailed: " + failure.FailureReason);
    }

    private void OnPurchasePending(PendingOrder order)
    {
        var validated = true;
#if !UNITY_EDITOR
        validated = Validate(order);
#endif

        if (!validated)
        {
            CompletePendingOrder(order, IapResponse.Failed, order.Info.TransactionID);
            return;
        }

        if (isRestoring)
        {
            AddRestoredProductIds(order);
            storeController.ConfirmPurchase(order);
            return;
        }

        CompletePendingOrder(order, IapResponse.Success, order.Info.TransactionID);
    }

    private void CompletePendingOrder(PendingOrder order, IapResponse response, string message)
    {
        foreach (var cartItem in order.CartOrdered.Items())
        {
            if (cartItem == null || cartItem.Product == null)
            {
                continue;
            }

            new CompleteIapCmd(cartItem.Product.definition.id, message).Run(response);
        }

        new HideViewCmd().Run(ViewName.LoadingView);
        storeController.ConfirmPurchase(order);
    }

    private bool Validate(Order order)
    {
        try
        {
#if UNITY_ANDROID
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
            var result = validator.Validate(order.Info.Receipt);
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate.ToString());
                Debug.Log(productReceipt.transactionID);
            }
#endif
            return true;
        }
        catch (IAPSecurityException ex)
        {
            Debug.LogError("Invalid receipt, not unlocking content. " + ex);
            return false;
        }
    }

    private void OnPurchaseConfirmed(Order order)
    {
        if (order is FailedOrder failedOrder)
        {
            Debug.LogWarning(
                "MyIAPManager ConfirmPurchase failed: "
                    + failedOrder.FailureReason
                    + " details:"
                    + failedOrder.Details
            );
        }
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        new HideViewCmd().Run(ViewName.LoadingView);
        var urlEncodedMsg = Uri.EscapeDataString(failedOrder.Details);

        foreach (var cartItem in failedOrder.CartOrdered.Items())
        {
            if (cartItem == null || cartItem.Product == null)
            {
                continue;
            }

            new CompleteIapCmd(cartItem.Product.definition.id, urlEncodedMsg).Run(IapResponse.Failed);
            Debug.LogWarning(
                "MyIAPManager OnPurchaseFailed: "
                    + cartItem.Product.definition.id
                    + " PurchaseFailureReason:"
                    + failedOrder.FailureReason
                    + " details:"
                    + failedOrder.Details
            );
        }
    }

    private void OnPurchaseDeferred(DeferredOrder deferredOrder)
    {
        new HideViewCmd().Run(ViewName.LoadingView);

        foreach (var cartItem in deferredOrder.CartOrdered.Items())
        {
            if (cartItem == null || cartItem.Product == null)
            {
                continue;
            }

            Debug.LogWarning("MyIAPManager OnPurchaseDeferred: " + cartItem.Product.definition.id);
        }
    }

    private void OnPurchasesFetched(Orders orders)
    {
        if (!isRestoring)
        {
            return;
        }

        foreach (var order in orders.ConfirmedOrders)
        {
            AddRestoredProductIds(order);
        }

        foreach (var order in orders.PendingOrders)
        {
            AddRestoredProductIds(order);
        }

        FinishRestore(IapResponse.Restore);
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        if (!isRestoring)
        {
            return;
        }

        FinishRestore(IapResponse.Failed, failure.Message);
    }

    private void AddRestoredProductIds(Order order)
    {
        foreach (var cartItem in order.CartOrdered.Items())
        {
            if (cartItem != null && cartItem.Product != null)
            {
                restoredProductIds.Add(cartItem.Product.definition.id);
            }
        }
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.LogWarning("MyIAPManager Store disconnected: " + description.message);
    }
}
