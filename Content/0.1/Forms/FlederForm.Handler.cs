using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class FlederFormHandler : ModPlayer, IDoubleTap {
    public const int CD = 50, DURATION = 35;
    public const float SPEED = 10f;

    private IDoubleTap.TapDirection _dashDirection;
    internal float _dashDelay, _dashTimer;
    private int[] _localNPCImmunity = new int[Main.npc.Length];
    internal int _shootCounter;
    internal bool _holdingLmb;

    public bool ActiveDash => _dashDelay > 0;

    public override void ResetEffects() {
        if (!Player.GetModPlayer<BaseFormHandler>().IsInADruidicForm) {
            _dashDelay = _dashTimer = 0;
            _dashDirection = IDoubleTap.TapDirection.None;
            _shootCounter = 0;
        }
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
        if (!flag) {
            return;
        }
        if (!player.GetModPlayer<BaseFormHandler>().IsConsideredAs<FlederForm>()) {
            return;
        }

        player.GetModPlayer<FlederFormHandler>().UseFlederDash(direction);
    }

    public override void PreUpdateMovement() {
        bool flag = _dashDirection != IDoubleTap.TapDirection.None || ActiveDash;
        if (flag && !Player.GetModPlayer<BaseFormHandler>().IsConsideredAs<FlederForm>()) {
            _dashDirection = IDoubleTap.TapDirection.None;
            _dashDelay = _dashTimer = 0;
            return;
        }

        if (flag && !ActiveDash) {
            Vector2 newVelocity = Player.velocity;
            int dashDirection = (_dashDirection == IDoubleTap.TapDirection.Right).ToDirectionInt();
            switch (_dashDirection) {
                case IDoubleTap.TapDirection.Left:
                case IDoubleTap.TapDirection.Right: {
                        newVelocity.X = dashDirection * SPEED;
                        break;
                    }
            }
            _dashDirection = IDoubleTap.TapDirection.None;
            _dashDelay = CD;
            _dashTimer = DURATION;
            SpawnDusts(Player);
            Player.velocity = newVelocity;
            if (Player.velocity.Y == Player.gravity) {
                Player.velocity.Y -= 5f;
            }
            Point tileCoordinates1 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), (float)(Player.gravDir * -Player.height / 2.0 + Player.gravDir * 2.0))).ToTileCoordinates();
            Point tileCoordinates2 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), 0.0f)).ToTileCoordinates();
            if (WorldGen.SolidOrSlopedTile(tileCoordinates1.X, tileCoordinates1.Y) || WorldGen.SolidOrSlopedTile(tileCoordinates2.X, tileCoordinates2.Y)) {
                Player.velocity.X /= 2f;
            }

            SoundEngine.PlaySound(SoundID.Item169 with { Pitch = -0.8f, PitchVariance = 0.1f, Volume = 0.6f }, Player.Center);

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(Player, 14, Player.Center));
            }
        }

        if (ActiveDash) {
            _dashDelay--;
        }

        if (_dashTimer > 0) {
            if (!BaseForm.IsInAir(Player)) {
                for (int i = 0; i < 3; i++) {
                    if (Main.rand.NextBool(3)) {
                        int num = 0;
                        if (Player.gravDir == -1f)
                            num -= Player.height;
                        int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height + (float)num), Player.width + 8, 4, 59, (0f - Player.velocity.X) * 0.5f, Player.velocity.Y * 0.5f, 59, default(Color), Main.rand.NextFloat(2f, 3f) * 0.95f);
                        Main.dust[num6].velocity.X = Main.dust[num6].velocity.X * 0.2f;
                        Main.dust[num6].velocity.Y = -0.5f - Main.rand.NextFloat() * 1.5f;
                        Main.dust[num6].fadeIn = 0.5f;
                        Main.dust[num6].scale *= Main.rand.NextFloat(1.1f, 1.25f);
                        Main.dust[num6].scale *= 0.8f;
                        Main.dust[num6].noGravity = true;
                    }
                }
            }

            for (int k = 0; k < 200; k++) {
                if (_localNPCImmunity[k] > 0) {
                    _localNPCImmunity[k]--;
                }
            }
            Player.eocDash = (int)_dashTimer;
            Player.armorEffectDrawShadowEOCShield = true;
            if (Player.velocity.Length() > 5f) {
                Rectangle rectangle = new((int)((double)Player.position.X + (double)Player.velocity.X * 0.5 - 4.0), (int)((double)Player.position.Y + (double)Player.velocity.Y * 0.5 - 4.0), Player.width + 8, Player.height + 8);
                for (int i = 0; i < 200; i++) {
                    NPC nPC = Main.npc[i];
                    if (!nPC.active || nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !Player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC))
                        continue;

                    if (_localNPCImmunity[i] > 0) {
                        continue;
                    }

                    Rectangle rect = nPC.getRect();
                    if (rectangle.Intersects(rect) && (nPC.noTileCollide || Player.CanHit(nPC))) {
                        int damage = 40;
                        float num = Player.GetTotalDamage(DruidClass.Nature).ApplyTo(damage);
                        float num2 = 3f;
                        bool crit = false;

                        if (Main.rand.Next(100) < (4 + Player.GetTotalCritChance(DruidClass.Nature)))
                            crit = true;

                        int num3 = Player.direction;
                        if (Player.velocity.X < 0f)
                            num3 = -1;

                        if (Player.velocity.X > 0f)
                            num3 = 1;

                        if (Player.whoAmI == Main.myPlayer)
                            Player.ApplyDamageToNPC(nPC, (int)num, num2, num3, crit, DruidClass.Nature, true);

                        _dashTimer = DURATION;
                        _dashDelay = CD;
                        Player.velocity *= 0.9f;
                        _localNPCImmunity[i] = 10;
                        Player.immune = true;
                        Player.immuneTime = 10;
                        Player.immuneNoBlink = true;
                    }
                }
            }
            _dashTimer--;
        }
    }

    internal static void SpawnDusts(Player player, int strength = 3) {
        Vector2 vector11 = player.Center;
        for (int k = 0; k < 40 - 10 * (3 - strength); k++) {
            if (Main.rand.NextChance(0.75f)) {
                int num23 = 59;
                float num24 = 0.4f;
                if (k % 2 == 1) {
                    num24 = 0.65f;
                }
                num24 *= 3f;

                Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(3 * 2));
                int num25 = Dust.NewDust(vector12 - Vector2.One * 30f, 60, 60, num23, player.velocity.X / 2f, player.velocity.Y / 2f);
                Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)3f * 2f) / 10f;
                Main.dust[num25].noGravity = true;
                Main.dust[num25].scale = num24;
            }
        }
    }

    internal void UseFlederDash(IDoubleTap.TapDirection direction, bool server = false) {
        var handler = Player.GetModPlayer<FlederFormHandler>();
        if (handler.ActiveDash) {
            return;
        }

        handler._dashDirection = direction;
        if (!server && Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new FlederFormPacket1(Player, direction));
        }
    }
}