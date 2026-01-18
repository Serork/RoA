using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Heavy : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Heavy");
        //Description.SetDefault("Press DOWN to increase falling speed");
    }

    public override void Update(Player player, ref int buffIndex) {
        player.noFallDmg = true;
        player.maxFallSpeed += 6;
        if ((player.gravDir == 1 && player.velocity.Y > 0) || (player.gravDir == -1 && player.velocity.Y < 0)) {
            player.velocity.Y *= 1.035f;
            //if (player.controlDown) player.velocity.Y *= 1.035f;
        }

        player.GetModPlayer<HeavyExtraEffects>().IsEffectActive = true;
    }

    private class HeavyExtraEffects : ModPlayer {
        private bool _onGround;
        private Vector2 _speedBeforeGround;
        private int _fallLength;

        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            if (WorldGenHelper.CustomSolidCollision(Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms)) {
                if (Player.IsGrounded() && _fallLength > 12f && !_onGround) {
                    SoundEngine.PlaySound(SoundID.Item167 with { PitchVariance = 0.1f, Volume = 0.8f }, Player.Bottom);
                    if (_fallLength > 200f && Main.rand.NextBool(5)) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "VeryHeavy") with { Volume = 0.75f }, Player.Bottom);
                    else SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Heavy") with { PitchVariance = 0.25f, Volume = 0.2f }, Player.Bottom);
                    Vector2 velocity = _speedBeforeGround * 0.5f;
                    int count = (int)_speedBeforeGround.Length();
                    for (int i = 0; i < count * 2; i++) {
                        int dust = Dust.NewDust(Player.Bottom - new Vector2(Player.width / 2, 30f), Player.width, 30, DustID.Smoke, -velocity.X * 0.4f, -velocity.Y * 0.4f, 40, Color.GhostWhite,
                            (Main.rand.NextFloat() * 1.5f + Main.rand.NextFloat() * 1.5f) * 0.75f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity.X *= 7f;
                        Main.dust[dust].velocity *= 0.5f;
                    }

                    int dustType = TileHelper.GetKillTileDust((int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16, Main.tile[(int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16]);
                    for (int k = 0; k < count * 3; k++) {
                        Dust.NewDust(new Vector2(Player.position.X, Player.Bottom.Y), Player.width, 2, dustType, SpeedX: -velocity.X * 0.4f, SpeedY: -velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.85f);
                    }

                    _onGround = true;
                }

                return;
            }

            _speedBeforeGround = Player.velocity;
            if (Player.velocity.Y > 9.5f) _fallLength++;
            else _fallLength = 0;

            if (_fallLength > 10f) {
                int dust = Dust.NewDust(Player.Bottom - new Vector2(Player.width / 2, 30f), Player.width, 30, DustID.Smoke, 0, 0, 40, Color.GhostWhite,
                            (Main.rand.NextFloat() * 1.5f + Main.rand.NextFloat() * 1.5f) * 0.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = new Vector2(0, -3).RotatedByRandom(80);
            }

            _onGround = false;
        }
    }
}