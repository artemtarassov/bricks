
using UnityEngine.Assertions;

public class SetNextBricksColorsCmd
{

    private CityElement cityElement;
    public SetNextBricksColorsCmd(CityElement cityElement)
    {
        this.cityElement = cityElement;
    }

    public void Run()
    {
        var maxDifferentColors = 3;
        var list = this.cityElement.cityElementColors.NextColorIndexList(maxDifferentColors);
        Assert.IsTrue(list.Count > 0, "There should be at least one color index in the list");

        for (var i = 0; i < list.Count; i++)
        {
            this.cityElement.SetBrickColors(list[i]);
        }
    }
}