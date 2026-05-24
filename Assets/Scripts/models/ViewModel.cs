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
    LoadIapView
}

public class ViewData
{
    public ViewName viewName;
    public object data = null;
}

public class ViewModel
{
    public static ViewModel Instance;

    public List<EmitterBrick> Emitters = new List<EmitterBrick>();

    private List<ViewData> viewDataStack = new List<ViewData>();//only the last one is visible

    public Action<ViewName, bool> OnShowView;
    public Action<ViewName> OnHideView;

    public Transform root;
    public Action OnShake;
    public Action OnZoom;
    public Action<string, Vector2> OnShowParticles;
    public bool SetupCompleted { get; private set; } = false;
    public Action<ViewData, bool> OnViewChange;

    public Action OnSetupCompleted;

    public Action OnRequestPushNotificationsPermission;

    public Action OnShowPushNotificationsSettings;

    private float UILockedTime;

    public ViewModel()
    {
        UILockedTime = 0;
    }

    public List<ViewData> GetViews()
    {
        return viewDataStack;
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

    private HashSet<ViewName> activeViews = new HashSet<ViewName>();

    public void ShowView(ViewName viewName, bool animate = true)
    {
        Debug.Log($"ViewModel: Showing view {viewName}, animate {animate}");    
        activeViews.Add(viewName);
        viewDataStack.Add(new ViewData { viewName = viewName });
        OnShowView?.Invoke(viewName, animate);
    }

    public void HideView(ViewName viewName = ViewName.None)
    {
        Debug.Log($"ViewModel: Hiding view {viewName}");
        activeViews.Remove(viewName);
        viewDataStack.RemoveAll(v => v.viewName == viewName);
        OnHideView?.Invoke(viewName);
    }

    public bool HasView(ViewName viewName)
    {
        return activeViews.Contains(viewName);
    }

    public bool HasAnyView()
    {
        return activeViews.Count > 0;
    }




}