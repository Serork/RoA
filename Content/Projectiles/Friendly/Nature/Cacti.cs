using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.Networking;
using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

// TODO: rewrite
sealed class Cacti : NatureProjectile {
    private static Asset<Texture2D> _trailTexture = null!;

    public enum State {
        Normal,
        Enchanted
    }

    internal State _state = State.Normal;
    private Projectile _parent = null;
    private float _useTimeFactor;

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 6);
        ProjectileID.Sets.NeedsUUID[Type] = true;

        if (Main.dedServ) {
            return;
        }

        _trailTexture = ModContent.Request<Texture2D>(Texture + "_Trail");
    }

    protected override void SafeSetDefaults() {
        Projectile.Size = 24 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write((int)_state);
        writer.Write(_useTimeFactor);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _state = (State)reader.ReadInt32();
        _useTimeFactor = reader.ReadSingle();
    }

    //public override bool? CanDamage() => Projectile.Opacity > 0.5f;
    public override bool? CanCutTiles() => _state == State.Enchanted;

    protected override void SafeOnSpawn(IEntitySource source) {
        int byUUID = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[1]);
        if (Main.projectile.IndexInRange(byUUID)) {
            _parent = Main.projectile[byUUID];
        }

        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        if (_parent == null) {
            Projectile.Kill();
            return;
        }

        Player player = Main.player[Projectile.owner];

        Projectile.ai[2] = NatureWeaponHandler.GetUseSpeed(player.GetSelectedItem(), player);
        _useTimeFactor = 0.0275f * (float)(1f - Projectile.ai[2] / (Main.player[Projectile.owner].itemTimeMax + Main.player[Projectile.owner].itemTimeMax / 6f));

        Vector2 basePosition = _parent.As<CactiCaster.CactiCasterBase>()._position;
        basePosition.Y += player.height - player.height * (3f + _useTimeFactor * player.height * 0.75f);
        Vector2 pointPosition = basePosition;
        float lastY = Math.Abs(basePosition.Y - Main.screenPosition.Y + Main.screenHeight);
        Projectile.Center = pointPosition + Vector2.UnitY * lastY;
        Vector2 dif = pointPosition - Projectile.Center;
        Projectile.velocity.X = 0f;
        float value = _useTimeFactor;
        Projectile.velocity.Y = -dif.Length() * (0.04975f + value);
        Projectile.ai[0] = Main.player[Projectile.owner].direction;

        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_state == State.Enchanted) {
            Texture2D texture = Projectile.GetTexture(),
                      trailTexture = _trailTexture.Value;
            Vector2 origin = texture.Size() / 2f;
            Color color = lightColor;
            int length = Projectile.oldPos.Length;
            float baseRotation = (Projectile.velocity.SafeNormalize(Vector2.One) * 2f).ToRotation() - MathHelper.PiOver2;
            for (int i = length - 2; i > 0; i--) {
                float progress = (length - i) / (float)length * 1.25f;
                color *= Utils.Remap(progress, 0f, 1f, 0.5f, 1f);
                color *= 1.125f;
                float scale = Projectile.scale * Math.Clamp(progress, 0.5f, 1f);
                float offsetYBetween = Projectile.Size.Y * 0.2f * scale;
                if (i == 4) {
                    offsetYBetween *= 0.875f;
                }
                Vector2 last = Projectile.position;
                Vector2 dif = (last - Projectile.oldPos[i]).SafeNormalize(Vector2.UnitY);
                Main.EntitySpriteDraw(trailTexture,
                                      Projectile.oldPos[i] + (dif * offsetYBetween / 2f + origin - dif * offsetYBetween * i) - Main.screenPosition,
                                      null,
                                      color * scale,
                                      baseRotation,
                                      origin,
                                      scale,
                                      default);
            }

            int offset = 1, size = offset * 2;
            for (int k = 0; k < 2; k++) {
                for (int i = -size; i <= size; i += offset) {
                    for (int j = -size; j <= size; j += offset) {
                        if (Math.Abs(i) + Math.Abs(j) == size) {
                            Main.EntitySpriteDraw(trailTexture, Projectile.Center + new Vector2(i, j) * 2f - Main.screenPosition,
                                                  null,
                                                  color * 0.325f,
                                                  baseRotation + Projectile.velocity.X * 0.05f,
                                                  origin,
                                                  Projectile.scale,
                                                  default);
                        }
                    }
                }
            }
        }

        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (_state == State.Enchanted) {
            Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.Opacity < 0.75f) {
            return;
        }

        if (Projectile.owner == Main.myPlayer) {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(player.GetSource_ItemUse(player.GetSelectedItem()), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CactiExplosion>(), Projectile.damage, Projectile.knockBack * 1.5f, Projectile.owner);
        }

        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves2") { Volume = 0.3f, Pitch = -0.75f });

        if (!Main.dedServ) {
            for (int i = 0; i < 2; i++)
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/CactiGore").Type, 1f);
        }

        for (int num559 = 0; num559 < 10; num559++) {
            int num560 = Dust.NewDust(Projectile.Center - Projectile.velocity, Projectile.width, Projectile.height, DustID.OasisCactus, Alpha: Main.rand.Next(80));
            Dust dust2 = Main.dust[num560];
            dust2.noGravity = Main.rand.NextBool();
            dust2.noLight = true;
            if (Main.rand.NextBool(2)) {
                dust2.scale *= 1.2f;
            }
        }

        for (float num17 = 0f; num17 < 1f; num17 += 0.05f) {
            bool flag = Main.rand.NextBool(2);
            Dust dust8 = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * Projectile.Size.Y + Main.rand.NextVector2Circular(80, 40f) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitY) * num17, flag ? ModContent.DustType<CactiCasterDust>() : DustID.OasisCactus, Main.rand.NextVector2Circular(3f, 3f), Scale: Main.rand.NextFloat(1.25f, 1.5f));
            dust8.velocity.Y -= 4f;
            dust8.noGravity = true;
            dust8.noLight = true;
            Dust dust2 = dust8;
            dust2.velocity += Projectile.velocity * 0.2f;
            dust2.velocity *= 1.01f;
        }
    }

    public override void AI() {
        Projectile.Opacity = Utils.GetLerpValue(180, 155, Projectile.timeLeft, true);

        Projectile.tileCollide = _state == State.Enchanted;

        float distY = (Main.player[Projectile.owner].position.Y - Projectile.position.Y);
        if (distY > 600f) {
            Projectile.Kill();
            return;
        }

        switch (_state) {
            case State.Normal:
                int baseType = ModContent.ProjectileType<CactiCaster.CactiCasterBase>();
                while (_parent != null && _parent.type != baseType) {
                    _parent = Main.projectile.FirstOrDefault(x => x.active && x.owner == Projectile.owner && x.type == baseType);
                }
                bool flag = _parent == null || (!_parent.active && _parent.ModProjectile != null && _parent.As<CactiCaster.CactiCasterBase>() != null);
                Vector2 corePosition = flag ? Main.player[Projectile.owner].Center : _parent.As<CactiCaster.CactiCasterBase>().CorePosition - Vector2.One * 1f + _parent.As<CactiCaster.CactiCasterBase>().GetCorePositionOffset(new Vector2(0.15f, Projectile.GetOwnerAsPlayer().direction < 0 ? -0.9f : -1.075f));

                if (Main.rand.NextBool(2)) {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.OasisCactus, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 80, default, 1.4f + Main.rand.NextFloat(0f, 0.075f));
                    Main.dust[dust].noGravity = true;
                }

                if (Projectile.velocity.Length() < 10f && Projectile.localAI[0] == 0f) {
                    SoundEngine.PlaySound(SoundID.Item20, corePosition);

                    Projectile.localAI[0] = 1f;

                    if (!flag) {
                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new CactiCasterDustsPacket(Projectile.GetOwnerAsPlayer(), corePosition));
                        }
                        for (int i = 0; i < 15; i++) {
                            int dust = Dust.NewDust(corePosition, 4, 4, ModContent.DustType<CactiCasterDust>(), Main.rand.Next(-50, 51) * 0.05f, Main.rand.Next(-50, 51) * 0.05f, 0, default, 1.5f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                        }
                    }
                }

                Projectile.velocity.Y *= 0.95f - _useTimeFactor;
                Projectile.rotation = Projectile.velocity.Y * Projectile.ai[0];
                if (Math.Abs(Projectile.velocity.Y) <= 0.5f) {
                    Player player = Main.player[Projectile.owner];
                    if (player.whoAmI == Main.myPlayer) {
                        _state = State.Enchanted;

                        SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);

                        Vector2 mousePoint = player.GetViableMousePosition();
                        float speed = MathHelper.Clamp((mousePoint - Projectile.Center).Length() * 0.0375f, 10.5f, 12f);
                        Projectile.velocity = Helper.VelocityToPoint(Projectile.Center, mousePoint, speed);
                        Projectile.netUpdate = true;
                    }

                    for (int i = 0; i < 20; i++) {
                        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CactiCasterDust>(), 0f, -2f, 0, default, 1.5f + Main.rand.NextFloatRange(0.1f));
                        Main.dust[dust].position.X += Main.rand.Next(-50, 51) * 0.05f - 1.5f;
                        Main.dust[dust].position.Y += Main.rand.Next(-50, 51) * 0.05f - 1.5f;
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;

                        if (Main.dust[dust].position != Projectile.Center) {
                            Main.dust[dust].velocity = Projectile.DirectionTo(Main.dust[dust].position) * 2f;
                        }
                    }
                }
                break;
            case State.Enchanted:
                float baseRotation = (Projectile.velocity.SafeNormalize(Vector2.One) * 2f).ToRotation() - MathHelper.PiOver2;
                Projectile.rotation = baseRotation + Projectile.localAI[2] * Projectile.localAI[1];
                if (Main.rand.NextBool(5)) {
                    Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, ModContent.DustType<CactiCasterDust>(), 0f, 0f, 0, default(Color), Main.rand.NextFloat(0.8f, 1f) * 1.4f * 0.95f);
                    dust2.noGravity = true;
                    dust2.noLight = true;
                    dust2.velocity = Projectile.velocity * 0.5f;
                    dust2.fadeIn = 1f;
                }
                Projectile.localAI[1] = Helper.Approach(Projectile.localAI[1], 1.05f, 0.075f);
                Projectile.localAI[2] += Projectile.velocity.X * 0.0075f;
                for (int i = 0; i < 2; i++) {
                    int direction = i != 0 ? 1 : -1;
                    Vector2 vector32 = new(Projectile.Size.X * 0.4f * direction, Projectile.Size.Y * 0.15f);
                    vector32 = vector32.RotatedBy(baseRotation);
                    int type = Dust.NewDust(Projectile.Center - Vector2.One * 4f + Projectile.velocity + vector32, 4, 4, ModContent.DustType<CactiCasterDust>());
                    Dust dust = Main.dust[type];
                    dust.scale = Main.rand.NextFloat(0.8f, 1f) * 1.4f;
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.velocity = (dust.velocity * 0.25f + Vector2.Normalize(vector32)).SafeNormalize(new Vector2(10f * direction, -1f)) * Main.rand.NextFloat(1f, 1.5f) * 3f;
                    dust.velocity -= new Vector2(3f * direction, 5f).RotatedBy(baseRotation);
                    dust.fadeIn = 1f;
                    dust.scale *= 0.95f;
                }
                break;
        }
    }

    private class CactiExplosion : NatureProjectile {
        public override string Texture => ResourceManager.EmptyTexture;

        public override bool PreDraw(ref Color lightColor) => false;

        protected override void SafeSetDefaults() {
            Projectile.Size = new Vector2(125, 75);
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.timeLeft = 10;
        }
    }
}