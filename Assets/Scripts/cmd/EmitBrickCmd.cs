using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;


public class EmitBricksCmd
{
    public void Run()
    {
        var emitters = SlotModel.Instance.Emitters;
        var cityElement = CityModel.Instance.GetCurrentElement();
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
    private CityElementDataContainer dataContainer => cityElement.dataContainer;
    private Transform nextBrick;
    private ColorIndex colorIndex => emitter.brickData.color;
    private EmitterSpace emitter;
    private BrickData emitterBrickData;
    private BrickData elementBrickData;

    public EmitBrickCmd(EmitterSpace emitter)
    {
        Assert.IsTrue(emitter.HasColoredBricks);
        this.cityElement = CityModel.Instance.GetCurrentElement();
        this.emitter = emitter;
        this.emitterBrickData = emitter.brickData;
        this.elementBrickData = dataContainer.brickDataList.Find(b => b.color == this.colorIndex && b.coloredAmount > 0);
        Assert.IsNotNull(this.elementBrickData, $"No brick data found in city element for color {this.colorIndex}");
        Assert.IsNotNull(this.cityElement, "No current city element found");
        Assert.AreNotEqual(this.colorIndex, ColorIndex.Undefined, "Color index must be defined");
    }

    private Vector3 GetFromPos()
    {
        var screenPos = ViewModel.Instance.Emitters[emitter.index].position;
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10));
        return worldPos;
    }

    public void Run()
    {
        if (this.cityElement == null)
        {
            return;
        }
        var coloredBricksInElement = dataContainer.ElementCountColoredBricks(this.colorIndex);
        if (coloredBricksInElement == 0)
        {
            return;
        }

        this.nextBrick = cityElement.GetColoredBrick(this.colorIndex);
        if (nextBrick == null)
        {
            Debug.LogError("No next brick found in city element for color " + this.colorIndex);
            return;
        }
        //from pos is 10 in front of camera
        var fromPos = GetFromPos();
        CityModel.Instance.FlyBrick(fromPos, nextBrick, this.colorIndex);

        emitterBrickData.SetBrickState(emitterBrickData.GetBrickIndex(BrickState.Colored), BrickState.Full);
        elementBrickData.SetBrickState(elementBrickData.GetBrickIndex(BrickState.Colored), BrickState.Emitting);

        SlotModel.Instance.UpdateEmitters(emitter);
        DOVirtual.DelayedCall(Durations.FlyBrickDuration, OnFlyComplete);
    }

    private void OnFlyComplete()
    {
        elementBrickData.SetBrickState(elementBrickData.GetBrickIndex(BrickState.Emitting), BrickState.Full);

        var cityElementCompleted = dataContainer.ElementCompleted();
        cityElement.ShowBrickStates();

        if (cityElementCompleted)
        {
            new UnlockCityElementCmd().Run();
            return;
        }



        if (dataContainer.ElementCountColoredBricks() == 0 && dataContainer.ElementCountEmittingBricks() == 0)
        {
            dataContainer.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
            cityElement.ShowBrickStates();
        }


        /*else
        {
            var coloredBricks = cityElement.CountColoredBricks();
            var transparentBricks = cityElement.CountTransparentBricks();
            if (coloredBricks < 20 && transparentBricks > 0)
            {
                new SetNextBricksColorsCmd(cityElement).Run();
            }
        }*/
    }
}