using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SoundModel
{
    public static SoundModel Instance;

    public readonly string DONG = "dong";
    public readonly string CONFIRM = "confirm";

    public readonly string ERROR = "error";
    public readonly string COINS_FLY = "coins_fly";

    public readonly string COINS = "coins";

    public readonly string MAGIC_LIGHT = "magic_shine_light_spell_04";

    public readonly string MUSIC1 = "music1";

    public readonly string BRICK_CLICK = "switch_button_push_small_05";
    public readonly string EXPLOSION1 = "explosion1";

    public readonly string CAM_MOVE_BACK = "camMoveBack";

    public readonly string TICK = "clock_tick_01";
    public readonly string TOCK = "clock_tock_01";

    public readonly string CLICK1 = "click1";
    public readonly string CLICK2 = "click2";

    public readonly string EMITTER_OPEN = "wood_block_sticks_hit_clap_04";
    public readonly string EMITTER_CLOSE = "";//ui_interface_57 switch_button_push_small_06
    //public readonly string SELECT_COLUMN = "ui_interface_90";
    public readonly string NEW_COLORED_BRICKS_APPEAR = "ui_interface_60";

    public Action<string> OnPlaySound;
    public Action<string> OnStopSound;

    public void Play(string name)
    {
        this.OnPlaySound?.Invoke(name);
    }

    public void Stop(string name)
    {
        this.OnStopSound?.Invoke(name);
    }

}