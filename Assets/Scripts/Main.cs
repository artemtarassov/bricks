using UnityEngine;

public class Main : MonoBehaviour
{
    void Awake()
    {
        new SetupCmd().Run();
    }

    //on Application Quit, save player data
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PlayerModel.Instance.Save();
        }
    }

    void OnApplicationQuit()
    {
        PlayerModel.Instance.Save();
    }

}