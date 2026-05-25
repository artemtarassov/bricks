
using UnityEngine.Assertions;

public class SelectColumnCmd
{
    private SlotElementData data;

    public SelectColumnCmd(SlotElementData data)
    {
        this.data = data;
    }

    public void Run()
    {
        var element = CityModel.Instance.GetCurrentElement();
        if (!SlotModel.Instance.HasEmitterSpace() && element.CountFlyingBricks() == 0)
        {
            this.ShowOutOfSpace();
            return;
        }

        if (this.data.type == SlotElementType.Bricks)
        {
            var hasEmitterSpace = SlotModel.Instance.HasEmitterSpace();
            Assert.IsTrue(hasEmitterSpace, "No space for new emitter");

            SlotModel.Instance.MoveFromColumnToEmitter(this.data);
            return;
        }
        if (this.data.type == SlotElementType.AddMoreBricks)
        {
            SlotModel.Instance.MoveFromColumnToEmitter(this.data);
            CityModel.Instance.GetCurrentElement().ShowNextColoredBricks(BalancingModel.AdditionalBricksOnEmptyElement);
            return;
        }
    }

    private void ShowOutOfSpace()
    {
        var attemptsLeft = PlayerModel.Instance.playerData.attempts;
        if (attemptsLeft > 0)
        {
            new ShowViewCmd().Run(ViewName.OutOfSpaceView);
        }
        else
        {
            new ShowViewCmd().Run(ViewName.GameOverView);
        }
    }
}