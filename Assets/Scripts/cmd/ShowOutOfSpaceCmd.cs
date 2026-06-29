using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowGameOverCmd
{


    public void Run(GameOverReason reason)
    {

        if (reason == GameOverReason.OutOfSpace)
            LogEventCmd.OutOfSpace();

        if (reason == GameOverReason.OutOfTime)
            LogEventCmd.OutOfTime();
        //
        new ShowViewCmd(ViewName.GameOverView).Run();
        new SoundCmd("impact_deep_thud_bounce_01").Run();
        ViewModel.Instance.ChangeTopNav(TopNav.Coins);
        SoundModel.Instance.Stop(SoundModel.Instance.MUSIC1);
        PlayerModel.Instance.playerData.difficultyIndex = Random.Range(0, 3);
        PlayerModel.Instance.playerData.isDirty = true;
        ViewModel.Instance.DisableOutOfSpaceCounter();
    }

}