using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Common.VisualEffects;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class BloodshedAxe : ModProjectile, DruidPlayerShouldersFix.IProjectileFixShoulderWhileActive {
    public override bool? CanDamage() => Projectile.ai[1] >= 2f;

    private bool _powerUp;
    private int _direction;
    private bool _init, _init2;
    private float _f;
    private int _time = -1;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
    }

    public override void SetDefaults() {
        int width = 78; int height = 62;
        Projectile.Size = new Vector2(width, height);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.ownerHitCheck = true;
        Projectile.ownerHitCheckDistance = 300f;
        Projectile.usesOwnerMeleeHitCD = true;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;

        Projectile.noEnchantmentVisuals = true;
    }

    public override bool? CanCutTiles() => true;

    public override void CutTiles() {
        Utils.PlotTileLine(Projectile.Center + (Projectile.rotation - 0.7853982f).ToRotationVector2() * 55f * Projectile.scale,
            Projectile.Center + (Projectile.rotation + 0.7853982f).ToRotationVector2() * 55f * Projectile.scale, 55f * Projectile.scale,
            new Utils.TileActionAttempt(DelegateMethods.CutTiles));
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (!target.CanActivateOnHitEffect()) {
            return;
        }

        int type = ModContent.ProjectileType<BloodshedAxeEnergy>();
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
        Player player = Main.player[Projectile.owner];
        if (player.ownedProjectileCounts[type] != 0) {
            modifiers.SetCrit();
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player player = Main.player[Projectile.owner];
        int itemAnimationMax = player.itemAnimationMax;
        int itemAnimation = player.itemAnimation;
        float value = 1f - itemAnimation / (float)itemAnimationMax;
        float coneLength = 80f * Projectile.scale;
        float num1 = 0.5105088f * value;
        float maximumAngle = 0.3926991f;
        float coneRotation = Projectile.rotation + num1;
        return targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle) && Collision.CanHit(Projectile.Center, 2, 2, targetHitbox.Center.ToVector2(), 2, 2);
    }

    private void Dusts(NPC npc) {
        for (int i = 0; i < 15; i++) {
            Color newColor = Main.hslToRgb((0.92f + Main.rand.NextFloat() * 0.02f) % 1f, 1f, 0.4f + Main.rand.NextFloat() * 0.25f);
            int num4 = Dust.NewDust(npc.Center - new Vector2(0f, npc.height / 4f), 0, 0, ModContent.DustType<VampParticle>(), 0f, 0f, 0, newColor);
            Main.dust[num4].velocity = Main.rand.NextVector2Circular(2f, 2f);
            Main.dust[num4].velocity -= -Projectile.velocity * (0.5f + 0.5f * Main.rand.NextFloat()) * 1.4f;
            Main.dust[num4].noGravity = true;
            Main.dust[num4].scale = 1f;
            Main.dust[num4].position -= Main.rand.NextVector2Circular(16f, 16f);
            Main.dust[num4].velocity = Projectile.velocity;
            if (num4 != 6000) {
                Dust dust = Dust.CloneDust(num4);
                dust.scale /= 2f;
                dust.fadeIn *= 0.75f;
                dust.color = new Color(255, 255, 255, 255);
            }
        }

        for (int num251 = 0; num251 < 3; num251++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1.2f);
            Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center) / 2f;
            Main.dust[num252].noGravity = true;
            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.5f;
        }

        for (int num253 = 0; num253 < 2; num253++) {
            int num252 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 0.4f);
            switch (num253) {
                case 0:
                    Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center * 5f) / 6f;
                    break;
                case 1:
                    Main.dust[num252].position = (Main.dust[num252].position + (Projectile.Center + Projectile.velocity / 2f) * 5f) / 6f;
                    break;
            }

            Dust dust2 = Main.dust[num252];
            dust2.velocity *= 0.1f;
            Main.dust[num252].noGravity = true;
            Main.dust[num252].fadeIn = 1f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 100);

        if (!target.CanActivateOnHitEffect()) {
            return;
        }
        Player player = Main.player[Projectile.owner];
        /*if (player.ownedProjectileCounts[type] != 0) {
			return;
		}*/
        if (Projectile.ai[1] == 2f && Projectile.ai[1] != 100f) {
            Projectile.ai[1] = 100f;
            for (int num251 = 0; num251 < 3; num251++) {
                int num252 = Dust.NewDust(new Vector2(target.position.X, target.position.Y), target.width, target.height, DustID.Blood, target.velocity.X, target.velocity.Y, 50, default, 1.2f);
                Main.dust[num252].position = (Main.dust[num252].position + Projectile.Center) / 2f;
                Main.dust[num252].noGravity = true;
                Dust dust2 = Main.dust[num252];
                dust2.velocity *= 0.5f;
            }

            for (int num253 = 0; num253 < 2; num253++) {
                int num252 = Dust.NewDust(new Vector2(target.position.X, target.position.Y), target.width, target.height, DustID.Blood, target.velocity.X, target.velocity.Y, 50, default, 0.4f);
                switch (num253) {
                    case 0:
                        Main.dust[num252].position = (Main.dust[num252].position + target.Center * 5f) / 6f;
                        break;
                    case 1:
                        Main.dust[num252].position = (Main.dust[num252].position + (target.Center + target.velocity / 2f) * 5f) / 6f;
                        break;
                }

                Dust dust2 = Main.dust[num252];
                dust2.velocity *= 0.1f;
                Main.dust[num252].noGravity = true;
                Main.dust[num252].fadeIn = 1f;
            }

            for (int i = 0; i < 15; i++) {
                Vector2 position = target.Center + new Vector2(Main.rand.Next(-Projectile.width + 20, Projectile.width - 10) - 5f, 10f) + Main.rand.NextVector2Circular(target.width, target.height) + Projectile.velocity * (0.5f + 0.5f * Main.rand.NextFloat()) * 1.4f;
                Vector2 velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(0.5f, 2f) - new Vector2(0f, 0.5f);
                float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color color = new(82, 15, 15, 255);
                VisualEffectSystem.New<BloodShedDust>(VisualEffectLayer.BEHINDPLAYERS)?.Setup(position, velocity,
                    color, rotation: rotation);

                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.BloodShedParticle, player, VisualEffectLayer.BEHINDPLAYERS, position, velocity, color, 1f, rotation));
                }
            }

            if (_powerUp) {
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "BloodShed") { Volume = 0.25f, Pitch = -0.25f, MaxInstances = 3 }, Projectile.Center);
                if (!target.HasBuff<BloodShedAxesDebuff>()) {
                    if (Main.myPlayer == Projectile.owner) {
                        Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<BloodShedAxesTarget>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
                    }
                }
                else {
                    if (target.FindBuff(ModContent.BuffType<BloodShedAxesDebuff>(), out int buffIndex)) {
                        target.DelBuff(buffIndex);
                    }
                }
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_direction);
        writer.Write(_init);
        writer.Write(_f);
        writer.Write(_time);
        writer.Write(Projectile.rotation);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _direction = reader.ReadInt32();
        _init = reader.ReadBoolean();
        _f = reader.ReadSingle();
        _time = reader.ReadInt32();
        Projectile.rotation = reader.ReadSingle();
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;
        int itemAnimationMax = 50;

        if (!player.IsAliveAndFree()) {
            Projectile.Kill();
        }

        if (Projectile.owner == Main.myPlayer) {
            if (!_init) {
                _time = itemAnimationMax;
                _direction = player.GetViableMousePosition().X > player.MountedCenter.X ? 1 : -1;
                Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);
                Projectile.Center = Utils.Floor(Projectile.Center);
                Projectile.direction = Projectile.spriteDirection = player.direction = _direction;
                _init = true;
                Projectile.netUpdate = true;
            }
        }
        if (Projectile.localAI[2] == 0f) {
            Projectile.timeLeft = itemAnimationMax;
            Projectile.localAI[2] = 1f;
            float scale = Main.player[Projectile.owner].CappedMeleeOrDruidScale();
            if (scale != 1f) {
                Projectile.localAI[2] *= scale;
            }
        }

        player.bodyFrame.Y = 56;
        int baseAnimationMax = ItemLoader.GetItem(ModContent.ItemType<Items.Weapons.Melee.BloodshedAxe>()).Item.useAnimation;
        float mult = 1f + 1f - (float)itemAnimationMax / baseAnimationMax;
        int itemAnimation = player.itemAnimation;
        int min = itemAnimationMax / 2 - itemAnimationMax / 4;
        if (Projectile.ai[1] == 0f) {
            Projectile.ai[0] += 0.02f;
            if (Projectile.ai[0] > 0.2f) {
                Projectile.ai[1] = 1f;
            }
        }
        else if (Projectile.ai[1] == 1f) {
            Projectile.ai[0] -= 0.02f;
            if (Projectile.ai[0] < -0.2f) {
                Projectile.ai[1] = 0f;
            }
        }

        for (float i = 0f; i <= MathHelper.PiOver4; i += MathHelper.PiOver2) {
            Rectangle rectangle = Utils.CenteredRectangle(
            Projectile.Center + (Projectile.rotation * player.gravDir + i + MathHelper.PiOver4 * player.direction / 4f).ToRotationVector2() * 60f * Projectile.scale,
            new Vector2(55f * Projectile.scale, 55f * Projectile.scale));
            Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
        }

        if (player.dead || !player.active) {
            Projectile.Kill();
        }
        else if (_init2) {
            _f = 1f - _time / (float)itemAnimationMax + 0.5f - (_time < itemAnimationMax * 0.45f ? 1f - _time / (float)itemAnimationMax + 0.5f : 0f);
            Projectile.direction = Projectile.spriteDirection = player.direction = _direction;
            Projectile.Center = Utils.Floor(player.MountedCenter);
            if (Projectile.timeLeft > min) {
                //float value2 = f * 1.25f;
                float value2 = _f * 1.15f;
                if (value2 != 0f) {
                    Projectile.scale = value2 * Projectile.localAI[2];
                }
                Projectile.Center += Projectile.velocity * 2.5f * (0.5f + Projectile.ai[0]) * (_f > 0.5f ? _f : 1f - _f - 0.5f);
                bool flag = Math.Abs(Projectile.rotation) > 0.4f;
                if (Projectile.owner == Main.myPlayer || flag) {
                    Projectile.rotation = Projectile.rotation.AngleLerp(-MathHelper.PiOver2 - Projectile.spriteDirection * 0.4f, 0.2f);
                    if (!flag) {
                        Projectile.netUpdate = true;
                    }
                }
            }
            if (Projectile.timeLeft == min) {
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.15f, Volume = 0.55f }, Projectile.Center);
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 2f;
            }
            if (Projectile.timeLeft <= min + 5) {
                Projectile.scale -= 0.01f;
                Projectile.ai[0] += 0.01f;
                Projectile.rotation += (0.225f + Projectile.ai[0] * 1.0125f) * Projectile.spriteDirection * mult;
            }
        }
        Projectile.Opacity = Utils.GetLerpValue(itemAnimationMax, itemAnimationMax - 7, Projectile.timeLeft, clamped: true) * Utils.GetLerpValue(0f, 7f, Projectile.timeLeft, clamped: true);
        float f2 = Projectile.rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.5);
        float value = 1f - itemAnimation / (float)itemAnimationMax;
        Vector2 rotationVector2 = (f2 + value * 1.25f * MathHelper.PiOver2).ToRotationVector2();
        Vector2 position = Projectile.Center + f2.ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 10.0 * Projectile.scale);
        if (position.Distance(player.MountedCenter) > 40f && Projectile.timeLeft <= min + itemAnimationMax / 5) {
            Dust dust = Dust.NewDustPerfect(position, DustID.Blood, new Vector2?(rotationVector2 * 1f), (int)(255f - (1f - Projectile.timeLeft / min) * 1.5f * 255f), default, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
            dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
            dust.noLight = dust.noLightEmittence = true;
            dust.noGravity = true;
        }
        if (Projectile.ai[1] == 2 && Projectile.localAI[1] == 0f && player.statLife > player.statLifeMax2 / 5) {
            _powerUp = true;
            Projectile.damage = Projectile.damage - Projectile.damage / 3;
            int damage2 = Projectile.damage - Projectile.damage / 2;
            int damage = Main.DamageVar(damage2, 30, player.luck);
            player.statLife -= damage;
            CombatText.NewText(player.Hitbox, CombatText.DamagedFriendly, damage, false, false);

            for (int i = 0; i < 10; i++) {
                int dust = Dust.NewDust(Projectile.Center + new Vector2(0, -60).RotatedBy((Projectile.rotation + 90 * player.gravDir) * Projectile.spriteDirection), 0, 0, DustID.RainbowMk2, 0f, 0f, 0, new Color(255, 100, 100, 0), Main.rand.NextFloat(1f, 1.5f));
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(2f, 2f);
                Main.dust[dust].position += Main.dust[dust].velocity * Main.rand.NextFloat(20f, 25f);
                Main.dust[dust].velocity = rotationVector2 * Projectile.spriteDirection;
                Main.dust[dust].fadeIn = 1f;
                Main.dust[dust].noLightEmittence = true;
                Main.dust[dust].noGravity = true;
            }
        }
        if (_powerUp) Lighting.AddLight(Projectile.Center, 0.4f * (255 - Projectile.alpha) / 255, 0.2f * (255 - Projectile.alpha) / 255, 0.2f * (255 - Projectile.alpha) / 255);

        float armRotation = Projectile.rotation - MathHelper.PiOver2;
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        if (_time > 0) {
            _time--;
        }

        if (_init && !_init2) {
            player.ChangeDir(_direction);
            _init2 = true;
        }
    }

    public override bool ShouldUpdatePosition()
        => false;

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture2D = (Texture2D)ModContent.Request<Texture2D>(Texture);
        float extraRotation = Projectile.ai[1] != 2f ? Projectile.ai[0] * -(float)Projectile.spriteDirection : 0f;
        Player player = Main.player[Projectile.owner];
        Rectangle? rectangle = new Rectangle?(new Rectangle(66 * (Projectile.spriteDirection != 1 ? 1 : 0), 0, 66, 66));
        if (Projectile.ai[1] == 2 && Projectile.localAI[1] < 1f) Projectile.localAI[1] += 0.1f;
        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        Main.spriteBatch.Begin(snapshot with { blendState = BlendState.AlphaBlend }, true);

        if (_powerUp && Projectile.ai[1] != 100f) {
            for (int i = 0; i < 3; i++) {
                float circleCompletion = (float)Math.Sin((double)(Main.GlobalTimeWrappedHourly * 5f + i * MathHelper.PiOver2)) * -Projectile.spriteDirection;
                float drawRotation = 0.78f - 0.2f * i * Projectile.spriteDirection + circleCompletion * MathHelper.Pi / 10f - circleCompletion * 0.44f;
                Vector2 drawOffsetStraight = Projectile.Center - Main.screenPosition + -Projectile.spriteDirection * Vector2.One * (float)Math.Sin((double)(Main.GlobalTimeWrappedHourly * 20f)) * 0.01f;
                Vector2 drawDisplacementAngle = -Projectile.spriteDirection * Vector2.One.RotatedBy(MathHelper.PiOver2, default) * circleCompletion.ToRotationVector2().Y;
                int itemAnimationMax = player.itemAnimationMax;
                int itemAnimation = player.itemAnimation;
                float value = 1f - itemAnimation / (float)itemAnimationMax;
                Main.spriteBatch.Draw(texture2D, drawOffsetStraight + drawDisplacementAngle + Projectile.spriteDirection * Utils.ToRotationVector2(i).RotatedBy(Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(0f, 1.5f, speed: 1f - value) + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                                      rectangle,
                                      new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0) * Projectile.Opacity * ((4f - i) / 4f) * Projectile.localAI[1],
                                      Projectile.rotation + drawRotation,
                                      new Vector2(0f, texture2D.Height),
                                      Projectile.scale, SpriteEffects.None, 0f);
            }
        }
        /*for (int i = 0; i < 5; i++) {
			Main.spriteBatch.DrawSelf(texture2D, Projectile.Center + Utils.ToRotationVector2((float)(Main.GlobalTimeWrappedHourly + i * MathHelper.Pi * 2f / 5.0)) - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
								  rectangle,
								  Projectile.GetAlpha(lightColor) * 0.025f * MathUtils.Osc(0f, 1.5f, 0.5f, 0.5f) * Projectile.Opacity,
								  Projectile._rotation + extraRotation + 0.78f,
								  new Vector2(0f, texture2D.Height),
								  Projectile.scale, SpriteEffects.None, 0f);
		}*/
        Main.spriteBatch.Draw(texture2D, Projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                              rectangle,
                              Projectile.GetAlpha(lightColor) * Projectile.Opacity,
                              Projectile.rotation + extraRotation + 0.78f,
                              new Vector2(0f, texture2D.Height),
                              Projectile.scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Begin(snapshot, true);
        return false;
    }
}