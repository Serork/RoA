using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class RipePumpkin : NatureProjectile {
    //private static Wiggler _rotateWiggler = Wiggler.Create(1f, 5.5f);

    private float _pulseScale = 3;
    private float _pulseAlpha;

    protected override void SafeSetDefaults() {
        Projectile.Size = 20 * Vector2.One;

        Projectile.ignoreWater = true;
        Projectile.friendly = true;

        Projectile.aiStyle = -1;
        Projectile.timeLeft = 240;
    }

    //protected override void SafeOnSpawn(IEntitySource source) {
    //    Player player = Main.player[Projectile.owner];
    //    Projectile.CritChance = player.GetWeaponCrit(player.HeldItem);
    //}

    //public override bool? CanDamage() => false;

    //public override void Unload() => _rotateWiggler = null;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin / 2f;
        Color baseColor = Projectile.GetAlpha(lightColor);
        if (Projectile.owner == Main.myPlayer) {
            Color color = baseColor * _pulseAlpha;
            spriteBatch.Draw(texture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, Projectile.scale * _pulseScale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture, drawPos, frameRect, baseColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }

    public override void AI() {
        //Helper.ApplyWindPhysics(Projectile.Center, ref Projectile.velocity);

        Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.03f * Projectile.direction;
        float f = Math.Clamp(Projectile.ai[2] / 6f, 0f, 1f);
        Projectile.ai[0] += 1f;
        float min1 = 20f * f, min2 = 28f * f;
        float max = 52f * f;
        if (Projectile.ai[0] >= min1) {
            Projectile.velocity.Y += 0.3f;
            Projectile.velocity.X *= 0.98f;
            if (Projectile.ai[0] <= min2) {
                Projectile.ai[1] = Projectile.rotation;
            }
            else {
                if (_pulseScale > 1f) {
                    float getValue(float x) {
                        return (x - (x * 1.2f * f)) * 0.8f;
                    }
                    float value = 0.1f;
                    _pulseScale -= value + getValue(value);
                    if (_pulseAlpha < 1f) {
                        value = 0.05f;
                        _pulseAlpha += value + getValue(value);
                    }
                }
                /*if (!_rotateWiggler.Active && _pulseScale <= 1.5f) {
                    _rotateWiggler.Start();
                }
                _rotateWiggler.Update();*/
                float progress = Math.Min((Projectile.ai[0] - min2) / (max - min2), 1f);
                //Projectile._rotation = Projectile.ai[1] + (float)((double)_rotateWiggler.Value * 13.5 * (Math.PI / 45.0)) * progress;
                Projectile.rotation = Projectile.ai[1];
                if (Projectile.ai[0] >= max * 0.75f) {
                    int type = ModContent.ItemType<Items.Weapons.Nature.PreHardmode.RipePumpkin>();
                    if (Projectile.owner == Main.myPlayer && Main.mouseLeft && Main.mouseLeftRelease && (Main.player[Projectile.owner].GetSelectedItem().type == type || Main.mouseItem.type == type)) {
                        Player player = Main.player[Projectile.owner];
                        player.itemAnimation = player.itemTime = player.itemAnimationMax / 2;

                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new ItemAnimationPacket(player, player.itemAnimation));
                        }

                        //_rotateWiggler.Stop();
                        Projectile.Kill();
                        SoundEngine.PlaySound(SoundID.NPCDeath22, Projectile.position);
                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 4, Projectile.position));
                        }
                        int count = Projectile.ai[2] <= 4.5f ? 2 : 3;
                        for (int i = 0; i < count; i++) {
                            float posX = Main.rand.Next(-15, 16);
                            float posY = Main.rand.Next(-15, 16);
                            Vector2 pointPosition = Main.player[Projectile.owner].GetViableMousePosition();
                            Vector2 mousePos = new Vector2(pointPosition.X + posX, pointPosition.Y + posY);
                            Vector2 projectilePos = new Vector2(Projectile.position.X + posX, Projectile.position.Y + posY);
                            Vector2 direction = new Vector2(mousePos.X - projectilePos.X, mousePos.Y - projectilePos.Y);
                            direction.Normalize();
                            direction *= 15 * Main.rand.NextFloat(0.9f, 1.1f);
                            Projectile.NewProjectile(player.GetSource_ItemUse(player.GetSelectedItem()),
                                new Vector2(Projectile.Center.X + posX, Projectile.Center.Y + posY), direction, ModContent.ProjectileType<PumpkinSeed>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
                            //Main.projectile[projectile].CritChance = Projectile.CritChance;
                            //Main.newT
                        }
                        Projectile.netUpdate = true;
                    }
                }
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.NPCHit18, Projectile.position);

        return base.OnTileCollide(oldVelocity);
    }

    public override void OnKill(int timeLeft) {
        if (!Main.dedServ) {
            for (int i = 0; i < 4; i++)
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center - Projectile.Size / 2f + Main.rand.RandomPointInArea(Projectile.width, Projectile.height) / 3f,
                    Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/PumpkinGore").Type, 1f);
        }
        for (int i = 0; i < 10; i++) {
            int dust = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(Projectile.width, Projectile.height) - Projectile.velocity * 2f, 0, 0, DustID.Water_Desert, Projectile.velocity.X * 0.3f, 0, 0, new Color(250, 200, 100), 1.5f);
            Main.dust[dust].noGravity = false;
            Main.dust[dust].scale *= 0.9f;
            int dust2 = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(Projectile.width, Projectile.height) - Projectile.velocity * 2f, 0, 0, DustID.Water_Desert, 0, 0, 0, new Color(250, 200, 100), 1.5f);
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].scale *= 0.9f;
        }
    }
}