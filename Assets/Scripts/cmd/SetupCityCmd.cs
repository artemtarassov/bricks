using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class SetupCityCmd
{
    private CityModel cityModel => CityModel.Instance;

    public void Run(List<CityElementGroup> groups)
    {
        Assert.IsTrue(groups.Count > 0, "SetupCityCmd: groups list should not be empty");
        var pd = PlayerModel.Instance.playerData;

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            var groupProgressData = pd.progress.Find(g => g.groupName == group.GroupName);
            if (groupProgressData == null)
            {
                groupProgressData = new GroupProgressData() { groupName = group.GroupName, state = i == 0 ? GroupState.Unlocked : GroupState.Locked };
                pd.progress.Add(groupProgressData);
               // #if UNITY_EDITOR
                    groupProgressData.state = GroupState.Unlocked; //unlock all groups in editor for testing
               // #endif
            }
        }


        if (string.IsNullOrEmpty(pd.currentGroupName))
        {
            var firstGroup = groups[0].GroupName;
            cityModel.SetGroups(groups, firstGroup);
            pd.SetCurrentGroup(firstGroup);
        }
        else
        {
            cityModel.SetGroups(groups, pd.currentGroupName);
        }

        new SwitchGroupCmd().Run(0);//load current group, this will also move the camera to the group.

#if UNITY_EDITOR    
        new ValidateDataCmd().Run();
#endif
    }
}