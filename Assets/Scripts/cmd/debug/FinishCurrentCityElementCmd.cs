
public class FinishCurrentCityElementCmd

{
    public void Run()
    {
        var cityElement = ModelUtils.GetCurrentElement();
        var isCompleted = cityElement.dataContainer.ElementCompleted();
        if (isCompleted)
        {
            new UnlockNextCmd().Run();
            return;
        }
        foreach (var c in cityElement.dataContainer.columns)
        {
            foreach (var ci in c.list)
            {
                ci.BrickData = null;
                ci.type = SlotElementType.Undefined;
            }
        }

        foreach (var bd in cityElement.dataContainer.brickDataList)
        {
            bd.SetAll(BrickState.Full);
        }
        cityElement.ShowCurrentState();

    }
}