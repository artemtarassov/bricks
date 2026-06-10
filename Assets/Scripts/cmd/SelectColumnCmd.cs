
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectColumnCmd
{
    private SlotElementData data;
    private SlotElement element;

    public SelectColumnCmd(int columnIndex, SlotElement e = null)
    {
        var elementData = SlotModel.Instance.GetNextSlotElementDataInColumn(columnIndex);
        Assert.IsNotNull(elementData, $"SelectColumnCmd: No element found in column {columnIndex}");
        this.data = elementData;
        this.element = e;
    }

    public void Run()
    {
        Debug.Log($"SelectColumnCmd: type={this.data.type}, brickData={this.data.brickData}");
        if (this.data.type == SlotElementType.Coins)
        {
            var nCoins = RemoteConfigModel.Instance.RemoteConfig.ColumnCoins;
            if (this.element == null)
            {
                new AddCoinsCmd(nCoins).Run();
            }
            else
            {
                new AddCoinsCmd(nCoins, this.element.coins.transform.position).Run();
            }
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }
        var element = ModelUtils.GetCurrentElement();

        if (this.data.type == SlotElementType.Bricks || this.data.type == SlotElementType.HiddenBricks)
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
            element.dataContainer.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
            element.ShowCurrentState();
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }

        if (this.data.type == SlotElementType.Ad)
        {
            if (AdModel.Instance.IsAdReady(RewardName.INTERSTITIAL))
                new ShowAdCmd().Run(RewardName.INTERSTITIAL);
            else Debug.Log("SelectColumnCmd: ad reward selected but ad is not ready");
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            return;
        }

        if (this.data.type == SlotElementType.FinalExplosion)
        {
            if (element.dataContainer.ElementCompleted() && element.HasVisuals() == false)
            {
                new FlyRocketCmd().Run();
                SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            }
            else
            {
                Debug.Log("SelectColumnCmd: explosion column selected but current group is not completed, not flying rocket");
            }
            return;
        }


        if (!SlotModel.Instance.HasEmitterSpace() && element.dataContainer.ElementCountEmittingBricks() == 0)
        {
            this.ShowOutOfSpace();
            return;
        }

    }

    private void ShowOutOfSpace()
    {
        new ShowViewCmd(ViewName.OutOfSpaceView).Run();
    }
}