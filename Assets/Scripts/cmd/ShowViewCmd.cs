using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowViewCmd
{

    public ShowViewCmd(ViewName viewName)
    {
        this.viewName = viewName;
    }

    private readonly ViewName viewName;


    public void Run()
    {
        ViewModel.Instance.ShowView(viewName);
    }
}
