using UnityEngine;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    [SerializeField] Button nextBtn;
    [SerializeField] Button autoSolve;

    [SerializeField] Button moveCameraToElementGroupBtn;

    [SerializeField] Button completeCurrentGroupBtn;

    [SerializeField] Button rocketBtn;

    [SerializeField] Button completeElement;

    [SerializeField] Button cha1;

    void Awake()
    {
#if !UNITY_EDITOR
            GameObject.Destroy(this.gameObject);
             return;
#endif
    }

    void Start()
    {
        this.nextBtn.onClick.AddListener(OnNextBtnClicked);
        this.autoSolve.onClick.AddListener(OnAutoSolveClicked);
        this.moveCameraToElementGroupBtn.onClick.AddListener(OnMoveCameraToElementGroupClicked);
        this.completeCurrentGroupBtn.onClick.AddListener(OnCompleteCurrentGroupClicked);
        this.rocketBtn.onClick.AddListener(OnRocketBtnClicked);
        this.completeElement.onClick.AddListener(OnCompleteElementClicked);

        this.cha1.onClick.AddListener(() =>
        {
            new SwitchBuildingCmd().Run(BuildingName.Challenge_House_Cat01);
        });
    }

    private void OnRocketBtnClicked()
    {
        new FlyRocketCmd().Run();
    }

    private void OnCompleteElementClicked()
    {
        new CompleteElementCmd().Run(ModelUtils.GetCurrentElement());
    }

    public void OnMoveCameraToElementGroupClicked()
    {
        CamModel.Instance.MoveCameraToBuilding();
    }

    public void OnCompleteCurrentGroupClicked()
    {
        new CompleteCurrentBuildingCmd().Run();
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