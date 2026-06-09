using UnityEngine;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    [SerializeField] Button nextBtn;
    [SerializeField] Button autoSolve;

    [SerializeField] Button moveCameraToElementGroupBtn;

    [SerializeField] Button completeCurrentGroupBtn;

    [SerializeField] Button rocketBtn;

    void Start()
    {
        this.nextBtn.onClick.AddListener(OnNextBtnClicked);
        this.autoSolve.onClick.AddListener(OnAutoSolveClicked);
        this.moveCameraToElementGroupBtn.onClick.AddListener(OnMoveCameraToElementGroupClicked);
        this.completeCurrentGroupBtn.onClick.AddListener(OnCompleteCurrentGroupClicked);
        this.rocketBtn.onClick.AddListener(OnRocketBtnClicked);
    }

    private void OnRocketBtnClicked()
    {
        new FlyRocketCmd().Run();
    }

    public void OnMoveCameraToElementGroupClicked()
    {
        CamModel.Instance.MoveCameraToElementGroup();
    }

    public void OnCompleteCurrentGroupClicked()
    {
        new CompleteCurrentGroupCmd().Run();
    }
    public void OnNextBtnClicked()
    {
        new FinishCurrentCityElementCmd().Run();
        //PlayerModel.Instance.playerData.currentElement.brickDataList.ForEach((bd) => bd.SetAllFull());
        //PlayerModel.Instance.playerData.currentElement.columns.ForEach((bd) => bd.SetAllFull());
        //new UnlockNextCmd().Run();
    }

    public void OnAutoSolveClicked()
    {
        new AutoPlayCmd().Run();
    }
}