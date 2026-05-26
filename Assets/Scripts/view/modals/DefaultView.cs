using UnityEngine;

public class DefaultView : MonoBehaviour
{
 
    protected void PlayOpenPopupSound()
    {
        new SoundCmd("popupOpen",0.3f).Run();
    }
    protected void PlayClosePopupSound()
    {
        new SoundCmd("popupClose",0.5f).Run();
    }

    public virtual void OnHidden()
    {
        Debug.Log("DefaultView OnHidden called");
    }

    public virtual void OnShown()
    {
        Debug.Log("DefaultView OnShown called");
    }

    public virtual void OnBackgroundTap()
    {
        Debug.Log("DefaultView OnBackgroundTap called");
    }


}