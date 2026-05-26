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

public class ViewModel
{
    public static ViewModel Instance;

    public List<Transform> Emitters = new List<Transform>();

    private List<ViewData> viewDataStack = new List<ViewData>();//only the last one is visible

    public Action<ViewName> OnShowView;
    public Action<ViewName> OnHideView;

    public Transform root;
    public Action OnShake;
    public Action OnZoom;
    public Action<string, Vector2> OnShowParticles;
    public bool SetupCompleted { get; private set; } = false;
    public Action<ViewData, bool> OnViewChange;

    public Action<string> OnToastMsg;

    public Action OnSetupCompleted;

    public Action OnRequestPushNotificationsPermission;

    public Action OnShowPushNotificationsSettings;

    private float UILockedTime;

    public bool OutOfSpaceFlag = false;

    public ViewModel()
    {
        UILockedTime = 0;
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

}