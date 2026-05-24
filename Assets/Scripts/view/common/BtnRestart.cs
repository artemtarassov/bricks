using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnRestart : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.Restart);
    }
}