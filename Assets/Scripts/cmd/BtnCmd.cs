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
            playerModel.AddCoins(-costs);
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
            var cityElement = cityModel.GetCurrentElement();
            var dataContainer = cityElement.dataContainer;
            dataContainer.Reset();

            cityElement.ShowBrickStates();
            slotModel.Fill(dataContainer.slotElementDataList);
            ViewModel.Instance.OutOfSpaceFlag = false;
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