using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckOutOfSpaceCmd
{
    public void Run()
    {
        var currentElement = CityModel.Instance.GetCurrentElement();
        if (currentElement == null || currentElement.CountFlyingBricks() > 0)
        {
            return;
        }

        if (ViewModel.Instance.HasAnyView())
        {
            return;
        }

        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();
        if (cntEmitterSpace > 0)
        {
            return;
        }
        if (cntEmitterSpace < 1)
        {
            Debug.Log("No empty emitter space");
            SetOutOfSpace();
            return;
        }

        /*var columns = SlotModel.Instance.Columns;
        var emitters = SlotModel.Instance.Emitters;
        var colorsInSlots = new HashSet<ColorIndex>();
        var colorsInCityElement = currentElement.GetBrickColors().ToList();

        foreach (var column in columns)
        {
            var bricksInColumn = column.list.FindAll(e => e.type == SlotElementType.Bricks || e.type == SlotElementType.HiddenBricks);
            for (var i = 0; i < cntEmitterSpace && i < bricksInColumn.Count; i++)
            {
                colorsInSlots.Add(bricksInColumn[i].brickData.color);
            }

        }

        var hasCorrectColor = colorsInSlots.Any(c => colorsInCityElement.Contains(c));
        if (!hasCorrectColor)
        {
            Debug.Log("No correct color in slots, no space. cntEmitterSpace: " + cntEmitterSpace + ". Colors in slots: " + string.Join(", ", colorsInSlots) + ". Colors in city element: " + string.Join(", ", colorsInCityElement));
            SetOutOfSpace();
            return;
        }*/
    }

    private void SetOutOfSpace()
    {
        var attemptsLeft = PlayerModel.Instance.playerData.attempts;
        if (attemptsLeft > 0)
        {
            new ShowViewCmd().Run(ViewName.OutOfSpaceView);
        }
        else
        {
            new ShowViewCmd().Run(ViewName.GameOverView);
        }
    }
}