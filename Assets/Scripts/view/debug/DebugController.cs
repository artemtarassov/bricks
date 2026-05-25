using UnityEngine;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    [SerializeField] Button nextBtn;

    void Start()
    {
        this.nextBtn.onClick.AddListener(OnNextBtnClicked);
    }

    public void OnNextBtnClicked()
    {
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
        //new UnlockCityElementCmd().Run();
    }
}