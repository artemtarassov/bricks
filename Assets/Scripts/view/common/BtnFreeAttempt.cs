using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnFreeAttempt : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnFreeAttemptClicked);
    }

    private void OnFreeAttemptClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.FreeAttemptForAd);
    }
}