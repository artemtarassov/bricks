using System.Linq;
using UnityEngine;

public class SetupCmd
{
    public void Run(Transform root)
    {
        //set fps to 60
        Application.targetFrameRate = 60;

#if UNITY_EDITOR
        FilePrefs.DeleteAll(); //for testing only, remove in production
#endif

        PlayerModel.Instance = new PlayerModel();
        PlayerModel.Instance.Load();

        RemoteConfigModel.Instance = new RemoteConfigModel();
        CamModel.Instance = new CamModel();

        IAPModel.Instance = new IAPModel();
        IAPModel.Instance.Load();

        AdModel.Instance = new AdModel();
        AdModel.Instance.Load();

        SoundModel.Instance = new SoundModel();
        CityModel.Instance = new CityModel();
        SlotModel.Instance = new SlotModel();
        ViewModel.Instance = new ViewModel(root);

        BalancingModel.Instance = new BalancingModel();
        BalancingModel.Instance.Load();

        var cityElementGroups = root.GetComponentsInChildren<CityElementGroup>(true).ToList();
        new SetupCityCmd().Run(cityElementGroups);

        new EnsureSolveableCmd().Run();


        new InitServicesCmd().Run(root);

    }
}