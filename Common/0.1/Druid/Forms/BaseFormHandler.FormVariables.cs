using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content;
using RoA.Content.Forms;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;

sealed partial class BaseFormHandler : ModPlayer {
    #region GENERIC
    public int ShootCounter;
    public float AttackFactor, AttackFactor2;
    public byte AttackCount;
    public Vector2 SavedVelocity;
    public bool IsPreparing, WasPreparing, Prepared;
    public Vector2 TempPosition;
    public bool Dashed, Dashed2;
    public IDoubleTap.TapDirection DashDirection;
    public float DashDelay, DashTimer;
    public int[] LocalNPCImmunity = new int[Main.npc.Length];
    public bool? FacedRight;
    public float DirectionChangedFor;

    public bool Jumped, Jumped2;
    public bool JustJumped, JustJumpedForAnimation, JustJumpedForAnimation2;
    public int JumpCD;
    public int Jump;

    public bool ActiveDash => DashDelay > 0;

    public static void ForcedDirectionChange(Player player, float changeFor = 1f, bool net = false) {
        ref bool? facedRight = ref player.GetFormHandler().FacedRight;
        ref float directionChagedFor = ref player.GetFormHandler().DirectionChangedFor;
        var value = (player.GetWorldMousePosition().X > player.position.X ? 1 : -1) == 1;
        if (facedRight != value) {
            facedRight = value;
            if (net) {
                player.SyncMousePosition();
            }
        }
        directionChagedFor = changeFor;
    }
    #endregion

    #region GRYPHON
    public static float MOVESPEEDBUFFTIMEINTICKS => 90f;
    public static float MOVESPEEDBUFFCOOLDOWN => 180f;

    public bool JustStartedDoingLoopAttack;
    public bool LoopAttackIsDone;
    public ushort CanDoLoopAttackTimer;
    public float MoveSpeedBuffTime;

    public bool CanDoLoopAttack {
        get => CanDoLoopAttackTimer <= 0;
        set => CanDoLoopAttackTimer = (ushort)(value ? 0 : 30);
    }

    public bool IncreasedMoveSpeed => MoveSpeedBuffTime > 0f;
    public bool CanIncreaseMoveSpeed => MoveSpeedBuffTime == 0f;

    public bool IsInLoopAttack => AttackFactor > 0;

