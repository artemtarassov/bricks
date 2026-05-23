using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowViewCmd
{
    public void Run(ViewName viewName, object p = null)
    {
        ViewModel.Instance.ShowView(viewName, true);
    }
    public void Run(ViewData vd)
    {
        //ViewModel.Instance.ShowView(vd);
    }
}
