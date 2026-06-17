
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
        //Debug.Log($"SelectColumnCmd: type={this.data.type}, brickData={this.data.brickData}");


        if (ModelUtils.IsOutOfSpace())
        {
            this.ShowOutOfSpace();
            return;
        }

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


        if (this.data.type == SlotElementType.Bricks || this.data.type == SlotElementType.HiddenBricks)
        {
            var hasEmitterSpace = SlotModel.Instance.HasEmitterSpace();
            if (!hasEmitterSpace)
            {
                new SoundCmd(SoundModel.Instance.ERROR).Run();
                return;
            }
            SlotModel.Instance.MoveFromColumnToEmitter(this.data.brickData);
            return;
        }

        var element = ModelUtils.GetCurrentElement();
        if (this.data.type == SlotElementType.AddMoreBricks)
        {
            CityModel.Instance.EnableDifferentColors(element, BalancingModel.AdditionalBricksOnEmptyElement);
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            SlotModel.Instance.EmitterAlive();
            new SoundCmd(SoundModel.Instance.NEW_COLORED_BRICKS_APPEAR).Run();
            return;
        }

        if (this.data.type == SlotElementType.Ad)
        {
            if (AdModel.Instance.IsAdReady(RewardName.MID_SESSION_INTERSTITIAL))
                new ShowAdCmd().Run(RewardName.MID_SESSION_INTERSTITIAL);
            else
                if (AdModel.Instance.IsAdReady(RewardName.MID_SESSION_REWARDED))
                    new ShowAdCmd().Run(RewardName.MID_SESSION_REWARDED);

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
                new SoundCmd(SoundModel.Instance.ERROR).Run();
            }
            return;
        }

        if (this.data.type == SlotElementType.EmitterDeathWaiting)
        {
            SlotModel.Instance.Replace(this.data, SlotElementType.EmitterDeathActive); 
            SlotModel.Instance.EmitterDeath();
            new SoundCmd(SoundModel.Instance.CLICK2).Run();
            return;
        }


    }

    private void ShowOutOfSpace()
    {
        new ShowOutOfSpaceCmd().Run();
    }
}