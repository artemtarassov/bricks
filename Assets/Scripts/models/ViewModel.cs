using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public enum ViewName
{
    None,
    AddSpaceView,
    LoadingView,
    OutOfSpaceView,
    SettingsView,
    GoldenTicketView,
    CompleteView
}


public enum BottomNav
{
    None,
    Slots,
    MainNav,
    FinishElement,
}



public class ViewData
{
    public ViewName viewName;
    public object data = null;

    public ViewData(ViewName viewName, object data = null)
    {
        this.viewName = viewName;
        this.data = data;
    }
}

public enum FadeType
{
    In,
    Out,
    Flash
}

public class ViewModel
{
    public static ViewModel Instance;

    public List<Transform> Emitters = new List<Transform>();

    private List<ViewData> viewDataStack = new List<ViewData>();//only the last one is visible

    public Action<ViewName> OnShowView;
    public Action<ViewName> OnHideView;

    public Transform root;
    public Action OnShake;
    public Action<string, Vector2> OnShowParticles;
    public bool SetupCompleted { get; private set; } = false;

    public Action<string> OnToastMsg;

    public Action OnSetupCompleted;

    public Action OnRequestPushNotificationsPermission;

    public Action OnShowPushNotificationsSettings;

    private float UILockedTime;

    public int OutOfSpaceCounter { get; private set; } = 0;

    public Action<Vector3, int, int> OnFlyCoin;

    public Action<Vector3, Vector3> OnFlyRocket;


    public Action OnAndroidReviewRequest;

    public Action<FadeType> OnFade;

    public Action<BottomNav> OnBottomNavChange;

    public BottomNav CurrentBottomNav { get; private set; } = BottomNav.None;

    public int GoldenTicketViewTriggerTimestamp = 0;

    public Action<float> OnFinishElement;


    public Action OnAnnouncer;


    public ViewModel(Transform root)
    {
        this.root = root;
        this.CurrentBottomNav = BottomNav.MainNav;
    }

    public void PlayAnnouncer()
    {
        OnAnnouncer?.Invoke();
    }


    public void FinishPercent(float percent)
    {
        OnFinishElement?.Invoke(percent);
    }
  

    public void ChangeBottomNav(BottomNav nav)
    {
        if (CurrentBottomNav == nav)
        {
            return;
        }
        CurrentBottomNav = nav;
        OnBottomNavChange?.Invoke(nav);
    }

    public void Fade(FadeType fadeType)
    {
        OnFade?.Invoke(fadeType);
    }

    public void FlyRocket(Vector3 fromFlyPos, Vector3 toFlyPos)
    {
        OnFlyRocket?.Invoke(fromFlyPos, toFlyPos);
    }

    public void FlyCoin(Vector3 fromFlyPos, int fromAmount, int toAmount)
    {
        OnFlyCoin?.Invoke(fromFlyPos, fromAmount, toAmount);
    }

    public List<ViewData> GetViews()
    {
        return viewDataStack;
    }

    public void ShowToast(string msg)
    {
        OnToastMsg?.Invoke(msg);
    }

    public void LockUI(float sec)
    {
        var timeout = Time.time + sec;
        if (UILockedTime < timeout)
        {
            UILockedTime = timeout;
        }
    }

    public bool IsUILocked()
    {
        var locked = Time.time < UILockedTime;
        return locked;
    }

    public void Shake()
    {
        OnShake?.Invoke();
    }

    public void CompleteSetup()
    {
        this.SetupCompleted = true;
        OnSetupCompleted?.Invoke();
    }

    public void ShowParticles(string name, Vector2 pos)
    {
        OnShowParticles?.Invoke(name, pos);
    }
    public void ShowView(ViewName viewName)
    {
        var existingView = viewDataStack.FirstOrDefault(v => v.viewName == viewName);
        if (existingView == null)
        {
            viewDataStack.Add(new ViewData(viewName));
        }
        OnShowView?.Invoke(viewName);
    }

    public void HideView(ViewName viewName = ViewName.None)
    {
        viewDataStack.RemoveAll(v => v.viewName == viewName);
        OnHideView?.Invoke(viewName);
    }

    public bool HasView(ViewName viewName)
    {
        return viewDataStack.Any(v => v.viewName == viewName);
    }

    public bool HasAnyView()
    {
        return viewDataStack.Count > 0;
    }


    public void IncOutOfSpaceCounter()
    {
        this.OutOfSpaceCounter++;
    }
    public void ResetOutOfSpaceCounter()
    {
        this.OutOfSpaceCounter = 0;
    }
    public void DisableOutOfSpaceCounter()
    {
        this.OutOfSpaceCounter = int.MinValue;
    }

}