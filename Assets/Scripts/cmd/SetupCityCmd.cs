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
                #if UNITY_EDITOR
                    groupProgressData.state = GroupState.Unlocked; //unlock all groups in editor for testing
                #endif
            }
        }


        if (string.IsNullOrEmpty(pd.currentGroupName))
        {
            var firstGroup = groups[0].GroupName;
            cityModel.SetGroups(groups, firstGroup);
            pd.currentGroupName = firstGroup;
        }
        else
        {
            cityModel.SetGroups(groups, pd.currentGroupName);
        }

        /*var cityElementData = pd.currentElement;
        if (cityElementData != null)
        {
            var group = cityModel.GetGroupByName(pd.currentGroupName);
            var elements = group.GetElements().ToList();
            var currentElementIndex = elements.FindIndex((e) => e.dataKey == cityElementData.dataKey);
            cityModel.ActivateElements(currentElementIndex);

            var allCompleted = pd.currentElement.AllSlotsEmpty() && pd.currentElement.ElementCompleted();
            if (allCompleted)
            {
                //new UnlockNextCmd().Run();
            }
            else
            {

                if (cityElementData.ElementCountColoredBricks() == 0)
                    cityElementData.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
                var cityElement = cityModel.GetElementByDataKey(cityElementData.dataKey);
                cityElement.Setup(cityElementData);
                //SlotModel.Instance.Fill(cityElementData.columns);
                //CamModel.Instance.MoveCameraToCityElement(cityElement);
            }
            return;
        }*/

        //CamModel.Instance.MoveCameraToElementGroup();

        new SwitchGroupCmd().Run(0);//load current group, this will also move the camera to the group.

        //new UnlockNextCmd().Run();

#if UNITY_EDITOR    
        new ValidateDataCmd().Run();
#endif
    }
}