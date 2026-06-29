using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class NextExplosionStepCmd
{
    public void Run(bool last)
    {
        var elemnt = ModelUtils.GetCurrentElement();
        if (elemnt.ExplosionStepsCompleted())
        {
            return;
        }
        var completed = elemnt.NextExplosionStep();
        new SoundCmd(SoundModel.Instance.EXPLOSION1).Run();

        if (last)
        {
            while (!elemnt.ExplosionStepsCompleted())
            {
                elemnt.NextExplosionStep();
            }
            completed = true;
        }

        if (completed)
        {
            if (ModelUtils.CurrentBuildingCompleted())
            {
                DOVirtual.DelayedCall(1, new CompleteCurrentBuildingCmd().Run);
            }
            else
            {
                new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.NextElement, true).Run();
            }
        }
    }

}