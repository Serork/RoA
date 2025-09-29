using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Claws;
using RoA.Common.Druid.Wreath;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.VisualEffects;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HellfireClawsSlash : ClawsSlash {
    private const int MAX = 14;

    private int _hitTimer, _oldTimeleft;
    private int _oldItemUse, _oldItemAnimation;
    private int _hitAmount;
    private float _knockBack;
    private float[] oldRot = new float[MAX];
    private bool _hit;
    private int _proj = -1;

    public bool Charged { get; private set; } = true;

    protected override bool SpawnSlashDust => false;

    private bool Hit => _hitTimer > 0;

    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<ClawsSlash>()).Texture;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        ProjectileID.Sets.TrailCacheLength[Type] = MAX;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    protected override void SafeSetDefaults() {
        base.SafeSetDefaults();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        base.OnHitPlayer(target, info);

        if (!Charged) {
            return;
        }

        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        target.AddBuff(BuffID.OnFire, (int)(60f * num2 * 2f));
    }

    private void UpdateOldInfo() {
        for (int num28 = oldRot.Length - 1; num28 > 0; num28--) {
            oldRot[num28] = oldRot[num28 - 1];
        }

        oldRot[0] = Projectile.rotation;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Projectile.localNPCHitCooldown = -2;
        Projectile.usesOwnerMeleeHitCD = false;
        _knockBack = Projectile.knockBack;

        Update((MathHelper.PiOver2 / 2f + MathHelper.PiOver4 * 0.5f) * Projectile.ai[0]);
        UpdateOldInfo();
        for (int num28 = oldRot.Length - 1; num28 > 0; num28--) {
            if (oldRot[num28] == 0f) {
                oldRot[num28] = Projectile.rotation - 0.1f * num28;
            }
        }
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.Write(_hitTimer);
        writer.Write(_oldTimeleft);
        writer.Write(_oldItemUse);
        writer.Write(_oldItemAnimation);
        writer.Write(_hit);
        writer.Write(_proj);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _hitTimer = reader.ReadInt32();
        _oldTimeleft = reader.ReadInt32();
        _oldItemUse = reader.ReadInt32();
        _oldItemAnimation = reader.ReadInt32();
        _hit = reader.ReadBoolean();
        _proj = reader.ReadInt32();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        if (!Charged) {
            return;
        }
        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        target.AddBuff(BuffID.OnFire, (int)(60f * num2 * 2f));
        _hitAmount++;
        target.immune[Projectile.owner] = 0;
        Projectile.localNPCImmunity[target.whoAmI] = 10 + _hitAmount;
        Projectile.localAI[2] += 0.1f * Projectile.ai[0];
        if (!Hit) {
            _hit = true;
            Main.player[Projectile.owner].GetWreathHandler().HandleOnHitNPCForNatureProjectile(Projectile, true);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "HellfireClaws") with { PitchVariance = 0.25f, Volume = Main.rand.NextFloat(0.75f, 0.85f) }, GetPos());
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayHellfireSoundPacket(Main.player[Projectile.owner], GetPos()));
            }
            for (int i = 0; i < 25; i++) {
                if (Main.rand.NextBool(3)) {
                    Vector2 pos = GetPos(MathHelper.PiOver4 * 0.5f);
                    Vector2 to = pos.DirectionTo(GetPos(MathHelper.PiOver4 * 0.6f));
                    Dust dust = Dust.NewDustPerfect(GetPos(MathHelper.PiOver4 * 0.5f), 6, -to.RotatedBy(Main.rand.NextFloatRange(0.275f)) * Main.rand.NextFloat(3f, 6f) * Main.rand.NextFloat(0.75f, 1f), 0, default, 2.25f + Main.rand.NextFloatRange(0.25f));
                    dust.customData = 0;
                    //dust.noGravity = true;
                }
            }
            if (Projectile.localAI[1] == 0f) {
                Projectile.localAI[1] = 1f;
                if (_proj == -1 && Projectile.owner == Main.myPlayer) {
                    _proj = Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target),
                        GetPos(MathHelper.PiOver4 * 0.5f),
                        Helper.VelocityToPoint(Main.player[Projectile.owner].Center, Main.MouseWorld, 1f),
                        ModContent.ProjectileType<HellfireFracture>(), Projectile.damage, 0f, Projectile.owner, ai2: Projectile.identity).identity;
                }
            }
            if (Projectile.owner == Main.myPlayer) {
                _oldTimeleft = Projectile.timeLeft;
                _hitTimer = 10;
                _oldItemUse = Owner.itemTime;
                _oldItemAnimation = Owner.itemAnimation;
                Projectile.netUpdate = true;
            }
            //UpdateMainCycle();
        }
    }

    public Vector2 GetPos(float extraRot = 0f) {
        float rot = Projectile.rotation + (float)(Projectile.ai[0] * extraRot);
        float num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        float offset = Owner.gravDir == 1 ? 0f : (-MathHelper.PiOver4 * num1);
        float f = rot + (float)(Projectile.ai[0] * MathHelper.PiOver2 * 0f);
        Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)(50.0 * Projectile.scale + 20.0 * Projectile.scale);
        return position + (rot + Utils.Remap(num1, 0f, 1f, 0f, (float)Math.PI / 2f) * Projectile.ai[0]).ToRotationVector2() * num;
    }

    public override bool PreDraw(ref Color lightColor) {
        for (int index = 0; index < 10; index += 2) {
            DrawItself(ref lightColor, oldRot[index]);
        }
        Texture2D value = TextureAssets.Extra[98].Value;
        float fromValue = Ease.QuintIn(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
        color1.A = 50;
        color2.A = 50;
        Color shineColor = new(255, 200, 150);
        Microsoft.Xna.Framework.Color color = color1 * Projectile.Opacity;
        Vector2 origin = value.Size() / 2f;
        color2 = color2;
        float num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        Vector2 scale = new Vector2(2f, 2f);
        Vector2 fatness = Vector2.One;
        Vector2 vector = new Vector2(fatness.X * 0.5f, scale.X) * num;
        Vector2 vector2 = new Vector2(fatness.Y * 0.5f, scale.Y) * num;
        color *= num * 1.5f;
        color2 *= num * 1.5f;
        SpriteEffects dir = Projectile.ai[0] >= 0.0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        Vector2 drawpos = GetPos();
        float rot = Projectile.rotation + MathHelper.PiOver4 * Projectile.ai[0] + num1 + Projectile.localAI[2];
        Main.EntitySpriteDraw(value, drawpos - Main.screenPosition, null, color, (float)Math.PI / 2f + rot, origin, vector, dir);
        Main.EntitySpriteDraw(value, drawpos - Main.screenPosition, null, color, 0f + rot, origin, vector2, dir);
        Main.EntitySpriteDraw(value, drawpos - Main.screenPosition, null, color2, (float)Math.PI / 2f + rot, origin, vector * 0.6f, dir);
        Main.EntitySpriteDraw(value, drawpos - Main.screenPosition, null, color2, 0f + rot, origin, vector2 * 0.6f, dir);
        return false;
    }

    protected override void UpdateMainCycle() {
        if (!Hit) {
            Projectile.localAI[0] += 1f;
            Update((MathHelper.PiOver2 / 2f + MathHelper.PiOver4 * 0.5f) * Projectile.ai[0]);
            UpdateOldInfo();
        }
    }

    public override void SafePostAI() {
        if (_hit) {
            _hit = false;
            Projectile projectile = Main.projectile[_proj];
            if (projectile != null && projectile.active && projectile.type == ModContent.ProjectileType<HellfireFracture>() && projectile.ai[0] < 5f) {
                if (Projectile.localAI[0] < Projectile.ai[1] * 1.2f) {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] += 1.5f;
                    projectile.Opacity = 0f;
                    projectile.netUpdate = true;
                }
            }
        }

        if (Projectile.localAI[0] >= Projectile.ai[1] * 0.3f && Projectile.localAI[0] < Projectile.ai[1] * 1.45f) {
            //for (int index = 0; index < AttackTime; index += 2) {
            //    int index2 = Math.Max(0, index - 2);
            //if (oldRot[index2] != 0f) {
            for (int i = 0; i < 2; i++) {
                float spriteWidth = 15, spriteHeight = spriteWidth;
                float num = (float)Math.Sqrt(spriteWidth * spriteWidth + spriteHeight * spriteHeight);
                float normalizedPointOnPath = 0.2f + 0.8f * Main.rand.NextFloat();
                float rotation = Projectile.rotation + MathHelper.PiOver4 / 2f * Projectile.ai[0]; /*+ MathHelper.PiOver4 * Projectile.ai[0];*/
                if (Projectile.ai[0] == -1) {
                    rotation += MathHelper.PiOver4 / 2f;
                }
                Vector2 outwardDirection = rotation.ToRotationVector2().RotatedBy(3.926991f * Projectile.ai[0]);
                float itemScale = Projectile.scale;
                Vector2 location = Owner.RotatedRelativePoint(Projectile.Center + outwardDirection * num * normalizedPointOnPath * itemScale);
                Vector2 vector = outwardDirection.RotatedBy((float)Math.PI / 2f * (float)Projectile.ai[0] * Owner.gravDir);
                float f = rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
                Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
                float num1 = Projectile.ai[0];
                float offset = Owner.gravDir == 1 ? 0f : (-MathHelper.PiOver4 * num1);
                int offsetY = 0;
                for (float i2 = -MathHelper.PiOver4; i2 <= MathHelper.PiOver4; i2 += MathHelper.PiOver2) {
                    Rectangle rectangle = Utils.CenteredRectangle((rotation * Owner.gravDir + i2).ToRotationVector2() * 35f * Projectile.scale, new Vector2(35f * Projectile.scale, 35f * Projectile.scale));
                    location = location + Main.rand.NextVector2FromRectangle(rectangle) + Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale;
                    offsetY += Main.rand.Next(-1, 2);
                    if (offsetY > 5) {
                        offsetY = 5;
                    }
                    if (offsetY < -5) {
                        offsetY = -5;
                    }
                    if (location.Distance(Owner.Center) > 37.5f + offsetY) {
                        //if (Projectile.localAI[0] % 11 == 0) {
                        Dust dust = Dust.NewDustPerfect(location, 6, vector * 4.5f * Main.rand.NextFloat() /*- new Vector2?(rotationVector2 * Owner.gravDir) * 4f*/, 100, default(Color), 2.5f + Main.rand.NextFloatRange(0.25f));
                        dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                        dust.noGravity = true;
                        dust.scale *= Projectile.scale;
                        //}
                    }
                }
                //}
            }
            //}
        }
        Projectile.localAI[2] += 0.0125f * Projectile.ai[0];
        ClawsHandler clawsStats = Owner.GetModPlayer<ClawsHandler>();
        float fromValue = Helper.EaseInOut3(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6) * 0.5f, fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4) * 0.5f, fromValue);
        clawsStats.SetColors(color1, color2);

        if (Hit) {
            Projectile.knockBack = 0f;
            _hitTimer--;
            if (_hitTimer <= 0) {
                for (int i = 0; i < 200; i++) {
                    if (!Main.npc[i].active)
                        continue;

                    bool flag5 = Projectile.usesLocalNPCImmunity && Projectile.localNPCImmunity[i] == 0;
                    if (!((!Main.npc[i].dontTakeDamage || NPCID.Sets.ZappingJellyfish[Main.npc[i].type]) && flag5))
                        continue;

                    Projectile.localNPCImmunity[i] = 0;
                }
            }
            Projectile.timeLeft = _oldTimeleft;
            Owner.itemTime = _oldItemUse;
            Owner.itemAnimation = _oldItemAnimation;
        }
        else {
            Projectile.knockBack = _knockBack;
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.Knockback *= 0.5f;
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
        float fromValue = Ease.QuintIn(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
        float angle = MathHelper.PiOver2;
        Vector2 offset = new(0.2f);
        Vector2 velocity = 1.5f * offset;
        Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
        Color color = Color.Lerp(color1, color2, Main.rand.NextFloat())/*Lighting.GetColor(target.Center.ToTileCoordinates()).MultiplyRGB(DrawColor.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat()))*/;
        color.A = 50;
        position = target.Center + target.velocity + position + Main.rand.NextVector2Circular(target.width / 3f, target.height / 3f);
        velocity = angle.ToRotationVector2() * velocity * 0.5f;
        int layer = VisualEffectLayer.ABOVENPCS;
        VisualEffectSystem.New<ClawsSlashHit>(layer)?.Setup(position,
                  velocity,
                  color);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.ClawsHit, Owner, layer, position, velocity, color, 1f, 0f));
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.Knockback *= 0.5f;
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
        float fromValue = Ease.QuintIn(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
        float angle = MathHelper.PiOver2;
        Vector2 offset = new(0.2f);
        Vector2 velocity = 1.5f * offset;
        Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
        Color color = Color.Lerp(color1, color2, Main.rand.NextFloat())/*Lighting.GetColor(target.Center.ToTileCoordinates()).MultiplyRGB(DrawColor.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat()))*/;
        color.A = 50;
        position = target.Center + target.velocity + position + Main.rand.NextVector2Circular(target.width / 3f, target.height / 3f);
        velocity = angle.ToRotationVector2() * velocity * 0.5f;
        int layer = VisualEffectLayer.ABOVENPCS;
        VisualEffectSystem.New<ClawsSlashHit>(layer)?.Setup(position,
                  velocity,
                  color);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.ClawsHit, Owner, layer, position, velocity, color, 1f, 0f));
        }
    }
}
