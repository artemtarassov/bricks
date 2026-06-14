using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class SetupCityCmd
{
    private CityModel cityModel => CityModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;

    public void Run(List<BuildingElement> buildings)
    {
        Assert.IsTrue(buildings.Count > 0, "SetupCityCmd: buildings list should not be empty");
        var pd = PlayerModel.Instance.playerData;

        for (var i = 0; i < buildings.Count; i++)
        {
            var building = buildings[i];
            var progress = pd.GetBuildingProgressByName(building.BuildingName);
            if (progress == null)
            {
                progress = new BuildingProgressData(building.BuildingName, i == 0 ? BuildingState.Unlocked : BuildingState.Locked);
                pd.Progress.Add(progress);
            }
        }

        if (pd.CurrentBuildingName == BuildingName.Undefined)
        {
            var firstBuilding = buildings[0].BuildingName;
            cityModel.SetBuildings(buildings, firstBuilding);
            playerModel.SetCurrentBuilding(firstBuilding, BuildingState.Unlocked);
        }
        else
        {
            cityModel.SetBuildings(buildings, pd.CurrentBuildingName);
        }
        new EnsureSolveableCmd().Run();
        new SwitchBuildingCmd().Run(0);

#if UNITY_EDITOR    
        new ValidateDataCmd().Run();
#endif
    }
}