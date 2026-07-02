using UnityEngine;

public class DefaultView : MonoBehaviour
{


    public virtual void OnHidden()
    {
        //Debug.Log("DefaultView OnHidden called");
    }

    public virtual void OnShown()
    {
        //Debug.Log("DefaultView OnShown called");
    }

    public virtual void OnBackgroundTap()
    {
        //Debug.Log("DefaultView OnBackgroundTap called");
    }



}