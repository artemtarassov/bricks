using UnityEngine;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    [SerializeField] Button nextBtn;
    [SerializeField] Button autoSolve;

    void Start()
    {
        this.nextBtn.onClick.AddListener(OnNextBtnClicked);
    }

    public void OnNextBtnClicked()
    {
        new FinishCurrentCityElementCmd().Run();
        //PlayerModel.Instance.playerData.currentElement.brickDataList.ForEach((bd) => bd.SetAllFull());
        //PlayerModel.Instance.playerData.currentElement.columns.ForEach((bd) => bd.SetAllFull());
        //new UnlockNextCmd().Run();
    }
}