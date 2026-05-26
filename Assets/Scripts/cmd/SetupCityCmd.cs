using System.Collections.Generic;
using UnityEngine.Assertions;

public class SetupCityCmd
{
    private CityModel cityModel => CityModel.Instance;
    private BalancingModel balancingModel => BalancingModel.Instance;

    public void Run(List<CityElementGroup> groups)
    {
        Assert.IsTrue(groups.Count > 0, "SetupCityCmd: groups list should not be empty");
        var pd = PlayerModel.Instance.playerData;
        var currentGroup = pd.currentGroup;
        if (currentGroup == null)
        {
            var firstGroup = groups[0].GroupName;
            cityModel.SetGroups(groups, firstGroup);
            var groupData = balancingModel.GetDataCopy(firstGroup);
            pd.currentGroup = groupData;
            Assert.IsNotNull(groupData, $"SetupCityCmd: groupData should not be null for group {firstGroup}");
        }
        else
        {
            cityModel.SetGroups(groups, currentGroup.groupName);
        }

        new UnlockCityElementCmd().Run();
    }
}