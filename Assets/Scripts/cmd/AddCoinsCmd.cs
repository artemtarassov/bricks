using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AddCoinsCmd
{
    private int amount;
    private Vector3 fromFlyPos;

    public AddCoinsCmd(int amount, Vector3 fromFlyPos = default)
    {
        this.fromFlyPos = fromFlyPos;
        this.amount = amount;
    }
    public void Run()
    {
        if (this.fromFlyPos != default && amount > 0)
        {
            var from = PlayerModel.Instance.playerData.coins;
            var to = from + amount;
            ViewModel.Instance.FlyCoin(fromFlyPos, from, to);
            return;
        }
        PlayerModel.Instance.AddCoins(amount);
    }

}