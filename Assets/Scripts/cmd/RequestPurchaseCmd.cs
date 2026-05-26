public class RequestPurchaseCmd
{
    private string productId;

    public RequestPurchaseCmd(string productId)
    {
        this.productId = productId;
    }

    public void Run()
    {
        IAPModel.Instance.RequestPurchase(this.productId);
    }
}