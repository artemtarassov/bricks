using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnIAP : MonoBehaviour
{
    [SerializeField] public IAPProductName productName = IAPProductName.Undefined;


    public Action<IAPProductName> onClicked;

    void Awake()
    {
        Assert.IsNotNull(this.GetComponent<Button>());
        Assert.IsTrue(this.productName != IAPProductName.Undefined);
        this.GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        string productId = IAPModel.GetProductIdByIAPProductName(productName);
        var hasPrice = IAPModel.Instance.HasPriceForProduct(productId);
        if (!hasPrice)
        {
            new SoundCmd(SoundModel.Instance.ERROR).Run();
            return;
        }
        onClicked?.Invoke(this.productName);
    }

    void OnEnable()
    {
        var txt = this.GetComponentInChildren<TMP_Text>();
        string productId = IAPModel.GetProductIdByIAPProductName(productName);
        if (string.IsNullOrEmpty(productId))
        {
            txt.text = "-";
            return;
        }

        var hasPrice = IAPModel.Instance.HasPriceForProduct(productId);
        if (!hasPrice)
        {
            txt.text = "-";
            return;
        }
        txt.text = IAPModel.Instance.GetPriceForProduct(productId);
    }
}