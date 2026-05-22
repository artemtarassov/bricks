
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
}