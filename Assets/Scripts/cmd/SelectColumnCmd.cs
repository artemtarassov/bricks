
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectColumnCmd
{
    private SlotElementData data;
    private SlotElement element;

    public SelectColumnCmd(SlotElement se)
    {
        this.element = se;
        this.data = se.slotElementData;
    }

    public void Run()
    {
        Debug.Log($"SelectColumnCmd: type={this.data.type}, brickData={this.data.brickData}");
        if (this.data.type == SlotElementType.Coins)
        {
            Debug.Log("SelectColumnCmd: Coins selected");
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
            SlotModel.Instance.MoveFromColumnToEmitter(this.data.brickData);
            element.dataContainer.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
            element.ShowCurrentState();
            return;
        }
    }

    private void ShowOutOfSpace()
    {
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
    }
}