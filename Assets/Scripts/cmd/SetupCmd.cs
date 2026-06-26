using System.Linq;
using UnityEngine;

public class SetupCmd
{
        public void Run(Transform root)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.filterLogType = LogType.Error | LogType.Exception | LogType.Assert;
#endif

#if UNITY_EDITOR
                FilePrefs.DeleteAll(); //for testing only, remove in production
#endif

                PlayerModel.Instance = new PlayerModel();
                PlayerModel.Instance.Load();

#if UNITY_EDITOR
                PlayerModel.Instance.EnableSetting(SettingsKey.Music, false);
#endif

                RemoteConfigModel.Instance = new RemoteConfigModel();
                ChallengeModel.Instance = new ChallengeModel();
                ChallengeModel.Instance.Load();
                
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

                var cityElementGroups = root.GetComponentsInChildren<BuildingElement>(true).ToList();
                new SetupCityCmd().Run(cityElementGroups);

#if !UNITY_EDITOR
                new InitServicesCmd().Run(root);

                root.gameObject.AddComponent<MobilePerformanceController>();
#endif
        }



}



