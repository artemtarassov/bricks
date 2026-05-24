using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnAddSpaceAd : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.AddSpaceForAd);
    }
}