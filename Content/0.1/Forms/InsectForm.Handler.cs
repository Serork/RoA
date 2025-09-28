using RoA.Common;
using RoA.Common.Druid.Forms;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class InsectFormHandler : ModPlayer {
    internal bool? _facedRight;
    internal int _shootCounter, _insectTimer;
    internal float _directionChangedFor;

    public override void ResetEffects() {
        if (!Player.GetModPlayer<BaseFormHandler>().IsInADruidicForm) {
            _facedRight = null;
            _shootCounter = _insectTimer = 0;
            _directionChangedFor = 0f;
        }
    }

    public override void PostUpdate() {
        if (_directionChangedFor > 0f) {
            _directionChangedFor -= TimeSystem.LogicDeltaTime;
            if (Player.controlLeft || Player.controlRight || Player.controlJump) {
                _directionChangedFor = 0f;
            }
        }
    }
}
