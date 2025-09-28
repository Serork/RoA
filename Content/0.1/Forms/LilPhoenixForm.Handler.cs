using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Content.Forms;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class LilPhoenixFormHandler : ModPlayer {
    public const float MAXCHARGE = 3.5f;

    internal bool _phoenixJumped, _phoenixJumped2;
    internal bool _phoenixJustJumped, _phoenixJustJumpedForAnimation, _phoenixJustJumpedForAnimation2;
    internal int _phoenixJumpsCD;
    internal int _phoenixJump;
    internal Vector2 _tempPosition;
    internal bool _isPreparing, _wasPreparing, _prepared;
    internal float _charge, _charge2, _charge3;
    internal bool _dashed, _dashed2;
    internal bool _holdLmb;

    internal void ResetDash(bool hardReset = false) {
        if (_dashed) {
            ClearProjectiles();
        }
        _dashed = _dashed2 = false;
        _wasPreparing = true;
        _prepared = true;
        _charge = _charge2 = 0f;
        if (hardReset || Player.GetModPlayer<BaseFormHandler>().IsConsideredAs<LilPhoenixForm>()) {
            Player.eocDash = 0;
            Player.armorEffectDrawShadowEOCShield = true;
        }
    }

    internal void ClearProjectiles() {
        //foreach (Projectile projectile in Main.ActiveProjectiles) {
        //    if (projectile.owner != Player.whoAmI) {
        //        continue;
        //    }
        //    if (projectile.type == (ushort)ModContent.ProjectileType<LilPhoenixTrailFlame>()) {
        //        projectile.Kill();
        //    }
        //}
    }

    public override void ResetEffects() {
        if (!Player.GetModPlayer<BaseFormHandler>().IsInADruidicForm) {
            ResetDash();
        }
    }
}

