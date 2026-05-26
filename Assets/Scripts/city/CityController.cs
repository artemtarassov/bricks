using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CityController : MonoBehaviour
{
    [SerializeField] private GameObject flyingBrickPrefab;


    void Start()
    {
        CityModel.Instance.OnFlyBrick += OnFlyBrick;
        var cityElementGroups = GetComponentsInChildren<CityElementGroup>(true).ToList();
        new SetupCityCmd().Run(cityElementGroups);
        this.gameObject.AddComponent<BrickEmissionController>();
    }

    void OnDestroy()
    {
        CityModel.Instance.OnFlyBrick -= OnFlyBrick;
    }

    private void OnFlyBrick(FlyData data)
    {
        var t = Durations.FlyBrickDuration;
        var go = Instantiate(flyingBrickPrefab, data.from, Quaternion.identity);
        go.transform.SetParent(this.transform);
        go.GetComponent<Renderer>().material.color = ColoredMaterials.Instance.GetColorByColorIndex(data.colorIndex);
        go.transform.DOMove(data.targetBrick.position, t).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(go);
        });
    }


    private void GenerateCity()
    {

    }
}