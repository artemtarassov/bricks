using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport; 
#endif

public class ConsentStartupCmd 
{
    public void Run(Action callback)
    {
#if UNITY_IOS
#if GOOGLE_ADS
        new ConsentRequest().Run(() =>
        {
            new ATTRequest().Run(
                (bool granted) =>
                {
                    callback?.Invoke();
                }
            );
        });
#else
        new ATTRequest().Run(
            (bool granted) =>
            {
                callback?.Invoke();
            }
        );
#endif
#endif

#if UNITY_ANDROID
#if GOOGLE_ADS
        new ConsentRequest().Run(() =>
        {
            callback?.Invoke();
        });
#else
        callback?.Invoke();
#endif
#endif
    }
}

#if UNITY_IOS
class ATTRequest
{
    public void Run(Action<bool> callback)
    {
        var state = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        if (
            state == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED
            || state == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED
        )
        {
            callback?.Invoke(
                state == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED
            );
            callback = null;
            return;
        }
        ViewModel
            .Instance.root.GetComponent<MonoBehaviour>()
            .StartCoroutine(
                RequestAuthorization(
                    (bool granted) =>
                    {
                        callback?.Invoke(granted);
                        callback = null;
                    }
                )
            );
    }

    private IEnumerator RequestAuthorization(Action<bool> callback)
    {
        var secTimeout = 10;
        //yield return new WaitForSeconds(2);
        ATTrackingStatusBinding.RequestAuthorizationTracking();
        while (secTimeout > 0)
        {
            var state = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            //Debug.Log("IosConsentRequest.RequestAuthorization state: " + state + ", timeout " + secTimeout);
            if (state == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
            {
                break;
            }
            if (state == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED)
            {
                break;
            }
            yield return new WaitForSeconds(1);
            secTimeout--;
        }
        ;
        callback?.Invoke(
            ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
                == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED
        );
    }
}
#endif


#if GOOGLE_ADS
class ConsentRequest
{
    //https://www.youtube.com/watch?v=SysASyh9XKo
    private Action callback;

    public void Run(Action callback)
    {
        this.callback = callback;
        GoogleMobileAds.Ump.Api.ConsentRequestParameters request =
            new GoogleMobileAds.Ump.Api.ConsentRequestParameters();
        GoogleMobileAds.Ump.Api.ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void ExecuteCallback()
    {
        if (this.callback != null)
        {
            var c = this.callback;
            this.callback = null;
            DOVirtual.DelayedCall(0.1f, c.Invoke);
        }
    }

    void OnConsentInfoUpdated(GoogleMobileAds.Ump.Api.FormError consentError)
    {
        if (consentError != null)
        {
            Debug.LogWarning(consentError.Message);
            this.ExecuteCallback();
            return;
        }
        GoogleMobileAds.Ump.Api.ConsentForm.LoadAndShowConsentFormIfRequired(
            (GoogleMobileAds.Ump.Api.FormError e) =>
            {
                if (e != null)
                {
                    Debug.LogWarning(e);
                }
                this.ExecuteCallback();
            }
        );
    }
}
#endif