    public partial void PostUpdate2() {
        if (!Player.GetFormHandler().IsConsideredAs<HallowedGryphon>()) {
            return;
        }
        if (LoopAttackIsDone && CanDoLoopAttackTimer > 0) {
            CanDoLoopAttackTimer--;
            if (CanDoLoopAttackTimer <= 0) {
                LoopAttackIsDone = false;
            }
        }
        if (IncreasedMoveSpeed) {
            MoveSpeedBuffTime--;
            if (CanIncreaseMoveSpeed) {
                MoveSpeedBuffTime = -MOVESPEEDBUFFCOOLDOWN;
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.6f, Volume = 0.8f }, Player.Center);
            }
        }
        else if (!CanIncreaseMoveSpeed) {
            MoveSpeedBuffTime++;
        }
    }

    public partial void Load1() {
        On_Player.GetImmuneAlpha += On_Player_GetImmuneAlpha;
        On_Player.GetImmuneAlphaPure += On_Player_GetImmuneAlphaPure;
    }

    private Color On_Player_GetImmuneAlphaPure(On_Player.orig_GetImmuneAlphaPure orig, Player self, Color newColor, float alphaReduction) {
        if (self.GetFormHandler().IsConsideredAs<HallowedGryphon>() && self.GetFormHandler().IncreasedMoveSpeed) {
            float num = (float)(255 - self.immuneAlpha) / 255f;
            if (alphaReduction > 0f)
                num *= 1f - alphaReduction;

            float shimmerTransparency = 0.15f * HallowedGryphon.GetMoveSpeedFactor(self);
            if (shimmerTransparency > 0f) {
                if ((double)shimmerTransparency >= 0.8)
                    return Color.Transparent;

                num *= 1f - shimmerTransparency;
                num *= 1f - shimmerTransparency;
                num *= 1f - shimmerTransparency;

                newColor.A = 0;
            }

            if (self.immuneAlpha > 125)
                return Color.Transparent;

            newColor.A = 0;

            Color result = Color.Multiply(newColor.MultiplyRGB(Color.Lerp(Color.LightYellow, new Color(255, 224, 224), Helper.Wave(0f, 1f, speed: 15f))), num);
            //result.A = (byte)Math.Max(result.A - 25, 0);
            return result;
        }

        return orig(self, newColor, alphaReduction);
    }

    private Color On_Player_GetImmuneAlpha(On_Player.orig_GetImmuneAlpha orig, Player self, Color newColor, float alphaReduction) {
        if (self.GetFormHandler().IsConsideredAs<HallowedGryphon>() && self.GetFormHandler().IncreasedMoveSpeed) {
            float num = (float)(255 - self.immuneAlpha) / 255f;
            float shimmerTransparency = 0.15f * HallowedGryphon.GetMoveSpeedFactor(self);
            if (alphaReduction > 0f)
                num *= 1f - alphaReduction;

            if (shimmerTransparency > 0f)
                num *= 1f - shimmerTransparency;

            newColor.A = 0;

            Color result = Color.Multiply(newColor.MultiplyRGB(Color.Lerp(Color.LightYellow, new Color(255, 224, 224), Helper.Wave(0f, 1f, speed: 15f))), num);
            //result.A = (byte)Math.Max(result.A - 25, 0);
            return result;
        }

        return orig(self, newColor, alphaReduction);
    }

    public void ResetGryphonStats() {
        JustStartedDoingLoopAttack = LoopAttackIsDone = false;
        SavedVelocity = Vector2.Zero;
        CanDoLoopAttackTimer = 0;
        AttackFactor = 0f;
        AttackCount = 0;
        MoveSpeedBuffTime = 0f;
        AttackFactor2 = 0f;
    }

    public partial void OnDoubleTap2(Player player, IDoubleTap.TapDirection direction) {
        bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
        if (!flag) {
            return;
        }
        if (!player.GetFormHandler().IsConsideredAs<HallowedGryphon>()) {
            return;
        }

        player.GetFormHandler().GainGryphonMoveSpeedBuff();
    }

    public void GainGryphonMoveSpeedBuff() {
        if (!CanIncreaseMoveSpeed) {
            return;
        }
        MoveSpeedBuffTime = MOVESPEEDBUFFTIMEINTICKS;
        SoundEngine.PlaySound(SoundID.Item66 with { Pitch = 0.9f, Volume = 0.8f }, Player.Center);
        SoundEngine.PlaySound(SoundID.Item77 with { Pitch = 1.2f, Volume = 0.8f }, Player.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "GryphonRoar") with { Pitch = 0.5f, Volume = 2f }, Player.Center);
    }
    #endregion
    #region PHOENIX
    public const float MAXPHOENIXCHARGE = 3.5f;

    internal float _charge, _charge2, _charge3;

    internal void ResetPhoenixDash(bool hardReset = false) {
        if (Dashed) {
            ClearPhoenixProjectiles();
        }
        Dashed = Dashed2 = false;
        WasPreparing = true;
        Prepared = true;
        _charge = _charge2 = 0f;
        if (hardReset || Player.GetFormHandler().IsConsideredAs<LilPhoenixForm>()) {
            Player.eocDash = 0;
            Player.armorEffectDrawShadowEOCShield = true;
        }
    }

    internal void ClearPhoenixProjectiles() {
        //foreach (Projectile projectile in Main.ActiveProjectiles) {
        //    if (projectile.owner != Player.whoAmI) {
        //        continue;
        //    }
        //    if (projectile.type == (ushort)ModContent.ProjectileType<LilPhoenixTrailFlame>()) {
        //        projectile.Kill();
        //    }
        //}
    }

    public partial void ResetEffects1() {
        if (!Player.GetFormHandler().IsInADruidicForm) {
            ResetPhoenixDash();
        }
    }
    #endregion
    #region FLEDER
    public const int CD = 50, DURATION = 35;
    public const float SPEED = 10f;

    public partial void ResetEffects2() {
        if (!Player.GetFormHandler().IsInADruidicForm) {
            DashDelay = DashTimer = 0;
            DashDirection = IDoubleTap.TapDirection.None;
            ShootCounter = 0;
        }
    }

    public partial void OnDoubleTap1(Player player, IDoubleTap.TapDirection direction) {
        bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
        if (!flag) {
            return;
        }
        if (!player.GetFormHandler().IsConsideredAs<FlederForm>()) {
            return;
        }

        player.GetFormHandler().UseFlederDash(direction);
    }

    public override void PreUpdateMovement() {
        bool flag = DashDirection != IDoubleTap.TapDirection.None || ActiveDash;
        if (flag && !Player.GetFormHandler().IsConsideredAs<FlederForm>()) {
            DashDirection = IDoubleTap.TapDirection.None;
            DashDelay = DashTimer = 0;
            return;
        }

        if (flag && !ActiveDash) {
            Vector2 newVelocity = Player.velocity;
            int dashDirection = (DashDirection == IDoubleTap.TapDirection.Right).ToDirectionInt();
            switch (DashDirection) {
                case IDoubleTap.TapDirection.Left:
                case IDoubleTap.TapDirection.Right: {
                        newVelocity.X = dashDirection * SPEED;
                        break;
                    }
            }
            DashDirection = IDoubleTap.TapDirection.None;
            DashDelay = CD;
            DashTimer = DURATION;
            SpawnFlederDusts(Player);
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
            DashDelay--;
        }

        if (DashTimer > 0) {
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
                if (LocalNPCImmunity[k] > 0) {
                    LocalNPCImmunity[k]--;
                }
            }
            Player.eocDash = (int)DashTimer;
            Player.armorEffectDrawShadowEOCShield = true;
            if (Player.velocity.Length() > 5f) {
                Rectangle rectangle = new((int)((double)Player.position.X + (double)Player.velocity.X * 0.5 - 4.0), (int)((double)Player.position.Y + (double)Player.velocity.Y * 0.5 - 4.0), Player.width + 8, Player.height + 8);
                for (int i = 0; i < 200; i++) {
                    NPC nPC = Main.npc[i];
                    if (!nPC.active || nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !Player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC))
                        continue;

                    if (LocalNPCImmunity[i] > 0) {
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

                        DashTimer = DURATION;
                        DashDelay = CD;
                        Player.velocity *= 0.9f;
                        LocalNPCImmunity[i] = 10;
                        Player.immune = true;
                        Player.immuneTime = 10;
                        Player.immuneNoBlink = true;
                    }
                }
            }
            DashTimer--;
        }
    }

    internal static void SpawnFlederDusts(Player player, int strength = 3) {
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
        var handler = Player.GetFormHandler();
        if (handler.ActiveDash) {
            return;
        }

        handler.DashDirection = direction;
        if (!server && Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new FlederDashPacket(Player, direction));
        }
    }
    #endregion
    #region INSECT
    public partial void ResetEffects3() {
        if (!Player.GetFormHandler().IsInADruidicForm) {
            FacedRight = null;
            ShootCounter = 0;
            AttackFactor = 0;
            AttackFactor2 = 0;
            DirectionChangedFor = 0f;
        }
    }

    public partial void PostUpdate1() {
        if (DirectionChangedFor > 0f) {
            DirectionChangedFor -= TimeSystem.LogicDeltaTime;
            //if (Player.controlLeft || Player.controlRight || Player.controlJump) {
            //    DirectionChangedFor = 0f;
            //}
            return;
        }
        if (FacedRight != null) {
            FacedRight = null;
        }
    }
    #endregion
    #region ENT
    public override void OnHurt(Player.HurtInfo info) {
        if (!Player.GetFormHandler().IsConsideredAs<HallowEnt>()) {
            return;
        }

        var hallowWardProjectile = TrackedEntitiesSystem.GetTrackedProjectile<HallowWard>(checkProjectile => checkProjectile.owner != Player.whoAmI);
        foreach (var projectile in hallowWardProjectile) {
            projectile.Kill();
            ProjectileUtils.SpawnPlayerOwnedProjectile<HallowWard>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_OnHurt(info.DamageSource)) {
                Position = Player.Center
            });
            return;
        }

        ProjectileUtils.SpawnPlayerOwnedProjectile<HallowWard>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_OnHurt(info.DamageSource)) {
            Position = Player.Center
        });
    }
    #endregion
}
