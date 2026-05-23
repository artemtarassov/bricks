using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class CompleteAdRewardCmd
{
    public void Run(string unit, bool recorded)
    {
        AdModel.Instance.SetRewardEarned(unit);
    }

}

[System.Serializable]
sealed class CompeleteAdRewardCmd
{
    public string unit;
    public bool recorded;
}