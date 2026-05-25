using UnityEngine;

public class SetupCmd
{
    public void Run(Transform root)
    {
        FilePrefs.DeleteAll(); //for testing only, remove in production

        PlayerModel.Instance = new PlayerModel();
        PlayerModel.Instance.Load();

        IAPModel.Instance = new IAPModel();
        AdModel.Instance = new AdModel();

        CityModel.Instance = new CityModel();
        SlotModel.Instance = new SlotModel();
        ViewModel.Instance = new ViewModel();
        ViewModel.Instance.root = root;
        BalancingModel.Instance = new BalancingModel();
        BalancingModel.Instance.Load();


        new InitServicesCmd().Run(root);
    }
}