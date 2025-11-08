using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer, IDoubleTap {
    public static ushort DEERSKULLATTACKTIME => 40;
    public static ushort DEERSKULLATTACKDISTANCE => (ushort)(TileHelper.TileSize * 15);

    public bool ApplyDeerSkullSetBonus;

    public float DeerSkullAppearanceProgress;
    public float DeerSkullHornsOpacity;
    public float DeerSkullHornsBorderOpacity, DeerSkullHornsBorderOpacity2;
    public NPC? DeerSkullHornsTarget;
    public ushort DeerSkullAttackTime;

    public partial void DeerSkullResetEffects() { }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.GetCommon().ApplyDeerSkullSetBonus && direction == Helper.CurrentDoubleTapDirectionForSetBonuses) {
            player.GetWreathHandler().SlowlyFill2();
        }
    }

    public partial void DeerSkullPostUpdateEquips() {
        float lerpValue = 0.1f;

        bool deerSkullEquippedAndActivated = !(!ApplyDeerSkullSetBonus || !Player.GetWreathHandler().ChargedBySlowFill);
        if (DeerSkullAppearanceProgress <= 0f && !deerSkullEquippedAndActivated) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 0f, lerpValue);
            return;
        }

        float targetValue = 1f;
        if (!deerSkullEquippedAndActivated) {
            if (DeerSkullAppearanceProgress != 0f) {
                DeerSkullAppearanceProgress = 0f;

                // gores

                if (!Main.dedServ) {
                    var source = Player.GetSource_Misc("deerskullhorns");
                    for (int i = 0; i < 5; i++) {
                        int minX = -56;
                        int maxX = 0;
                        if (!Player.FacedRight()) {
                            minX = 0;
                            maxX = 56 - 16;
                        }
                        int gore = Gore.NewGore(source, Player.Top + Main.rand.Random2(minX, maxX, -20, 0).RotatedBy(Player.fullRotation), Vector2.Zero, ("DeerSkullFormGore" + (Main.rand.Next(3) + 1)).GetGoreType());
                        Main.gore[gore].velocity.X *= Main.rand.NextFloat(0.1f, 1f);
                        Main.gore[gore].velocity.Y = MathF.Abs(Main.gore[gore].velocity.Y);
                        Main.gore[gore].velocity += Player.velocity;

                        minX = 0;
                        maxX = 56 - 16;
                        if (!Player.FacedRight()) {
                            minX = -56;
                            maxX = 0;
                        }
                        gore = Gore.NewGore(source, Player.Top + Main.rand.Random2(minX, maxX, -20, 0).RotatedBy(Player.fullRotation), Vector2.Zero, ("DeerSkullFormGore" + (Main.rand.Next(3) + 1)).GetGoreType());
                        Main.gore[gore].velocity.X *= Main.rand.NextFloat(0.1f, 1f);
                        Main.gore[gore].velocity.Y = MathF.Abs(Main.gore[gore].velocity.Y);
                        Main.gore[gore].velocity += Player.velocity;
                    }

                    for (int i = 0; i < 7; i++) {
                        int minX = -56;
                        int maxX = 0;
                        if (!Player.FacedRight()) {
                            minX = 0;
                            maxX = 56 - 16;
                        }
                        Dust dust = Dust.NewDustPerfect(Player.Top + Main.rand.Random2(minX, maxX, -20, 0).RotatedBy(Player.fullRotation), DustID.Bone, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2f), Scale: 1f + 0.1f * Main.rand.NextFloatDirection());
                        dust.noGravity = !Main.rand.NextBool(3);
                        Dust dust2 = dust;
                        if (dust.noGravity) {
                            dust2.scale *= 1.25f;
                        }
                        dust2 = dust;
                        dust2.velocity *= 0.5f;

                        minX = 0;
                        maxX = 56 - 16;
                        if (!Player.FacedRight()) {
                            minX = -56;
                            maxX = 0;
                        }
                        dust = Dust.NewDustPerfect(Player.Top + Main.rand.Random2(minX, maxX, -20, 0).RotatedBy(Player.fullRotation), DustID.Bone, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2f), Scale: 1f + 0.1f * Main.rand.NextFloatDirection());
                        dust.noGravity = !Main.rand.NextBool(3);
                        dust2 = dust;
                        if (dust.noGravity) {
                            dust2.scale *= 1.25f;
                        }
                        dust2 = dust;
                        dust2.velocity *= 0.5f;
                    }
                }

                return;
            }
            //targetValue = 0f;
            //lerpValue *= 1.5f;
        }

        DeerSkullAppearanceProgress = Helper.Approach(DeerSkullAppearanceProgress, targetValue, lerpValue);

        Vector2 checkPosition = Player.Center;
        int checkDistance = DEERSKULLATTACKDISTANCE;
        NPC? target = NPCUtils.FindClosestNPC(checkPosition, checkDistance, false);
        DeerSkullHornsTarget = target;

        DeerSkullHornsBorderOpacity = Helper.Approach(DeerSkullHornsBorderOpacity, 0f, lerpValue);
        DeerSkullHornsBorderOpacity2 = Helper.Approach(DeerSkullHornsBorderOpacity2, 0f, lerpValue);

        if (target is null) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 0f, lerpValue);
            return;
        }

        if (Player.HasProjectile<HornsLightning>()) {
            DeerSkullHornsOpacity = Helper.Approach(DeerSkullHornsOpacity, 1f, lerpValue * 2.5f);
        }

        void spawnLightning() {
            //DeerSkullHornsOpacity = 1f;

            if (!Player.IsLocal()) {
                return;
            }

            int damage = 50;
            float knockBack = 1f;
            Vector2 targetPosition = DeerSkullHornsTarget!.Center;
            ProjectileUtils.SpawnPlayerOwnedProjectile<HornsLightning>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("hornsattack")) {
                Damage = damage,
                KnockBack = knockBack,
                AI0 = targetPosition.X,
                AI1 = targetPosition.Y
            });
        }
        if (DeerSkullAttackTime > DEERSKULLATTACKTIME / 3) {
            if (DeerSkullAttackTime % (int)(DEERSKULLATTACKTIME / 4) == 0) {
                spawnLightning();
                DeerSkullHornsBorderOpacity = 2f;
                DeerSkullHornsBorderOpacity2 = 1f;
            }
        }
        if (DeerSkullAttackTime++ > DEERSKULLATTACKTIME) {
            DeerSkullAttackTime = 0;
        }
    }
}
