public class SetupCmd
{
    public void Run()
    {
        PlayerModel.Instance = new PlayerModel();
        CityModel.Instance = new CityModel();
        SlotModel.Instance = new SlotModel();
        ViewModel.Instance = new ViewModel();
        BalancingModel.Instance = new BalancingModel();
        BalancingModel.Instance.Load();

        PlayerModel.Instance.Load();
    }
}