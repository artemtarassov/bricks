using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnAddSpaceIAP : MonoBehaviour
{

    void OnEnable()
    {
        var txt = this.GetComponentInChildren<TMP_Text>();
        if (!IAPModel.Instance.HasPriceForProduct(IAPModel.AdditionalSpace))
        {
            txt.text = "-";
            return;
        }
        txt.text = IAPModel.Instance.GetPriceForProduct(IAPModel.AdditionalSpace);
    }
}