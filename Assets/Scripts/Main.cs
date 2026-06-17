using DG.Tweening;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Awake()
    {
        new SetupCmd().Run(this.transform);
    }

    void Start()
    {
        DOTween.Sequence(this).AppendInterval(1).AppendCallback(OnSecUpdate).SetLoops(-1);
    }

    void OnDisable()
    {
       // throw new System.NotImplementedException();
    }

    void OnDestroy()
    {

    }

    void OnApplicationQuit()
    {
        Debug.Log("Main OnApplicationQuit: killing all tweens");
        PlayerModel.Instance.Save();
        DOTween.KillAll();
    }

    private void OnSecUpdate()
    {
        //new SecUpdateCmd().Run();
    }

    //on Application Quit, save player data
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PlayerModel.Instance.Save();
        }
    }



}