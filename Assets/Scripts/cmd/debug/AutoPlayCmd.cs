using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class AutoPlayCmd
{

    private static Sequence sequence;
    public void Run()
    {
        //Time.timeScale = 2.0f;
        if (sequence != null)
        {
            sequence.Kill();
            Time.timeScale = 1.0f;
            return;
        }
        sequence = DOTween.Sequence().AppendInterval(1f).AppendCallback(OnSecUpdate).SetLoops(-1);

    }

    private void OnSecUpdate()
    {
        if (ViewModel.Instance.HasAnyView())
        {
            if (sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }
            return;
        }

        var columns = SlotModel.Instance.Columns;
        var currentElement = ModelUtils.GetCurrentElement();

        if (currentElement.dataContainer.ElementCountEmittingBricks() > 0)
        {
            return;
        }

        var coloredBricksInElement = currentElement.dataContainer.brickDataList.FindAll(b => b.coloredAmount > 0);


        for (var row = 0; row < 3; row++)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var clrInColumn = SlotModel.Instance.GetNextSlotElementDataInColumn(column.columnIndex, row);
                if (clrInColumn == null)
                {
                    continue;
                }
                if (clrInColumn.type == SlotElementType.Bricks)
                {
                    var clrInElement = coloredBricksInElement.Find(c => c.color == clrInColumn.brickData.color);
                    if (clrInElement != null)
                    {
                        new SelectColumnCmd(column.columnIndex).Run();
                        return;
                    }
                }
                else
                {
                    new SelectColumnCmd(column.columnIndex).Run();
                    return;
                }
            }
        }



    }



}