using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;


public class EmitBricksCmd
{
    public void Run()
    {
        var emitters = SlotModel.Instance.Emitters;
        foreach (var emitter in emitters)
        {
            if (emitter != null && emitter.amount > 0)
            {
                new EmitBrickCmd(emitter).Run();
                var hasCityElement = CityModel.Instance.GetCurrentElement() != null;
                if (!hasCityElement)
                {
                    return;
                }
            }
        }
    }
}

public class EmitBrickCmd
{
    private CityElement cityElement;
    private Transform nextBrick;
    private ColorIndex colorIndex => emitter.color;
    private BrickData emitter;

    public EmitBrickCmd(BrickData emitter)
    {
        this.cityElement = CityModel.Instance.GetCurrentElement();
        this.emitter = emitter;
        Assert.IsNotNull(this.cityElement, "No current city element found");
        Assert.AreNotEqual(this.colorIndex, ColorIndex.Undefined, "Color index must be defined");
    }

    private Vector3 GetFromPos()
    {
        var screenPos = ViewModel.Instance.Emitters.Find(e => e.brickData == this.emitter).transform.position;
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10));
        return worldPos;
    }

    public void Run()
    {
        if (this.cityElement == null)
        {
            return;
        }
        var bricksLeft = cityElement.CountColoredBricks(this.colorIndex);
        if (bricksLeft == 0)
        {
            return;
        }
        this.nextBrick = cityElement.GetNextColoredBrick(this.colorIndex);
        if (nextBrick == null)
        {
            return;
        }
        //from pos is 10 in front of camera
        var fromPos = GetFromPos();
        CityModel.Instance.FlyBrick(fromPos, nextBrick, this.colorIndex);
        cityElement.SetBrickState(nextBrick, BrickState.Flying);
        SlotModel.Instance.DecrementEmitter(this.emitter);
        DOVirtual.DelayedCall(Durations.FlyBrickDuration, OnFlyComplete);
    }

    private void OnFlyComplete()
    {
        cityElement.SetBrickState(this.nextBrick, BrickState.Full);
        if (cityElement.HasVisuals() || cityElement.CountFlyingBricks() > 0)
        {
            return;
        }
        var cityElementCompleted = cityElement.AllFull();
        if (cityElementCompleted)
        {
            //cityElement.EnableAllBricks(false);
            cityElement.AddComponent<BrickTopDownExplosion>();
            cityElement.EnableVisuals(true);
            new UnlockCityElementCmd().Run();
        }
        else
        {
            var hasColoredBricks = cityElement.CountColoredBricks() > 0;
            var hasTransparentBricks = cityElement.CountTransparentBricks() > 0;
            if (!hasColoredBricks && hasTransparentBricks)
            {
                cityElement.ShowNextColoredBricks(BalancingModel.AdditionalBricksOnEmptyElement);
            }

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