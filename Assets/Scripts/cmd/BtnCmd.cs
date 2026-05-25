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
        if (action == BtnAction.RefillAttempts)
        {
            var full = playerModel.playerData.attempts >= 5;
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
            playerModel.FillAttempts();
            Toast("Attempts refilled");

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
            var dataContainer = BalancingModel.Instance.GetDataCopy(cityElement.dataKey);
            cityElement.Setup(dataContainer, BalancingModel.AdditionalBricksOnEmptyElement);
            slotModel.Fill(dataContainer.slotElementDataList);
            return;
        }
    }

    private void Toast(string message)
    {
        new ToastCmd(message).Run();
    }
}