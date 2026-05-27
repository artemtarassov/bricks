using System.Linq;
using UnityEngine;


public class FlyCoinsAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject flyCoinPrefab;
    [SerializeField] private Transform target;

    void Start()
    {
        ViewModel.Instance.OnFlyCoin += OnFlyCoin;
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnFlyCoin -= OnFlyCoin;
    }

    private void OnFlyCoin(Vector3 srcPos)
    {
        var targetPos = target.position;
    }
}