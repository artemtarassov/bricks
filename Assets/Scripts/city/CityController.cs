using System.Linq;
using UnityEngine;

public class CityController : MonoBehaviour
{
    [SerializeField] private GameObject flyingBrickPrefab;

    private FlyingBricks2 flyingBricks;


    void Awake()
    {
//        var cityElementGroups = GetComponentsInChildren<CityElementGroup>(true).ToList();
//        new SetupCityCmd().Run(cityElementGroups);
    }
    
    void Start()
    {
        this.flyingBricks = new FlyingBricks2(this.flyingBrickPrefab, this.transform);
        CityModel.Instance.OnFlyBrick += OnFlyBrick;
        this.gameObject.AddComponent<BrickEmissionController>();
    }

    void OnDestroy()
    {
        CityModel.Instance.OnFlyBrick -= OnFlyBrick;
        this.flyingBricks?.Dispose();
    }

    private void OnFlyBrick(FlyBrickData data)
    {
        var otherBricks = ModelUtils.GetCurrentElement().GetBrickLayersContainer().sortedBricks;
        this.flyingBricks.Fly(otherBricks, data);
    }
}
