using TMPro;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnAddSpaceIAP : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.AddSpaceForIAP);
    }

    void OnEnable()
    {
        var txt = this.GetComponentInChildren<TMP_Text>();
        txt.text = "+";
    }
}