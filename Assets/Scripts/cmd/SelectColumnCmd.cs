
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
            Assert.IsTrue(SlotModel.Instance.HasEmitterSpace(), "No space for new emitter");
            var didRemove = SlotModel.Instance.RemoveFromColumn(this.data);
            Assert.IsTrue(didRemove, "Failed to remove element from column");
            SlotModel.Instance.AddEmitter(this.data.brickData);
            return;
        }
        if (this.data.type == SlotElementType.AddMoreBricks)
        {
            //tbd.
            var didRemove = SlotModel.Instance.RemoveFromColumn(this.data);
            Assert.IsTrue(didRemove, "Failed to remove element from column");
            new SetNextBricksColorsCmd(CityModel.Instance.GetCurrentElement()).Run();
            return;
        }
    }
}