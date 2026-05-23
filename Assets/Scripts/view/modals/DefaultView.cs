using UnityEngine;

public class DefaultView : MonoBehaviour
{
    [SerializeField] protected GameObject bg;
    [SerializeField] protected GameObject contents;
 
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

    public virtual void OnShown(bool animate)
    {
        Debug.Log("DefaultView OnShown called");
    }




}