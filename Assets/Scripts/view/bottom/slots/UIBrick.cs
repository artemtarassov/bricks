using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIBrick : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private TMP_Text amountTxt;

    public void SetData(BrickData brickData = null)
    {
        if (brickData == null)
        {
            this.img.gameObject.SetActive(false);
            this.amountTxt.gameObject.SetActive(false);
            return;
        }
        this.img.gameObject.SetActive(true);
        this.amountTxt.gameObject.SetActive(true);
        var color = ColoredMaterials.Instance.GetColorByColorIndex(brickData.color);
        this.img.color = color;
        this.amountTxt.text = brickData.coloredAmount.ToString();
    }
}