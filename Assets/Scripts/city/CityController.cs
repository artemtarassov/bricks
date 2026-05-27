using System.Linq;
using UnityEngine;

public class CityController : MonoBehaviour
{
    [SerializeField] private GameObject flyingBrickPrefab;

    private FlyingBricks flyingBricks;

    void Start()
    {
        this.flyingBricks = new FlyingBricks(this.flyingBrickPrefab, this.transform);
        CityModel.Instance.OnFlyBrick += OnFlyBrick;
        var cityElementGroups = GetComponentsInChildren<CityElementGroup>(true).ToList();
        new SetupCityCmd().Run(cityElementGroups);
        this.gameObject.AddComponent<BrickEmissionController>();
    }

    void OnDestroy()
    {
        CityModel.Instance.OnFlyBrick -= OnFlyBrick;
        this.flyingBricks?.Dispose();
    }

    private void OnFlyBrick(FlyBrickData data)
    {
        this.flyingBricks.Fly(data);
    }
}
