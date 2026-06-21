using UnityEngine;
using UnityEngine.iOS;

public class RateAppCmd
{
    public void Run()
    {
        OpenStore();
    }
    private void OpenStore()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            ViewModel.Instance.OnAndroidReviewRequest?.Invoke();
            //var appId = Application.identifier;
            //Application.OpenURL("https://play.google.com/store/apps/details?id=" + appId);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            bool popupShown = UnityEngine.iOS.Device.RequestStoreReview();
            if (popupShown)
            {
                //
            }
            else
            {
                var appId = "6779683450";
                var appStoreUrl = "https://apps.apple.com/app/id" + appId;
                Application.OpenURL(appStoreUrl);
            }
#endif
        }
    }

}