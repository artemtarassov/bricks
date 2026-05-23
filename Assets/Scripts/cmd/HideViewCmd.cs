using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideViewCmd
{
    public void Run()
    {
        var views = ViewModel.Instance.GetViews().ToList();
        Debug.Log($"HideViewCmd: Hiding {views.Count} views.");
        foreach (var view in views)
        {
            ViewModel.Instance.HideView(view.viewName);
        }
    }

    public void Run(ViewName viewName)
    {
        if (!ViewModel.Instance.HasView(viewName))
        {
            return;
        }
        Debug.Log($"HideViewCmd: Hiding view {viewName}");
        ViewModel.Instance.HideView(viewName);
    }
}
