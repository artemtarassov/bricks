public class RequestPurchaseCmd
{
    private string productId;

    public RequestPurchaseCmd(string productId)
    {
        this.productId = productId;
    }

    public RequestPurchaseCmd(IAPProductName productName)
    {
        this.productId = IAPModel.GetProductIdByIAPProductName(productName);
    }


    public void Run()
    {
        IAPModel.Instance.RequestPurchase(this.productId);
    }
}