
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
        if (this.data.type == SlotElementType.Coins)
        {
            
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
            Assert.IsTrue(hasEmitterSpace, "No space for new emitter");

            SlotModel.Instance.MoveFromColumnToEmitter(this.data.brickData);
            return;
        }
        if (this.data.type == SlotElementType.AddMoreBricks)
        {
            SlotModel.Instance.MoveFromColumnToEmitter(this.data.brickData);
            element.dataContainer.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
            element.ShowBrickStates();
            return;
        }
    }

    private void ShowOutOfSpace()
    {
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
    }
}