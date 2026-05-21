using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BottomNavController : MonoBehaviour
{
    [SerializeField] private Button btn0;
    [SerializeField] private Button btn1;
    [SerializeField] private Button btn2;


    void Start()
    {
        this.btn0.GetComponent<HoldButton>().OnHold.AddListener(OnBtnClicked1);
        this.btn1.GetComponent<HoldButton>().OnHold.AddListener(OnBtnClicked2);
        this.btn2.GetComponent<HoldButton>().OnHold.AddListener(OnBtnClicked3);


        this.btn0.GetComponent<ColoredButton>().SetColor(ColorIndex.C0);
        this.btn1.GetComponent<ColoredButton>().SetColor(ColorIndex.C1);
        this.btn2.GetComponent<ColoredButton>().SetColor(ColorIndex.C2);
    }



    public void OnBtnClicked1()
    {
    }

    public void OnBtnClicked2()
    {
    }

    public void OnBtnClicked3()
    {
    }


}