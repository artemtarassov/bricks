using UnityEngine.Assertions;

public class BtnCmd
{

    public enum BtnAction
    {
        Restart,
        RefillAttempts,
        FreeAttemptForAd,
        AddSpaceForAd,
        AddSpaceForIAP,
        ContinueNextAttempt
    }

    private PlayerModel playerModel => PlayerModel.Instance;
    private CityModel cityModel => CityModel.Instance;
    private SlotModel slotModel => SlotModel.Instance;
    private RemoteConfigData remoteConfig => RemoteConfigModel.Instance.RemoteConfig;

    public void Run(BtnAction action)
    {
        if (action == BtnAction.Restart)
        {
            playerModel.playerData.currentElement = null;
            cityModel.DeactivateAllElements();
            new UnlockNextCmd().Run();
            return;
        }
        if (action == BtnAction.RefillAttempts)
        {
            var max = RemoteConfigModel.Instance.RemoteConfig.MaxAttempts;
            var full = playerModel.playerData.attempts >= max;
            if (full)
            {
                Toast("Attempts are already full");
                return;
            }
            var costs = remoteConfig.RefillCoins;
            if (!playerModel.CanAfford(costs))
            {
                Toast("Not enough coins");
                return;
            }
            new AddCoinsCmd(-costs).Run();
            playerModel.FillAttempts(max, max);
            return;
        }
        if (action == BtnAction.ContinueNextAttempt)
        {
            var didUse = playerModel.UseAttempt();
            if (!didUse)
            {
                Toast("No attempts left");
                return;
            }
            var pd = playerModel.playerData;
            var currentGroup = pd.currentGroupName;
            var currentElement = pd.currentElement;
            currentElement = BalancingModel.Instance.GetDataCopy(currentGroup, currentElement.dataKey);
            pd.currentElement = currentElement;
            pd.isDirty = true;
       
            var cityElement = ModelUtils.GetCurrentElement();
            Assert.AreEqual(cityElement.dataKey, currentElement.dataKey, "BtnCmd: ContinueNextAttempt: current city element data key should match player data current element data key");
            
            cityElement.Setup(currentElement);
            currentElement.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);

            cityElement.ShowCurrentState();
            slotModel.Fill(currentElement.columns);
            ViewModel.Instance.OutOfSpaceSeconds = 0;
            return;
        }

        if (action == BtnAction.FreeAttemptForAd)
        {
            new ShowAdCmd().Run(RewardName.ADD_ATTEMPT);
            return;
        }
    }

    private void Toast(string message)
    {
        new ToastCmd(message).Run();
    }
}