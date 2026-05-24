using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnRefill : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRefillClicked);
    }

    private void OnRefillClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.Refill);
    }
}