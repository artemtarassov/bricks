using UnityEngine;

public class MainView : MonoBehaviour
{
    void Awake()
    {
        new SetupCmd().Run();   
    }
}