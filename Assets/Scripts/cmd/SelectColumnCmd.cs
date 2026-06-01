
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectColumnCmd
{
    private SlotElementData data;
    private SlotElement element;

    public SelectColumnCmd(int columnIndex)
    {
        var elementData = SlotModel.Instance.GetNextSlotElementDataInColumn(columnIndex);
        Assert.IsNotNull(elementData, $"SelectColumnCmd: No element found in column {columnIndex}");
        this.data = elementData;
    }

    public void Run()
    {
        Debug.Log($"SelectColumnCmd: type={this.data.type}, brickData={this.data.brickData}");
        if (this.data.type == SlotElementType.Coins)
        {
            Debug.Log("SelectColumnCmd: Coins selected");
            if (this.element != null)
                ViewModel.Instance.FlyCoin(this.element.coins.transform.position);
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }
        var element = CityModel.Instance.GetCurrentElement();
        if (!SlotModel.Instance.HasEmitterSpace() && element.dataContainer.ElementCountEmittingBricks() == 0)
        {
            this.ShowOutOfSpace();
            return;
        }

        if (this.data.type == SlotElementType.Bricks)
        {
            var hasEmitterSpace = SlotModel.Instance.HasEmitterSpace();
            if (!hasEmitterSpace)
            {
                return;
            }

            SlotModel.Instance.MoveFromColumnToEmitter(this.data.brickData);
            return;
        }
        if (this.data.type == SlotElementType.AddMoreBricks)
        {
            element.dataContainer.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement * 2);
            element.ShowCurrentState();
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }

        if (this.data.type == SlotElementType.Ad)
        {
            if (AdModel.Instance.IsAdReady(RewardName.INTERSTITIAL))
                new ShowAdCmd().Run(RewardName.INTERSTITIAL);
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }
    }

    private void ShowOutOfSpace()
    {
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
    }
}