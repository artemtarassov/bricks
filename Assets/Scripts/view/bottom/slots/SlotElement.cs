using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotElement : MonoBehaviour
{
    //[SerializeField] private UIBrick brick;
    [SerializeField] private GameObject addMoreBricks;
    [SerializeField] private GameObject coins;
    [SerializeField] private GameObject hiddenBricks;

    [SerializeField] private TMP_Text count;
    [SerializeField] private Image colorImg;

    [HideInInspector]
    public SlotElementData slotElementData;

    public void Setup(SlotElementData data)
    {
        this.slotElementData = data;
        if (data == null)
        {
            SetupAsEmpty();
            return;
        }
        if (data.type == SlotElementType.Bricks)
        {
            SetupWithBricks();
            return;
        }
        if (data.type == SlotElementType.AddMoreBricks)
        {
            SetupWithAddMoreBricks();
            return;
        }
        if (data.type == SlotElementType.Coins)
        {
            SetupWithCoins();
            return;
        }
        if (data.type == SlotElementType.HiddenBricks)
        {
            SetupWithHiddenBricks();
            return;
        }
    }

    private void SetupWithHiddenBricks()
    {
        this.SetupAsEmpty();
        this.ShowAmount();
        this.hiddenBricks.SetActive(true);
    }

    private void ShowColor()
    {
        this.colorImg.gameObject.SetActive(true);
        this.colorImg.color = ColoredMaterials.Instance.GetColorByColorIndex(this.slotElementData.brickData.color);
    }

    private void ShowAmount()
    {
        this.count.text = this.slotElementData.brickData.coloredAmount.ToString();
        this.count.gameObject.SetActive(true);
    }

    private void SetupWithCoins()
    {
        this.SetupAsEmpty();
        this.coins.SetActive(true);
    }

    private void SetupWithBricks()
    {
        this.SetupAsEmpty();
        this.ShowColor();
        this.ShowAmount();
    }

    private void SetupWithAddMoreBricks()
    {
        this.SetupAsEmpty();
        this.addMoreBricks.SetActive(true);
    }

    private void SetupAsEmpty()
    {
        this.colorImg.gameObject.SetActive(false);
        this.addMoreBricks.SetActive(false);
        this.coins.SetActive(false);
        this.hiddenBricks.SetActive(false);
        this.count.gameObject.SetActive(false);
    }
}