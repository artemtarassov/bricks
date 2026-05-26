using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowViewCmd
{
    public void Run(ViewName viewName)
    {
        ViewModel.Instance.ShowView(viewName);
    }
}
