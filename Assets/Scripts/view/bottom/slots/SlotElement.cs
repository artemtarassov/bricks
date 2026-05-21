using UnityEngine;

public class SlotElement : MonoBehaviour
{
    [SerializeField] private UIBrick brick;
    [SerializeField] private GameObject addMoreBricks;

    public BrickData brickData;

    public void Setup(SlotElementData data)
    {
        if (data == null)
        {
            SetupAsEmpty();
            return;
        }
        if (data.type == SlotElementType.Bricks)
        {
            SetupWithBricks(data.brickData);
            return;
        }
        if (data.type == SlotElementType.AddMoreBricks)
        {
            SetupWithAddMoreBricks();
            return;
        }
    }

    public void SetupWithBricks(BrickData brickData)
    {
        this.brickData = brickData;
        this.brick.SetData(brickData);
        this.brick.gameObject.SetActive(true);
        this.addMoreBricks.SetActive(false);
    }

    public void SetupWithAddMoreBricks()
    {
        this.brickData = null;
        this.brick.SetData(null);
        this.brick.gameObject.SetActive(false);
        this.addMoreBricks.SetActive(true);
    }

    public void SetupAsEmpty()
    {
        this.brickData = null;
        this.brick.SetData(null);
        this.brick.gameObject.SetActive(false);
        this.addMoreBricks.SetActive(false);
    }
}