using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class PlayerCommonHandler : ModPlayer {
    private static float MAXFALLSPEEDMODIFIERFORFALL => 0.5f;

    public static ushort CONTROLUSEITEMTIMECHECKBASE => 10;

    private bool _fell;
    private float _fellTimer;
    private ushort _controlUseItemTimer;

    public ushort ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
    public bool ControlUseItem;

    public bool Fell { get; private set; }

    public delegate void PreUpdateDelegate(Player player);
    public static event PreUpdateDelegate PreUpdateEvent;
    public override void PreUpdate() {
        PreUpdateEvent?.Invoke(Player);
    }

    public override void PostUpdate() {
        if (Player.controlUseItem) {
            ControlUseItem = true;
        }
        else if (ControlUseItem) {
            if (_controlUseItemTimer++ > ControlUseItemTimeCheck) {
                ControlUseItem = false;
                _controlUseItemTimer = 0;
                ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
            }
        }

        TouchGround();
    }

    private void TouchGround() {
        if (Player.velocity.Y >= Player.maxFallSpeed * MAXFALLSPEEDMODIFIERFORFALL && !_fell) {
            _fell = true;
        }
        if (_fell && Player.velocity.Y == 0f && !Fell) {
            Fell = true;
            _fell = false;
            _fellTimer = 10f;
            return;
        }
        if (MathF.Abs(Player.velocity.Y) > 1f) {
            Fell = false;
        }
        if (_fellTimer > 0f) {
            _fellTimer--;
        }
        else if (!_fell) {
            Fell = false;
        }
    }
}
