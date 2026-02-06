using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Common.VisualEffects;
using RoA.Content.Items.Equipables.Armor.Nature.Hardmode;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class Phoenix : BaseForm {
    private static byte FRAMECOUNT => 14;

    public static ushort PREPARATIONTIME => MathUtils.SecondsToFrames(2);
    public static ushort ATTACKTIME => MathUtils.SecondsToFrames(1);
    public static byte FIREBALLCOUNT => 5;
    private static float AFTERDASHOFFSETVALUE => 50f;

    private static VertexStrip _vertexStrip = new VertexStrip();
    private static Asset<Texture2D> _burnoutTexture = null!;

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1.2f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, -8f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    protected override void SafeLoad() {
        ExtraDrawLayerSupport.PreMountBehindDrawEvent += ExtraDrawLayerSupport_PreMountBehindDrawEvent;
        On_PlayerDrawHelper.SetShaderForData += On_PlayerDrawHelper_SetShaderForData;
    }

    private void On_PlayerDrawHelper_SetShaderForData(On_PlayerDrawHelper.orig_SetShaderForData orig, Player player, int cHead, ref DrawData cdd) {
        if (!player.HasSetBonusFrom<FlamewardenHood>()) {
            orig(player, cHead, ref cdd);

            return;
        }
        if (!player.IsAlive()) {
            return;
        }
        float mainFactor = player.GetFormHandler().FlameTintOpacity;
        if (player.GetFormHandler().IsInADruidicForm && mainFactor > 0f) {
            orig(player, cHead, ref cdd);

            Effect flameTintShader = ShaderLoader.FlameTint.Value;
            float width = MathF.Max(100, player.width * 2.25f);
            float height = MathF.Max(100, player.height * 2.25f);
            Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
            Vector2 size = new(width, height);
            float waveFrequency = 2f;
            float waveOffset = 1f + player.whoAmI;
            Color color = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), Helper.Wave(0f, 1f, 15f, player.whoAmI * 3)).MultiplyAlpha(0.5f) * 1f;
            flameTintShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
            flameTintShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
            flameTintShader.Parameters["uImageSize0"].SetValue(size);
            flameTintShader.Parameters["uColor"].SetValue(color.ToVector3());
            flameTintShader.Parameters["uTime"].SetValue(TimeSystem.TimeForVisualEffects);
            flameTintShader.Parameters["uSaturation"].SetValue(Helper.Wave(0.25f, 0.75f, waveFrequency, waveOffset) * 2f);
            flameTintShader.Parameters["uOpacity"].SetValue(mainFactor);
            float globalOpacity = MathHelper.Lerp(MathUtils.Clamp01(Helper.Wave(0.625f, 0.75f, 10f, waveOffset) * 1.375f), 1f, 1f - mainFactor);
            flameTintShader.Parameters["uGlobalOpacity"]
                .SetValue(MathHelper.Lerp(0.375f, 1f, 1f - mainFactor));
            flameTintShader.Parameters["alphaModifier"]
    .SetValue(MathHelper.Lerp(globalOpacity * 0.375f, 1f, 1f - mainFactor));
            flameTintShader.CurrentTechnique.Passes[0].Apply();

            return;
        }

        orig(player, cHead, ref cdd);
    }

    private void ExtraDrawLayerSupport_PreMountBehindDrawEvent(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.HasSetBonusFrom<FlamewardenHood>()) {
            return;
        }
        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }
        if (!player.GetWreathHandler().StartSlowlyIncreasingUntilFull) {
            return;
        }
        Texture2D texture = GetTexture<Phoenix>().Value;
        Rectangle clip = Utils.Frame(texture, 1, FRAMECOUNT);
        Vector2 origin = clip.Centered();
        Vector2 drawPosition = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
        drawPosition += new Vector2(GetData<Phoenix>().xOffset * player.direction, GetData<Phoenix>().yOffset);
        float opacity = player.GetWreathHandler().ActualProgress4;
        float opacity2 = opacity;
        opacity = Utils.GetLerpValue(0.85f, 0.95f, opacity2, true);
        opacity *= Utils.GetLerpValue(1f, 0.95f, opacity2, true);
        Color lightColor = Lighting.GetColor(drawinfo.Position.ToTileCoordinates());
        Color baseColor = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), Helper.Wave(0f, 1f, 15f, player.whoAmI * 3));
        Color color = Color.Lerp(baseColor, baseColor.MultiplyRGB(lightColor), 0.75f).MultiplyAlpha(0.5f) * opacity * 0.75f;
        DrawData drawData = new(texture, drawPosition, clip,
            color, 0f, origin, 1f, drawinfo.playerEffect);
        drawData.shader = player.cMinion;
        drawinfo.DrawDataCache.Add(drawData);
    }

    protected override void SafeSetDefaults() {
        MountData.totalFrames = FRAMECOUNT;
        MountData.fallDamage = 0f;

        MountData.xOffset = -8;
        MountData.yOffset = -6;

        MountData.playerHeadOffset = -20;

        if (!Main.dedServ) {
            _burnoutTexture = ModContent.Request<Texture2D>(Texture + "_Burnout");
        }
    }

    public override bool ShouldSpawnFloorDust(Player player) => false;

    protected override void SafePostUpdate(Player player) {
        MountData.spawnDust = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
        MountData.spawnDustNoGravity = true;

        float to = 1f * (1f - Utils.GetLerpValue(0f, 0.2f, player.statLife / (float)player.statLifeMax2, true));
        float lerpValue = 0.1f;
        if (to > 0f) {
            lerpValue *= 0.5f;
        }
        player.GetFormHandler().FlameTintOpacity = Helper.Approach(player.GetFormHandler().FlameTintOpacity,
            to, lerpValue);

        Lighting.AddLight(player.Center, 0.5f * new Color(254, 158, 135).ToVector3() * MathHelper.Lerp(1f, 1.5f, BaseFormDataStorage.GetAttackCharge(player)));

        player.GetCommon().ShouldUpdateAdvancedShadows = true;
        player.GetFormHandler().UsePlayerSpeed = false;
        player.GetFormHandler().UsePlayerHorizontals = false;

        float num = 2f;
        float num2 = 0.1f;
        float num3 = 0.8f;

        Vector2 direction = Vector2.Zero;

        if (player.controlUp || player.controlJump) {
            direction.Y = -1f;
            //if (player.velocity.Y > 0f)
            //    player.velocity.Y *= num3;

            //player.velocity.Y -= num2;
            //if (player.velocity.Y < 0f - num)
            //    player.velocity.Y = 0f - num;
        }
        if (player.controlDown) {
            direction.Y = 1f;
            //if (player.velocity.Y < 0f)
            //    player.velocity.Y *= num3;

            //player.velocity.Y += num2;
            //if (player.velocity.Y > num)
            //    player.velocity.Y = num;
        }
        if ((player.controlUp || player.controlJump) && player.controlDown) {
            direction.Y = 0f;
        }
        //if (player.controlUp || player.controlJump || player.controlDown) {
        //}
        //else if ((double)player.velocity.Y < -0.1 || (double)player.velocity.Y > 0.1) {
        //    player.velocity.Y *= num3;
        //}
        //else {
        //    player.velocity.Y = 0f;
        //}

        if (player.controlLeft) {
            direction.X = -1f;
            //if (player.velocity.X > 0f)
            //    player.velocity.X *= num3;

            //player.velocity.X -= num2;
            //if (player.velocity.X < 0f - num)
            //    player.velocity.X = 0f - num;
        }
        if (player.controlRight) {
            direction.X = 1f;
            //if (player.velocity.X < 0f)
            //    player.velocity.X *= num3;

            //player.velocity.X += num2;
            //if (player.velocity.X > num)
            //    player.velocity.X = num;
        }
        if (player.controlLeft && player.controlRight) {
            direction.X = 0f;
        }
        //if (player.controlLeft || player.controlRight) {

        //}
        //else if ((double)player.velocity.X < -0.1 || (double)player.velocity.X > 0.1) {
        //    player.velocity.X *= num3;
        //}
        //else {
        //    player.velocity.X = 0f;
        //}

        player.velocity = Vector2.Lerp(player.velocity, direction.SafeNormalize() * 2f, direction == Vector2.Zero ? 0.2f : 0.1f);
        if (player.velocity.Length() < 0.1f) {
            player.velocity *= 0f;
        }

        if (player.velocity.X < 0f)
            player.direction = -1;
        else if (player.velocity.X > 0f)
            player.direction = 1;

        player.gravity = 0f;

        player.fullRotationOrigin = player.getRect().Centered();

        ref float attackFactor = ref player.GetFormHandler().AttackFactor;
        ref float attackFactor2 = ref player.GetFormHandler().AttackFactor2;
        ref int shootCounter = ref player.GetFormHandler().ShootCounter;
        ref byte attackCount = ref player.GetFormHandler().AttackCount;
        ref Vector2 savedVelocity = ref player.GetFormHandler().SavedVelocity;

        attackFactor2 += attackFactor2 <= 0f ? 1 : 2;
        if (attackFactor2 > PREPARATIONTIME) {
            attackFactor2 = PREPARATIONTIME;
        }
        if (attackFactor2 < 0f) {
            bool solid() {
                bool result = false;
                for (int i = 0; i < 1; i++) {
                    int width = (int)(player.width * 1.5f);
                    int height = (int)(player.height * 1.5f);
                    if (Collision.SolidCollision(player.position + player.velocity * width / 2f * i - new Vector2(width, height) / 2f, width, height)) {
                        result = true;
                        break;
                    }
                }
                return result;
            }
            void boom() {
                ref int shootCounter = ref player.GetFormHandler().ShootCounter;
                ref float attackFactor2 = ref player.GetFormHandler().AttackFactor2;
                if (shootCounter != -1) {
                    int count = 7 - (int)MathF.Abs(attackFactor2);

                    MakeExplosion(player, count);

                    for (int i = 0; i < count; i++) {
                        float maxAngle = MathHelper.PiOver4 * Main.rand.NextFloat(0.25f, 1f);
                        MakeFireball(player, 0, true, MathHelper.Pi / 2f + maxAngle * MathHelper.Lerp(-1f, 1f, (float)i / count) + maxAngle / count);
                    }
                    shootCounter = -1;
                }
            }
            if (solid()) {
                player.velocity = savedVelocity;
                if (attackFactor2 == -4f) {
                    boom();
                }
                attackFactor2 = 0f;
            }
            else {
                float blinkDistance = 0f;
                while (!solid()) {
                    blinkDistance++;
                    if (blinkDistance >= 100f) {
                        break;
                    }
                }
                if (!solid()) {
                    player.velocity = savedVelocity;
                    while (blinkDistance > 0) {
                        player.Center += player.velocity;
                        blinkDistance--;
                        if (solid()) {
                            boom();
                            break;
                        }
                    }
                }
            }

            if (player.whoAmI == Main.myPlayer) {
                Main.SetCameraLerp(0.075f, 1);
                //if (Main.mapTime < 5)
                //    Main.mapTime = 5;

                //Main.maxQ = true;
                //Main.renderNow = true;
            }

            BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f, false);

            player.fullRotation = player.velocity.X * 0.5f;
        }
        else {
            player.fullRotation = Utils.AngleLerp(player.fullRotation, player.velocity.X * 0.03f, 0.1f);
            savedVelocity = Vector2.Lerp(savedVelocity, Vector2.Zero, 0.1f);
        }
        if (attackFactor2 >= PREPARATIONTIME) {
            if (attackCount < FIREBALLCOUNT) {
                shootCounter = -1;
            }

            bool isAttacking = player.HoldingLMB(true);
            if (isAttacking) {
                attackFactor2 = -5f;
                attackFactor = 0f;

                attackCount = 0;

                shootCounter = 0;

                player.SyncMousePosition();
                savedVelocity = player.DirectionTo(player.GetViableMousePosition());

                player.GetCommon().ResetAdvancedShadows();

                MakeSlash(player, savedVelocity);
            }
        }
    }

    private void MakeSlash(Player player, Vector2 velocity, int count = 5) {
        if (player.IsLocal()) {
            int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(100) / count;
            float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(5f) / count;
            for (int i = 0; i < count; i++) {
                Vector2 center = player.position;
                float yProgress = (float)i / count;
                Vector2 offset = new Vector2(player.width * Main.rand.NextFloat(), player.height * yProgress);
                ProjectileUtils.SpawnPlayerOwnedProjectile<PhoenixSlash>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("phoenixattack")) {
                    Position = center,
                    Velocity = velocity,
                    AI0 = offset.X,
                    AI1 = offset.Y,
                    AI2 = MathUtils.YoYo(yProgress),
                    Damage = baseDamage,
                    KnockBack = baseKnockback
                });
            }
        }
    }

    private void MakeExplosion(Player player, float strength = 1f) {
        if (player.IsLocal()) {
            int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(100);
            float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(5f);
            Vector2 center = player.GetPlayerCorePoint();
            Vector2 velocity = -player.GetFormHandler().SavedVelocity * 2.5f;
            ProjectileUtils.SpawnPlayerOwnedProjectile<PhoenixExplosion>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("phoenixattack")) {
                Position = center,
                Velocity = velocity,
                Damage = baseDamage,
                KnockBack = baseKnockback,
                AI1 = strength
            });
        }
    }

    private void MakeFireball(Player player, int attackCount, bool noExtraPosition = false, float velocityRotation = 1f) {
        if (player.IsLocal()) {
            int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(100);
            float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(5f);

            float offsetValue = TileHelper.TileSize * 5;
            Vector2 center = player.GetPlayerCorePoint();
            if (noExtraPosition) {
                center += Main.rand.NextVector2Circular(player.width, player.height) / 2f;
            }
            float meInQueueValue = attackCount + 2;
            Vector2 offset = Vector2.One.RotatedBy(MathHelper.TwoPi / FIREBALLCOUNT) * offsetValue;
            if (noExtraPosition) {
                meInQueueValue = -MathF.Abs(velocityRotation);
            }
            Vector2 velocity = Vector2.UnitY * 5f;
            ProjectileUtils.SpawnPlayerOwnedProjectile<PhoenixFireball>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("phoenixattack")) {
                Position = center,
                Velocity = velocity,
                AI0 = meInQueueValue,
                AI1 = offset.X,
                AI2 = offset.Y,
                Damage = baseDamage,
                KnockBack = baseKnockback
            });
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        void playFlapSound(bool reset = false) {
            if (reset) {
                player.flapSound = false;
                return;
            }
            if (!player.flapSound) {
                SoundEngine.PlaySound(SoundID.Item32, player.Center);
            }
            player.flapSound = true;
        }
        if (frame == 4) {
            playFlapSound();
        }
        else {
            playFlapSound(true);
        }

        ref float attackFactor2 = ref player.GetFormHandler().AttackFactor2;
        if (++frameCounter > 6) {
            frameCounter = 0;
            frame++;
            ref float attackFactor = ref player.GetFormHandler().AttackFactor;
            ref int shootCounter = ref player.GetFormHandler().ShootCounter;
            ref byte attackCount = ref player.GetFormHandler().AttackCount;
            if (attackFactor2 >= PREPARATIONTIME) {
                if (frame == 3) {
                    if (shootCounter == -1) {
                        attackFactor = 0;
                        shootCounter = ATTACKTIME;

                        MakeFireball(player, attackCount);

                        BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f, false);

                        attackCount++;
                    }
                }
            }
            if (frame >= 7) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1f, false);

                frame = 0;
            }
        }

        if (attackFactor2 < 0f) {
            frameCounter = 0;
            frame = 13;

            MakeCopy(player.Center, (byte)frame, player.fullRotation, player.direction > 0);
        }

        if (frame > 13) {
            frame = 13;
        }

        BaseFormHandler.TransitToDark = Helper.Wave(0f, 1f, 5f, player.whoAmI);

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        skipDust = true;
        player.GetFormHandler().ResetPhoenixStats();

        Vector2 position = player.GetPlayerCorePoint() + player.velocity - Vector2.UnitY * 6f;
        for (int i = 0; i < 1; i++) {
            AdvancedDustSystem.New<AdvancedDusts.PhoenixExplosion>(AdvancedDustLayer.ABOVEDUSTS)?.Setup(position, Vector2.Zero);
        }
        Vector2 center = position;
        for (int i = 0; i < 32; i++) {
            Point size = new(44, 55);
            Vector2 offset = new Vector2(-3f, 10f);
            if (player.FacedRight()) {
                offset.X += 2f;
            }
            int dust = Dust.NewDust(center - size.ToVector2() / 2f + offset, size.X, size.Y, Utils.SelectRandom<int>(Main.rand, 6, 259, 158), 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y -= 0.5f;
            Main.dust[dust].velocity.Y *= 2.5f;
        }
        Vector2 positionInWorld = position;
        bool flag = false;
        for (int i = 0; i < 15; i++) {
            Vector2 value = Main.rand.NextVector2Circular(3f, 3f);
            float num = Main.rand.NextFloat() * 0.25f;
            float alphaModifier = 0.75f;
            Dust dust = Dust.CloneDust(Dust.NewDustPerfect(positionInWorld + Main.rand.NextVector2Circular(player.width, player.height), DustID.SparkForLightDisc, value, 0,
                Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), num) * alphaModifier, 2.3f + 0.4f * Main.rand.NextFloat()));
            dust.scale -= 0.6f;
            dust.color = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), num + 0.75f) * alphaModifier;
        }
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        skipDust = true;
        player.GetFormHandler().ResetPhoenixStats();
        if (player.statLife <= 0 && player.whoAmI == Main.myPlayer) {
            player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.Phoenix0").ToNetworkText(player.name)), 1.0, 0);

            int count = 56;
            for (int i = 0; i < count; i++) {
                Vector2 pos = player.GetPlayerCorePoint();
                int dust = Dust.NewDust(pos, 6, 6, Utils.SelectRandom<int>(Main.rand, 6, 259, 158), 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
                Main.dust[dust].position = pos + Main.rand.RandomPointInArea(20f);
                Main.dust[dust].velocity += Vector2.One.RotatedBy((float)i / count * MathHelper.TwoPi) * Main.rand.NextFloat(1f, 2f);
                Main.dust[dust].noGravity = true;
            }
            return;
        }

        Vector2 position = player.GetPlayerCorePoint() + player.velocity + Vector2.UnitY * 2f;
        Vector2 positionInWorld = position;
        bool flag = false;
        for (int i = 0; i < 15; i++) {
            Vector2 value = Main.rand.NextVector2Circular(3f, 3f);
            if (value.Y > 0f) {
                value.Y *= -1f;
            }
            if (flag) {
                value *= 1.5f;
            }
            float num = Main.rand.NextFloat() * 0.25f;
            float alphaModifier = 0.75f;
            Dust dust = Dust.CloneDust(Dust.NewDustPerfect(positionInWorld + Main.rand.NextVector2Circular(player.width, player.height), DustID.SparkForLightDisc, value, 0,
                Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), num) * alphaModifier, 2.3f + 0.4f * Main.rand.NextFloat()));
            dust.scale -= 0.6f;
            dust.color = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), num + 0.75f) * alphaModifier;
        }
        Vector2 center = position;
        for (int i = 0; i < 56; i++) {
            Point size = new(40, 55);
            Vector2 offset = new Vector2(-2f, -2f);
            int dust = Dust.NewDust(center - size.ToVector2() / 2f + offset, size.X, size.Y, Utils.SelectRandom<int>(Main.rand, 6, 259, 158), 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y += 0.5f;
            Main.dust[dust].velocity.Y *= 1.5f;
            Main.dust[dust].velocity.Y *= 0.5f;
        }
    }

    protected override void AdjustFrameBox(Player player, ref Rectangle frame) {

    }

    private Color StripColors(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f - 0.1f * BaseFormHandler.TransitToDark, 0.7f - 0.2f * BaseFormHandler.TransitToDark, progressOnStrip, clamped: true);
        Color result = Color.Lerp(Color.Lerp(Color.White, Color.Orange, BaseFormHandler.TransitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
        result.A = (byte)(result.A * 0.875f);
        result *= 0.25f;
        return result;
    }

    private float StripWidth(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f, 0.06f + BaseFormHandler.TransitToDark * 0.01f, progressOnStrip, clamped: true);
        lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
        float result = MathHelper.Lerp(24f + BaseFormHandler.TransitToDark * 16f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
        result *= 0.5f;
        return result;
    }

    protected override void DrawSelf(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        DrawData item = new(texture, drawPosition, frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects);
        item.shader = drawPlayer.cMinion;
        playerDrawData.Add(item);
        DrawGlowMask(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);

        float mainFactor = drawPlayer.GetFormHandler().FlameTintOpacity;
        Color color = drawColor;
        color *= mainFactor;
        item = new(_burnoutTexture.Value, drawPosition, frame, color, rotation, drawOrigin, drawScale, spriteEffects);
        item.shader = drawPlayer.cMinion;
        playerDrawData.Add(item);
    }

    protected override void PreDraw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        drawPosition += drawPlayer.GetFormHandler().SavedVelocity.SafeNormalize() * AFTERDASHOFFSETVALUE * MathUtils.Clamp01(drawPlayer.GetFormHandler().SavedVelocity.Length());

        float mainFactor = drawPlayer.GetFormHandler().FlameTintOpacity;
        drawColor = Color.Lerp(drawColor, drawColor.MultiplyRGB(Color.Lerp(new Color(254, 158, 35), new Color(255, 231, 66), 0.5f)).MultiplyAlpha(1f), mainFactor);

        MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
        miscShaderData.UseSaturation(-0.5f);
        miscShaderData.UseOpacity(10f);
        miscShaderData.UseOpacity(3f);
        miscShaderData.Apply();
        _vertexStrip.PrepareStripWithProceduralPadding(drawPlayer.GetCommon().GetAdvancedShadowPositions(30), drawPlayer.GetCommon().GetAdvancedShadowRotations(30),
            StripColors, StripWidth,
            -Main.screenPosition + drawPlayer.Size / 2f + new Vector2(drawPlayer.width * -drawPlayer.direction, drawPlayer.height * 0.75f) * 0.5f, includeBacksides: true);
        _vertexStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }

    protected override void DrawGlowMask(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        float value = BaseFormDataStorage.GetAttackCharge(drawPlayer);

        if (glowTexture != null) {
            float opacity = drawPlayer.GetFormHandler().AttackFactor2 / (float)PREPARATIONTIME;
            if (drawPlayer.GetFormHandler().AttackFactor2 < 5f) {
                opacity = 1f;
            }
            opacity = Ease.CubeOut(Utils.GetLerpValue(0.8f, 1f, opacity, true));

            DrawData item = new(glowTexture, drawPosition, frame, Color.White * MathHelper.Lerp(0.9f, 1f, value) * ((float)(int)drawColor.A / 255f), rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cMinion;
            playerDrawData.Add(item);
            for (float num6 = 0f; num6 < 4f; num6 += 1f) {
                float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + drawPlayer.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
                Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f * value;
                Vector2 position = item.position + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;
                DrawData item2 = item;
                item2.position = position;
                item2.color = color2;
                item2.shader = drawPlayer.cMinion;
                playerDrawData.Add(item2);
            }

            Rectangle frame2 = frame;
            frame2.Y += (frame2.Height + 2) * 7;
            value = 1f;

            item = new(glowTexture, drawPosition, frame2, Color.White * MathHelper.Lerp(0.9f, 1f, value) * ((float)(int)drawColor.A / 255f) * opacity, rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cMinion;
            playerDrawData.Add(item);
            for (float num6 = 0f; num6 < 4f; num6 += 1f) {
                float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + drawPlayer.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
                Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f * value * opacity;
                Vector2 position = item.position + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;
                DrawData item2 = item;
                item2.position = position;
                item2.color = color2;
                item2.shader = drawPlayer.cMinion;
                playerDrawData.Add(item2);
            }
        }
    }
}
