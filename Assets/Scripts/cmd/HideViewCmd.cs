using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideViewCmd
{
    private ViewName viewName;

    public HideViewCmd(ViewName viewName = ViewName.None)
    {
        this.viewName = viewName;
    }
    public void Run()
    {
        if (this.viewName == ViewName.None)
        {
            var views = ViewModel.Instance.GetViews().ToList();
            foreach (var view in views)
            {
                ViewModel.Instance.HideView(view.viewName);
            }
            return;
        }
        if (!ViewModel.Instance.HasView(viewName))
        {
            return;
        }
        ViewModel.Instance.HideView(viewName);
        var views2 = ViewModel.Instance.GetViews().ToList();
        if (views2.Count > 0)
        {
            ViewModel.Instance.ShowView(views2[0].viewName);
        }
    }
}
