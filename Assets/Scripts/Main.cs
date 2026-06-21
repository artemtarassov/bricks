using DG.Tweening;
using UnityEngine;
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Main : MonoBehaviour
{
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
    private InputAction androidBackAction;
#endif

    void Awake()
    {
        new SetupCmd().Run(this.transform);
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
        InitAndroidBackAction();
#endif
    }

    void Start()
    {
        DOTween.Sequence(this).AppendInterval(1).AppendCallback(OnSecUpdate).SetLoops(-1);
    }

    void OnDisable()
    {
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
        androidBackAction?.Disable();
#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
        if (androidBackAction == null)
        {
            return;
        }

        androidBackAction.performed -= OnAndroidBackPerformed;
        androidBackAction.Dispose();
        androidBackAction = null;
#endif
    }

    void OnEnable()
    {
#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
        androidBackAction?.Enable();
#endif
    }

    void OnApplicationQuit()
    {
        Debug.Log("Main OnApplicationQuit: killing all tweens");
        PlayerModel.Instance.Save();
        DOTween.KillAll();
    }

    private void OnSecUpdate()
    {
        new SecUpdateCmd().Run();
    }

    //on Application Quit, save player data
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PlayerModel.Instance.Save();
        }
    }

#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
    private void InitAndroidBackAction()
    {
        androidBackAction = new InputAction(
            name: "AndroidBack",
            type: InputActionType.Button,
            binding: "<Keyboard>/escape");
        androidBackAction.performed += OnAndroidBackPerformed;
    }

    private void OnAndroidBackPerformed(InputAction.CallbackContext _)
    {
        new GoBackBtnCmd().Run();
    }
#endif
}
