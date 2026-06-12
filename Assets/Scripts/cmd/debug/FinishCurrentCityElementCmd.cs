using System.Runtime.CompilerServices;

public class FinishCurrentCityElementCmd

{
    public void Run()
    {
        var cityElement = ModelUtils.GetCurrentElement();
        foreach (var c in cityElement.dataContainer.columns)
        {
            foreach (var ci in c.list)
            {
                ci.brickData = null;
                ci.type = SlotElementType.Undefined;
            }
        }

        foreach (var bd in cityElement.dataContainer.brickDataList)
        {
            bd.SetAll(BrickState.Full);
        }
        cityElement.ShowCurrentState();

        new UnlockNextCmd().Run();

        cityElement = ModelUtils.GetCurrentElement();
        foreach (var bd in cityElement.dataContainer.brickDataList)
        {
            bd.SetAll(BrickState.Colored);
        }
        cityElement.ShowCurrentState();

    }
}