using UnityEngine;
using Unity.Services.Core;
using UnityEngine.UnityConsent;
using Unity.Services.Core.Environments;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using System;
using Facebook.Unity;
using UnityEngine.Events;

public class InitServicesCmd
{
    public void Run(Transform root)
    {
        //init unity services
        root.gameObject.AddComponent<InitializeUnityServices>();
#if UNITY_IOS && !UNITY_EDITOR
        root.gameObject.AddComponent<AppleGameCenterController>();
#endif
        new InitFBCmd().Run();
        new InitIAPCmd().Run();
    }

}


class InitializeUnityServices : MonoBehaviour
{
    public string environment = "production";

    async void Start()
    {
        var options = new InitializationOptions().SetEnvironmentName(environment);

        while (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync(options);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("InitializeUnityServices.Run: " + e.Message);
            }
            //WaitForSeconds 10
            await System.Threading.Tasks.Task.Delay(10000);
        }
        EndUserConsent.SetConsentState(new ConsentState()
        {
            AdsIntent = ConsentStatus.Granted,
            AnalyticsIntent = ConsentStatus.Granted
        });

        try
        {
            await InitializeRemoteConfigAsync();
            // you can set the environment ID:
            RemoteConfigService.Instance.SetEnvironmentID(environment);

            // Fetch configuration settings from the remote service, they must be called with the attributes structs (empty or with custom attributes) to initiate the WebRequest.
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
            var r = await RemoteConfigService.Instance.FetchConfigsAsync(
                new userAttributes(),
                new appAttributes()
            );
        }
        catch (Exception e)
        {
            var state = UnityServices.State;
            Debug.LogWarning(
                "UnityServices.InitializeAsync (state=" + state + ") failed: " + e.Message + ", " + e.StackTrace
            );
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("InitializeUnityServices OnApplicationQuit");
    }

    void OnDestroy()
    {
        Debug.Log("InitializeUnityServices OnDestroy");
        RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteConfig;
    }

    async Task InitializeRemoteConfigAsync()
    {
        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void ApplyRemoteConfig(ConfigResponse response)
    {
        //Debug.Log("Remote config fetched with response: " + response.ToString());
        var ac = RemoteConfigService.Instance.appConfig;
        var allProperties = Enum.GetValues(typeof(RemoteConfigProperty));
        var remoteConfigPars = RemoteConfigModel.Instance.RemoteConfig;
        foreach (var property in allProperties)
        {
            var p = (RemoteConfigProperty)property;
            var propertyName = p.ToString();
            if (ac.HasKey(propertyName))
            {
                var remoteValue = ac.GetInt(propertyName);
                remoteConfigPars.SetValue(p, remoteValue);
            }
        }
        remoteConfigPars.Save();
        new ApplyRemoteConfigCmd().Run();
    }
}


public struct userAttributes
{
    // Optionally declare variables for any custom user attributes:
}

public struct appAttributes
{
    // Optionally declare variables for any custom app attributes:
    public string appVersion;
}





public class InitFBCmd
{
    private UnityAction callback;
    public void Run(UnityAction callback = null)
    {
        this.callback = callback;
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback);
        }
        else
        {
            FB.ActivateApp();
            this.callback?.Invoke();
        }
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            Debug.Log("InitFBCmd SDK is initialized");
            // Signal an app activation App Event     

            FB.ActivateApp();
            this.callback?.Invoke();
        }
        else
        {
            Debug.LogError("InitFBCmd Failed to Initialize the Facebook SDK");
        }
    }




}
