using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideViewCmd
{
    public void Run()
    {
        var views = ViewModel.Instance.GetViews().ToList();
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

        ViewModel.Instance.HideView(viewName);
        var views = ViewModel.Instance.GetViews().ToList();
        if (views.Count > 0)
        {
            ViewModel.Instance.ShowView(views[0].viewName);
        }
    }
}
