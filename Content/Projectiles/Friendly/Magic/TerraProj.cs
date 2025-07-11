using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

// TODO: rewrite
sealed class TerraProj : ModProjectile {
    private float currentCharge, hittingTimer;
    private bool maxCharged, hitting;
    private float _distance;
    private float _hitTime;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(hittingTimer);
        writer.Write(hitting);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        hittingTimer = reader.ReadSingle();
        hitting = reader.ReadBoolean();
    }

    public override Color? GetAlpha(Color lightColor) => Color.White;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults() {
        int width = 2; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        //Projectile.hide = true;

        Projectile.aiStyle = -1;

        hitting = false;
        currentCharge = 0;
        maxCharged = false;

        Projectile.ignoreWater = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player _player = Main.player[Projectile.owner];
        bool flag = _player.direction != 1;
        Item _item = _player.HeldItem;
        Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra_OnUse");
        Vector2 _origin = new(_texture.Width * 0.5f * (1 - _player.direction), (_player.gravDir == -1f) ? 0 : _texture.Height);
        int x = -(int)_origin.X;
        ItemLoader.HoldoutOrigin(_player, ref _origin);
        Vector2 _offset = new(_origin.X + x, 0);
        float _rotOffset = 0.785f * _player.direction;
        if (_player.gravDir == -1f)
            _rotOffset -= 1.57f * _player.direction;
        SpriteEffects effects = flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (_player.gravDir == -1f) {
            if (_player.direction == 1) {
                effects = SpriteEffects.FlipVertically;
            }
            else {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
        }
        SpriteBatch _spriteBatch = Main.spriteBatch;
        _spriteBatch.Draw(_texture, Projectile.position - Main.screenPosition + _offset, _texture.Bounds, lightColor, _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
        _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra2_Glow");
        float maxValue = Projectile.ai[1] / 110f;
        float _k = 1f - (maxValue > 0.3f ? 0.3f : 0f) - (maxValue > 0.6f ? 0.3f : 0f) - (maxValue > 0.9f ? 0.3f : 0f);
        float value = Projectile.ai[1] / 45f;
        float _rate = (float)(value % _k);
        //Color _lightColor = Color.Lerp(Color.DarkGreen, Color.White, 1f - (_rate - 0.5f) / 0.5f);
        Color _lightColor = Color.White;
        //if (_lightColor == Color.White && Projectile.ai[1] <= 170f && Projectile.localAI[1] <= 0f)
        //	SoundEngine.PlaySound(SoundID.Item43.WithVolumeScale(0.15f), Projectile.position);
        _spriteBatch.Draw(_texture, Projectile.position - Main.screenPosition + _offset, _texture.Bounds,
            Color.Lerp(Color.White, lightColor, Lighting.Brightness((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16)), _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
        if (maxCharged)
            DrawLaser(_spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), Projectile.position, Projectile.velocity, 5, Projectile.damage, -1.57f, 1f, 1000f, _lightColor, 78);
        _spriteBatch.BeginBlendState(BlendState.AlphaBlend, SamplerState.AnisotropicClamp);
        if (maxCharged && _player.gravDir != -1f)
            DrawLaser(_spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), Projectile.position, Projectile.velocity, 5, Projectile.damage, -1.57f, 1f, 1000f, _lightColor * 0.3f, 78);
        _spriteBatch.EndBlendState();
        //_texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra3_Glow");
        //_spriteBatch.Draw(_texture, _player.itemLocation - Main.screenPosition + _offset + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, _texture.Bounds, !maxCharged ? _lightColor : Color.White, _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
        return false;
    }

    private void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default, int transDist = 50) {
        if (Projectile.ai[2] < 5f) {
            return;
        }

        float value = (float)Math.Pow(Utils.GetLerpValue(5f, 25f, Projectile.localAI[1], true), 0.5f) * Utils.GetLerpValue(currentCharge, currentCharge - 40f, Projectile.localAI[1], true);
        float value1 = (float)Math.Pow(value, 0.5f);
        Vector2 drawScale = new(scale * value1, scale);
        start += Projectile.velocity.SafeNormalize(Vector2.Zero) * -4f;
        for (float i = transDist; i <= Projectile.ai[0] - 5f; i += step) {
            Color c = Color.White;
            Vector2 _origin = start + i * unit;
            spriteBatch.Draw(texture, _origin - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(c.R, c.G, c.B, 200), unit.ToRotation() + rotation, new Vector2(7f, 20f), drawScale, 0, 0);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player player = Main.player[Projectile.owner];
        Vector2 position = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 42f;
        float _ = 0f;
        bool flag = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, position + Projectile.velocity + Vector2.Normalize(Projectile.velocity) * (Projectile.ai[0] - 50f), 2, ref _);

        //if (flag) {
        //          Projectile.ai[0] = Vector2.Distance(targetHitbox.Center(), Projectile.position) * 0.935f;
        //          hittingTimer = 5f + (int)(Projectile.localAI[1] / 10f);
        //          hitting = true;

        //          Projectile.netUpdate = true;
        //          Projectile.localAI[2] = 1f;
        //      }
        //else {
        //          Projectile.localAI[2] = 0f;
        //      }
        //      if (!flag && hittingTimer <= 0f)
        //          hitting = false;

        return flag;
    }

    public override bool? CanDamage() => maxCharged && Projectile.ai[2] >= 5f && Projectile.localAI[1] <= currentCharge - 10f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.ai[2] < 5f) {
            return;
        }

        //Projectile.damage = Main.player[Projectile.owner].HeldItem.damage / 5 + (int)(float)(Projectile.damage * 0.9f);
        target.immune[Projectile.owner] = 3 + (int)(Projectile.localAI[1] / 10f);
        _hitTime = target.immune[Projectile.owner];
    }

    //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
    //	damage = Main.player[Projectile.owner].HeldItem.damage + (int)(currentCharge - Projectile.localAI[1]) / 5;
    //}

    public override void AI() {
        if (Projectile.ai[1] < 0f) Projectile.ai[1] = 0f;
        if (hittingTimer > 0f) hittingTimer--;
        else {
            hitting = false;
        }
        if (Projectile.ownerHitCheck) {
            hittingTimer = 5f + (float)(Projectile.localAI[1] / 10f);
            hitting = true;
        }

        Player player = Main.player[Projectile.owner];
        int dir = Projectile.direction;
        player.ChangeDir(dir);
        if (player.gravDir != -1f) {
            player.heldProj = Projectile.whoAmI;
        }
        player.itemAnimation = player.itemTime = 2;

        Vector2 mousePosition = player.GetViableMousePosition();
        Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra_OnUse");
        Vector2 _origin = new(_texture.Width * 0.5f * (1 - player.direction), (player.gravDir == -1f) ? 0 : _texture.Height);
        int _x = -(int)_origin.X;
        ItemLoader.HoldoutOrigin(player, ref _origin);
        Vector2 _offset = new(_origin.X + _x, 0);
        Vector2 pos = player.RotatedRelativePoint(player.MountedCenter, true);
        if (player.direction == -1) {
            pos.X += 1f;
        }
        pos += (player.itemRotation.ToRotationVector2() * 6f * player.direction).Floor();
        //if (player.gravDir == -1f) {
        //    float num = player.position.Y - player.itemLocation.Y;
        //    pos.Y = player.Bottom.Y + num;
        //}
        Vector2 _position = pos + _offset + new Vector2(0f, 6f) * player.gravDir;
        Lighting.AddLight(_position, 73f / 255f * (Projectile.ai[1] / 300f), 170f / 255f * (Projectile.ai[1] / 300f), 104f / 255f * (Projectile.ai[1] / 300f));
        float value = Utils.GetLerpValue(0f, 50f, Projectile.localAI[1], true) * Utils.GetLerpValue(currentCharge, currentCharge - 40f, Projectile.localAI[1], true);
        float value1 = (float)Math.Pow(value, 0.5f);
        if (maxCharged && Projectile.ai[2] >= 10f) {
            if (Projectile.soundDelay <= 0) {
                Projectile.soundDelay = 15;
                Projectile.soundDelay *= 2;

                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Quake") { Volume = 0.7f, PitchVariance = 0.3f });
            }
            Vector2 spinningpoint4 = Vector2.UnitX * 14f;
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            spinningpoint4 = spinningpoint4.RotatedBy(rotation - (float)Math.PI / 2f);
            Vector2 position = player.Center + Vector2.Normalize(Projectile.velocity) * 36f + new Vector2(player.direction == 1 ? 4f : -2f, 1f * player.direction).RotatedBy(Projectile.velocity.ToRotation());
            Vector2 vector13 = position + spinningpoint4;
            for (int l = 0; l < 2; l++) {
                if (Main.rand.NextBool()) {
                    int num26 = ModContent.DustType<NatureLaser>();
                    float num27 = 0.35f;
                    if (l % 2 == 1) {
                        num27 = 0.45f;
                    }
                    num27 *= 1.5f;
                    num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                    num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                    num27 *= 1.01f;

                    float num28 = Main.rand.NextFloatDirection();
                    Vector2 vector14 = vector13 + (rotation + num28 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
                    int num29 = 18;
                    int num30 = Dust.NewDust(vector14 - Vector2.One * (num29 / 2) - new Vector2(4f + (player.direction == -1 ? 4f : -4f), -3f * player.gravDir), num29, num29, num26, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
                    Main.dust[num30].velocity = (vector14 - vector13).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num28), clamped: true)) * 0.5f * value1;
                    Main.dust[num30].noGravity = true;
                    Main.dust[num30].scale = num27;
                    Main.dust[num30].fadeIn = 0.5f;
                }
            }
            for (int i = 0; i < 2; i++) {
                if (Main.rand.NextBool()) {
                    float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 1.82);
                    float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
                    Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
                    var _dust = Dust.NewDust(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 3.3f);
                    Lighting.AddLight(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 73f / 255f, 170f / 255f, 104f / 255f);
                    Main.dust[_dust].scale = 1f;
                    Main.dust[_dust].noGravity = true;
                    Main.dust[_dust].fadeIn = 0.25f;
                    if (Projectile.scale != 1.4) {
                        Dust dust = Dust.CloneDust(_dust);
                        dust.scale /= 2f;
                    }
                    _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 3.14);
                    _spawnY = (float)(Main.rand.NextDouble() * 10.0 + 1.0);
                    _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
                    _dust = Dust.NewDust(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 3.3f);
                    Main.dust[_dust].scale = 1f;
                    Main.dust[_dust].noGravity = true;
                    Main.dust[_dust].fadeIn = 0.15f;
                    if (Projectile.scale > 1.0) {
                        Main.dust[_dust].velocity *= Projectile.scale;
                        Main.dust[_dust].scale *= Projectile.scale;
                    }
                }
            }
        }
        else if (Projectile.owner == Main.myPlayer) {
            Vector2 _diff = mousePosition - _position;
            _diff.Normalize();
            Projectile.velocity = _diff;
            Projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
            Projectile.netUpdate = true;
        }

        Projectile.Center = _position;
        Projectile.Center = Utils.Floor(Projectile.Center);
        //Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians((Projectile.ai[1] / 300f) * 0.35f) * (1f - Projectile.localAI[1] / (currentCharge + 0.0001f)));
        Projectile.timeLeft = 2;
        player.itemTime = 2;
        player.itemAnimation = 2;
        player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * dir, Projectile.velocity.X * dir);
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

        int _delay = 5 + (int)(Projectile.localAI[1] / 10f);
        if (player.noItems || !player.IsAliveAndFree() || player.itemAnimation <= 0)
            Projectile.Kill();
        else {
            if (!maxCharged) {
                Vector2 spinningpoint3 = Vector2.UnitX * 18f;
                spinningpoint3 = spinningpoint3.RotatedBy(Projectile.rotation - (float)Math.PI / 2f);
                Vector2 position = player.Center + Vector2.Normalize(Projectile.velocity) * 56f + new Vector2(0f, player.gravDir == -1f ? -8f : 3f) + new Vector2(-10f, 11f);
                Vector2 vector11 = position + spinningpoint3;
                int count = Math.Min(6, (int)(Projectile.ai[1] / 15f));
                for (int k = 0; k < count + 1; k++) {
                    int num23 = ModContent.DustType<NatureLaser>();
                    float num24 = 0.4f;
                    if (k % 2 == 1) {
                        num24 = 0.65f;
                    }
                    num24 *= 1.1f;

                    Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(count * 2));
                    int num25 = Dust.NewDust(vector12, 16, 16, num23, 0f, 0f);
                    Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)count * 2f) / 10f;
                    Main.dust[num25].noGravity = true;
                    Main.dust[num25].scale = num24;
                    //Main.dust[num25].customData = player;
                }
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] % 10 < 1 && !player.CheckMana(3, true))
                    maxCharged = true;
                if (Projectile.ai[1] >= 130f)
                    maxCharged = true;
                else if (Projectile.ai[1] >= 100f && Projectile.ai[1] < 105f) {
                    if (Projectile.ai[1] >= 103f) {
                        if (Projectile.localAI[1] == 0f) {
                            Projectile.localAI[1] = 1f;
                            SoundEngine.PlaySound(SoundID.Item43, Projectile.position);
                        }
                    }
                    for (int i = 0; i < 5; i++) {
                        float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 0.1f);
                        float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
                        Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
                        var _dust = Dust.NewDust(_position + Projectile.velocity * 46f - new Vector2(4f + (player.direction == -1 ? 4f : 0f), 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 2.5f);
                        Main.dust[_dust].scale = 1.2f;
                        Main.dust[_dust].noGravity = true;
                    }
                }
                currentCharge = Projectile.ai[1];
                Projectile.localAI[0] = 1f;
            }
            else if (Projectile.localAI[0] == 1f) {
                maxCharged = true;
                if (Projectile.ai[1] < 100f) {
                    if (Projectile.localAI[1] == 0f) {
                        Projectile.localAI[1] = 1f;
                        SoundEngine.PlaySound(SoundID.Item43, Projectile.position);
                        int count = 3 + (int)(3 * Projectile.ai[1] / 100f);
                        for (int i = 0; i < count; i++) {
                            float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 0.1f);
                            float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
                            Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
                            var _dust = Dust.NewDust(_position + Projectile.velocity * 44f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 2.5f);
                            Main.dust[_dust].scale = 1.2f;
                            Main.dust[_dust].noGravity = true;
                        }
                    }
                }
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] >= currentCharge) {
                    //player.channel = false;
                    Projectile.Kill();
                }
            }
        }
        if (maxCharged)
            if (Projectile.ai[2] < 10f) {
                Projectile.ai[2]++;
            }
            else {
                for (int i = 78; i < Projectile.ai[0]; i++) {
                    if (Main.rand.NextChance(0.005)) {
                        Vector2 _velocity = Utils.RotatedBy(Projectile.velocity, 0d, default) * value1;
                        Vector2 _position2 = _position + i * Projectile.velocity + (player.direction == 1 ? -new Vector2(4f, 4f) : new Vector2(4f, -4f));
                        var _dust = Dust.NewDust(_position2, 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X, _velocity.Y, 0, Color.White, 2.3f);
                        Main.dust[_dust].scale = 1f;
                        Main.dust[_dust].noGravity = true;
                        Lighting.AddLight(_position2, 73f / 255f, 170f / 255f, 104f / 255f);
                    }
                }
            }

        //if (!hitting)
        //	for (Projectile.ai[0] = 100f; Projectile.ai[0] <= 2000f; Projectile.ai[0] += 8f) {
        //		Vector2 checkedPos = Projectile.position + Projectile.velocity * Projectile.ai[0] + Projectile.velocity * 24f;
        //		if (WorldGen.SolidOrSlopedTile((int)checkedPos.X / 16, (int)checkedPos.Y / 16)) {
        //			Projectile.ai[0] += 8f;
        //			break;
        //		}
        //	}
    }

    public override void PostAI() {
        if (_hitTime > 0f) {
            _hitTime--;
        }
        Vector2 samplingPoint = Main.player[Projectile.owner].Center;
        float distance = 0f;
        while (distance < 2000f) {
            Vector2 start = Projectile.position + Projectile.velocity * distance;
            NPC[] sortedNPC = Main.npc.Where(n => n.active && !n.friendly && !n.CountsAsACritter).OrderBy(n => (n.Center - start).Length()).ToArray();
            bool flag = false;
            for (int index = 0; index < sortedNPC.Length; index++) {
                NPC npc = sortedNPC[index];
                if (Collision.CheckAABBvAABBCollision(start, Vector2.One * 6f, npc.Hitbox.TopLeft(), npc.Hitbox.Size())) {
                    distance = npc.Distance(Main.player[Projectile.owner].Center);
                    flag = true;
                }
            }
            if (flag) {
                break;
            }
            distance += 15f;

        }
        _distance = distance;

        float num716 = 3;
        float[] array2 = new float[(int)num716];
        Collision.LaserScan(samplingPoint, Projectile.velocity, 0f, _distance, array2);
        float num718 = 0f;
        for (int num719 = 0; num719 < array2.Length; num719++) {
            num718 += array2[num719];
        }
        num718 /= num716;
        Projectile.ai[0] = num718;

        TerraProjCutTiles();

        Projectile.netUpdate = true;
    }

    private void TerraProjCutTiles() {
        float value = (float)Math.Pow(Utils.GetLerpValue(5f, 25f, Projectile.localAI[1], true), 0.5f) * Utils.GetLerpValue(currentCharge, currentCharge - 40f, Projectile.localAI[1], true);
        float value1 = (float)Math.Pow(value, 0.5f);
        if (value1 < 0.5f) {
            return;
        }
        if (!maxCharged) {
            return;
        }
        if (Projectile.owner != Main.myPlayer) {
            return;
        }
        Player player = Main.player[Projectile.owner];
        Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.Zero);
        for (int i2 = 1; i2 < Projectile.ai[0] / 20f; i2++) {
            Vector2 boxPosition = Projectile.Center + unit * 40f + unit * 20f * i2;
            int boxWidth = 5;
            int boxHeight = 5;
            int num = (int)(boxPosition.X / 16f);
            int num2 = (int)((boxPosition.X + (float)boxWidth) / 16f) + 1;
            int num3 = (int)(boxPosition.Y / 16f);
            int num4 = (int)((boxPosition.Y + (float)boxHeight) / 16f) + 1;
            if (num < 0)
                num = 0;

            if (num2 > Main.maxTilesX)
                num2 = Main.maxTilesX;

            if (num3 < 0)
                num3 = 0;

            if (num4 > Main.maxTilesY)
                num4 = Main.maxTilesY;

            bool[] tileCutIgnorance = Main.player[Projectile.owner].GetTileCutIgnorance(allowRegrowth: false, Projectile.trap);
            for (int i = num; i < num2; i++) {
                for (int j = num3; j < num4; j++) {
                    if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                        WorldGen.KillTile(i, j);
                        if (Main.netMode != 0)
                            NetMessage.SendData(17, -1, -1, null, 0, i, j);
                    }
                }
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanCutTiles() => false;
}