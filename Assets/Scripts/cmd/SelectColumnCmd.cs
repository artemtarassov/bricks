
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
        var gameOverReason = ModelUtils.IsGameOver();
        if (gameOverReason == GameOverReason.OutOfSpace)
        {
            new SoundCmd(SoundModel.Instance.ERROR).Run();
            if (ViewModel.Instance.OutOfSpaceCounter != 0)
            {
                new ShowGameOverCmd().Run(gameOverReason);
            }
            return;
        }

        if (gameOverReason == GameOverReason.OutOfTime)
        {
            new SoundCmd(SoundModel.Instance.ERROR).Run();
            new ShowGameOverCmd().Run(gameOverReason);
            return;
        }

        

        if (PlayerModel.Instance.playerData.secondsPlaying < 60)
        {
            new LogEventCmd().Run("column_selected", "slotElementType", this.data.type.ToString());
        }

        PlayerModel.Instance.playerData.isDirty = true;

        if (this.data.type == SlotElementType.AddSecondsInChallenge)
        {
            PlayerModel.Instance.AddTimeoutSeconds(this.data.secondsToAdd);
            ViewModel.Instance.OnClockTimeIncreased?.Invoke();
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            new SoundCmd(SoundModel.Instance.CONFIRM).Run();
            return;
        }

        if (this.data.type == SlotElementType.UnlockChallenge)
        {
            ChallengeModel.Instance.UnlockChallenge(this.data.challenge);
            SlotModel.Instance.Replace(this.data, SlotElementType.Undefined);
            var msgText = "New challenge unlocked!";
            new ToastCmd(msgText).Run(this.data.challenge);
            new LogEventCmd().Run("challenge_unlocked", "buildingName", this.data.challenge.ToString());
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


        if (this.data.IsBrick)
        {
            var hasEmitterSpace = SlotModel.Instance.HasEmitterSpace();
            if (!hasEmitterSpace)
            {
                new SoundCmd(SoundModel.Instance.ERROR).Run();
                return;
            }
            SlotModel.Instance.MoveFromColumnToEmitter(this.data.BrickData);
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
            var dc = this.data.deadCounter;
            SlotModel.Instance.EmitterDeath(this.data.deadCounter);
            var newSed = new SlotElementData(SlotElementType.EmitterDeathActive);
            newSed.deadCounter = dc;
            SlotModel.Instance.Replace(this.data, newSed);
            new SoundCmd(SoundModel.Instance.CLICK2).Run();
            return;
        }


    }

}