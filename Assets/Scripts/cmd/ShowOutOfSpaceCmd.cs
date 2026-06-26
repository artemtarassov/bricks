using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowOutOfSpaceCmd
{
    public void Run()
    {
        Assert.IsFalse(ViewModel.Instance.HasAnyView(), "ShowOutOfSpaceCmd should only be run when there is already a view showing");
        Assert.IsTrue(ModelUtils.IsOutOfSpace() || ModelUtils.IsOutOfTime(), "ShowOutOfSpaceCmd should only be run when player is out of space or out of time");

        if (ModelUtils.IsOutOfSpace())
            LogEventCmd.OutOfSpace();

        if (ModelUtils.IsOutOfTime())
            LogEventCmd.OutOfTime();
        //
        new ShowViewCmd(ViewName.OutOfSpaceView).Run();
        new SoundCmd("impact_deep_thud_bounce_01").Run();
        SoundModel.Instance.Stop(SoundModel.Instance.MUSIC1);
        PlayerModel.Instance.playerData.difficultyIndex = Random.Range(0, 3);
        PlayerModel.Instance.playerData.isDirty = true;
        ViewModel.Instance.DisableOutOfSpaceCounter();
        ChallengeModel.Instance.StopChallenge();
    }

}