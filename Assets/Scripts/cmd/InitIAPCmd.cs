using System;
using System.Collections.Generic;
using System.Linq;
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
        new ProductDefinition(IAPModel.AdditionalSpace, IAPModel.AdditionalSpace, ProductType.NonConsumable),
        new ProductDefinition(IAPModel.GoldenTicket, IAPModel.GoldenTicket, ProductType.NonConsumable),
        new ProductDefinition(IAPModel.GoldenTicketTemp, IAPModel.GoldenTicketTemp, ProductType.Consumable),
    };

    private StoreController storeController;
    private CrossPlatformValidator googlePlayValidator;
    private bool isInitialized;
    private bool isInitializing;
    private bool modelEventsSubscribed;
    private bool isRestoring = false;
    private bool isFetchingExistingPurchases;
    private bool appleValidationWarningShown;
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
        LogRequestedProducts("InitializeAsync");

        try
        {
            await storeController.Connect();
            isInitialized = true;
            Debug.Log(
                "MyIAPManager Connect succeeded for store "
                    + StandardPurchasingModule.Instance().appStore
                    + " and app "
                    + Application.identifier
            );
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
        EnableAppleAppReceiptRefresh();
        ValidateRequestedProducts();
        LogRequestedProducts("OnStoreConnected");
        storeController.FetchProducts(productDefinitions);
        InitializeReceiptValidation();
        isFetchingExistingPurchases = true;
        storeController.FetchPurchases();
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
        new ShowViewCmd(ViewName.LoadingView).Run();

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
        new HideViewCmd(ViewName.LoadingView).Run();

        foreach (var productId in restoredProductIds)
        {
            new CompleteIapCmd(productId, message).Run(response);
        }

        SavePurchaseStateImmediately();

        if (response == IapResponse.Failed)
        {
            Debug.LogWarning("MyIAPManager OnRestorePurchases failed: " + message);
        }
    }

    private void OnPurchaseRequest(string productId)
    {
        if (storeController == null)
        {
            Debug.LogWarning("MyIAPManager OnPurchaseRequest called before store initialization");
            return;
        }

        new ShowViewCmd(ViewName.LoadingView).Run();

        try
        {
            storeController.PurchaseProduct(productId);
        }
        catch (Exception e)
        {
            new HideViewCmd(ViewName.LoadingView).Run();
            Debug.LogWarning("MyIAPManager PurchaseProduct failed: " + e.Message);
        }
    }

    private void OnProductsFetched(List<Product> products)
    {
        Debug.Log(
            "MyIAPManager OnProductsFetched: fetched "
                + products.Count
                + " products. "
                + FormatFetchedProducts(products)
        );

        foreach (var product in products)
        {
            var localizedPrice = (float)product.metadata.localizedPrice;
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

        if (products.Count == 0)
        {
            Debug.LogError(
                "MyIAPManager no products found for requested definitions "
                    + FormatProductDefinitions(productDefinitions)
            );
            return;
        }

        var missingDefinitions = productDefinitions
            .Where(
                definition =>
                    products.All(
                        product => product.definition.storeSpecificId != definition.storeSpecificId
                    )
            )
            .ToList();
        if (missingDefinitions.Count > 0)
        {
            Debug.LogWarning(
                "MyIAPManager OnProductsFetched: Google Play did not return all requested products. Missing: "
                    + FormatProductDefinitions(missingDefinitions)
            );
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError(
            "MyIAPManager OnProductsFetchFailed: "
                + failure.FailureReason
                + ". failedCount="
                + failure.FailedFetchProducts.Count
                + ", requested="
                + FormatProductDefinitions(productDefinitions)
                + ", failed="
                + FormatProductDefinitions(failure.FailedFetchProducts)
                + ", store="
                + StandardPurchasingModule.Instance().appStore
                + ", appId="
                + Application.identifier
        );
        LogGooglePlayFetchTroubleshootingHint();
    }

    private void OnPurchasePending(PendingOrder order)
    {
        LogAppleReceiptState(order, "OnPurchasePending");

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

        if (
            IAPModel.Instance.HasCompletedTransaction(order.Info.TransactionID)
        )
        {
            Debug.Log(
                "MyIAPManager OnPurchasePending: transaction already processed, confirming "
                    + order.Info.TransactionID
            );
            new HideViewCmd(ViewName.LoadingView).Run();
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

            new CompleteIapCmd(cartItem.Product.definition.id, message, order.Info.TransactionID).Run(
                response
            );
        }

        SavePurchaseStateImmediately();
        new HideViewCmd(ViewName.LoadingView).Run();
        storeController.ConfirmPurchase(order);
    }

    private bool Validate(Order order)
    {
        if (order?.Info == null || string.IsNullOrEmpty(order.Info.Receipt))
        {
            Debug.LogError("MyIAPManager Validate failed: order receipt is missing");
            return false;
        }

        if (IsGooglePlayStoreSelected())
        {
            try
            {
                googlePlayValidator ??= new CrossPlatformValidator(
                    GooglePlayTangle.Data(),
                    Application.identifier
                );

                var result = googlePlayValidator.Validate(order.Info.Receipt);
                Debug.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate.ToString());
                    Debug.Log(productReceipt.transactionID);
                }

                return true;
            }
            catch (IAPSecurityException ex)
            {
                Debug.LogError("Invalid Google Play receipt, not unlocking content. " + ex);
                return false;
            }
        }

        if (IsAppleStoreSelected())
        {
            if (!appleValidationWarningShown)
            {
                appleValidationWarningShown = true;
                Debug.LogWarning(
                    "MyIAPManager is skipping local Apple receipt validation. "
                        + "Unity IAP 5.x with StoreKit 2 requires server-side validation "
                        + "using order.Info.Apple?.jwsRepresentation."
                );
            }

            return true;
        }

        Debug.LogError(
            "MyIAPManager Validate failed: unsupported store for local receipt validation: "
                + StandardPurchasingModule.Instance().appStore
        );
        return false;
    }

    private void EnableAppleAppReceiptRefresh()
    {
        if (!IsAppleStoreSelected())
        {
            return;
        }

        var applePurchaseService = storeController.AppleStoreExtendedPurchaseService;
        if (applePurchaseService == null)
        {
            Debug.LogWarning(
                "MyIAPManager AppleStoreExtendedPurchaseService is unavailable, cannot enable receipt refresh"
            );
            return;
        }

        applePurchaseService.SetRefreshAppReceipt(true);
        Debug.Log("MyIAPManager enabled automatic Apple app receipt refresh");
    }

    private static void LogAppleReceiptState(Order order, string context)
    {
        if (!IsAppleStoreSelected())
        {
            return;
        }

        var unifiedReceiptLength = order?.Info?.Receipt?.Length ?? 0;
        var appReceiptLength = order?.Info?.Apple?.AppReceipt?.Length ?? 0;
        var jwsLength = order?.Info?.Apple?.jwsRepresentation?.Length ?? 0;
        var transactionId = order?.Info?.TransactionID ?? "";

        Debug.Log(
            "MyIAPManager "
                + context
                + " Apple receipt state: transactionId="
                + transactionId
                + ", unifiedReceiptLength="
                + unifiedReceiptLength
                + ", appReceiptLength="
                + appReceiptLength
                + ", jwsLength="
                + jwsLength
        );
    }

    private void InitializeReceiptValidation()
    {
        if (!IsGooglePlayStoreSelected())
        {
            googlePlayValidator = null;
            return;
        }

        try
        {
            googlePlayValidator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                Application.identifier
            );
        }
        catch (IAPSecurityException ex)
        {
            googlePlayValidator = null;
            Debug.LogError("MyIAPManager could not initialize Google Play validator. " + ex);
        }
    }

    private static bool IsGooglePlayStoreSelected()
    {
        return StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay;
    }

    private static bool IsAppleStoreSelected()
    {
        var appStore = StandardPurchasingModule.Instance().appStore;
        return appStore == AppStore.AppleAppStore || appStore == AppStore.MacAppStore;
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
        new HideViewCmd(ViewName.LoadingView).Run();
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
        new HideViewCmd(ViewName.LoadingView).Run();

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
        if (isFetchingExistingPurchases)
        {
            isFetchingExistingPurchases = false;
            SyncOwnedPurchases(orders);
        }

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
        if (isFetchingExistingPurchases)
        {
            isFetchingExistingPurchases = false;
            Debug.LogWarning("MyIAPManager OnPurchasesFetchFailed during startup sync: " + failure.Message);
        }

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

    private void SyncOwnedPurchases(Orders orders)
    {
        var didApplyAnyPurchase = false;

        foreach (var order in orders.ConfirmedOrders)
        {
            didApplyAnyPurchase |= ApplyOwnedOrder(order);
        }

        if (didApplyAnyPurchase)
        {
            SavePurchaseStateImmediately();
        }
    }

    private bool ApplyOwnedOrder(Order order)
    {
        var didApply = false;
        var transactionId = order?.Info?.TransactionID;

        foreach (var cartItem in order.CartOrdered.Items())
        {
            if (cartItem == null || cartItem.Product == null)
            {
                continue;
            }

            didApply = true;
            new CompleteIapCmd(cartItem.Product.definition.id, null, transactionId).Run(
                IapResponse.Restore
            );
        }

        return didApply;
    }

    private static void SavePurchaseStateImmediately()
    {
        PlayerModel.Instance?.Save();
        IAPModel.Instance?.Save();
        FilePrefs.Save();
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.LogWarning("MyIAPManager Store disconnected: " + description.message);
    }

    private void ValidateRequestedProducts()
    {
        var duplicateIds = productDefinitions
            .GroupBy(definition => definition.storeSpecificId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateIds.Count > 0)
        {
            Debug.LogWarning(
                "MyIAPManager duplicate storeSpecificIds found in product definitions: "
                    + string.Join(", ", duplicateIds)
            );
        }

        var invalidDefinitions = productDefinitions
            .Where(
                definition =>
                    string.IsNullOrWhiteSpace(definition.id)
                    || string.IsNullOrWhiteSpace(definition.storeSpecificId)
            )
            .ToList();
        if (invalidDefinitions.Count > 0)
        {
            Debug.LogWarning(
                "MyIAPManager invalid product definitions detected: "
                    + FormatProductDefinitions(invalidDefinitions)
            );
        }
    }

    private void LogRequestedProducts(string context)
    {
        Debug.Log(
            "MyIAPManager "
                + context
                + " requested products: "
                + FormatProductDefinitions(productDefinitions)
        );
    }

    private static string FormatProductDefinitions(IEnumerable<ProductDefinition> definitions)
    {
        var formatted = definitions
            .Select(
                definition =>
                    "{id="
                    + definition.id
                    + ", storeSpecificId="
                    + definition.storeSpecificId
                    + ", type="
                    + definition.type
                    + "}"
            )
            .ToList();
        return formatted.Count == 0 ? "[]" : "[" + string.Join(", ", formatted) + "]";
    }

    private static string FormatFetchedProducts(IEnumerable<Product> products)
    {
        var formatted = products
            .Select(
                product =>
                    "{id="
                    + product.definition.id
                    + ", storeSpecificId="
                    + product.definition.storeSpecificId
                    + ", type="
                    + product.definition.type
                    + ", price="
                    + product.metadata.localizedPriceString
                    + "}"
            )
            .ToList();
        return formatted.Count == 0 ? "[]" : "[" + string.Join(", ", formatted) + "]";
    }

    private static void LogGooglePlayFetchTroubleshootingHint()
    {
        if (!IsGooglePlayStoreSelected())
        {
            return;
        }

        Debug.LogWarning(
            "MyIAPManager Google Play fetch checklist: verify each requested storeSpecificId exists in Play Console, "
                + "is Active, is published to the same applicationId, and the test build is installed from a Play testing track."
        );
    }
}
