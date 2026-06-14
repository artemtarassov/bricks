using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;


public class EmitBricksCmd
{
    private static CityElement GetCurrentElement()
    {
        return ModelUtils.GetCurrentElement();
    }
    public void Run()
    {
        var emitters = SlotModel.Instance.Emitters;
        var cityElement = GetCurrentElement();
        foreach (var emitter in emitters)
        {
            if (emitter.HasColoredBricks && cityElement.dataContainer.ElementCountColoredBricks(emitter.brickData.color) > 0)
            {
                new EmitBrickCmd(emitter).Run();
            }
        }
    }
}

public class EmitBrickCmd
{
    private CityElement cityElement;
    private CityElementDataContainer elementDataContainer => cityElement.dataContainer;
    private Transform nextBrick => elementBrickData.brickTransform;
    private ColorIndex colorIndex;
    private EmitterSpace emitter;
    private BrickData emitterBrickData;
    private ColoredBrickInfo elementBrickData;

    private int groupIndex;
    private BuildingProgressData progress;

    public EmitBrickCmd(EmitterSpace emitter)
    {
        Assert.IsTrue(emitter.HasColoredBricks);
        this.progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        this.cityElement = CityModel.Instance.GetElementByDataKey(progress.GetCurrentElement().dataKey);
        this.emitter = emitter;
        this.colorIndex = emitter.brickData.color;
        this.emitterBrickData = emitter.brickData;
        this.elementBrickData = cityElement.GetFurthestColoredBrick(this.colorIndex);

        Assert.IsNotNull(this.cityElement, "No current city element found");
        Assert.AreNotEqual(this.colorIndex, ColorIndex.Undefined, "Color index must be defined");
        this.groupIndex = ModelUtils.GetCurrentGroupIndex();
    }

    private Vector3 GetFromPos()
    {
        var screenPos = ViewModel.Instance.Emitters[emitter.index].position;
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 1));
        return worldPos;
    }

    public void Run()
    {
        PlayerModel.Instance.playerData.isDirty = true;
        //from pos is 10 in front of camera
        var fromPos = GetFromPos();
        CityModel.Instance.FlyBrick(fromPos, nextBrick, this.colorIndex);

        emitterBrickData.SetBrickState(emitterBrickData.GetBrickIndex(BrickState.Colored), BrickState.Full);
        elementBrickData.brickData.SetBrickState(elementBrickData.index, BrickState.Emitting);

        SlotModel.Instance.UpdateEmitters(emitter);
        DOVirtual.DelayedCall(Durations.FlyBrickDuration, OnFlyComplete, false);

        if (emitterBrickData.coloredAmount == 0)
        {
            SlotModel.Instance.Replace(emitterBrickData, SlotElementType.Undefined);
        }
    }



    private void OnFlyComplete()
    {
        new SoundCmd(SoundModel.Instance.BRICK_CLICK).Run();

        //Debug.Log("EmitBricksCmd OnFlyComplete Fly complete for color " + this.colorIndex + " time " + Time.time);
        {
            elementBrickData.brickData.SetBrickState(elementBrickData.index, BrickState.Full);
            cityElement.ShowCurrentState();
        }

        if (elementDataContainer.ElementCountColoredBricks() == 0 && elementDataContainer.ElementCountEmittingBricks() == 0)
        {
            CityModel.Instance.EnableDifferentColors(cityElement, BalancingModel.AdditionalBricksOnEmptyElement);
            new SoundCmd(SoundModel.Instance.NEW_COLORED_BRICKS_APPEAR).Run();
        }

        if (elementDataContainer.ElementCompleted())
        {
            var delay = 0.25f;
            new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK, delay).Run();
            DOVirtual.DelayedCall(delay, CamModel.Instance.MoveCamBack);
            CityModel.Instance.OnElementCompleted?.Invoke(cityElement);
        }

    }
}