using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.NPCs.Enemies.Tar;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using static RoA.Content.NPCs.Enemies.Tar.PerfectMimic;

namespace RoA.Content.Projectiles.Enemies;

[Tracked]
sealed class TarMass : ModProjectile {
    private static ushort TIMELEFT => 300;

    private FluidBodyPart[] _fluidBodyParts = null!;

    public bool OnDeath => Projectile.ai[2] < 0f;

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        Projectile.SetSizeValues(30);

        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.hide = true;
        Projectile.Opacity = 0f;
        Projectile.timeLeft = TIMELEFT;
        Projectile.aiStyle = 1;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCs.Add(index);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        Projectile.tileCollide = Projectile.timeLeft < TIMELEFT - TIMELEFT / 4;

        if (Projectile.ai[2] == -1f) {
            Projectile.ai[2] = -2f;
            Projectile.Opacity = 1f;
            Projectile.localAI[2] = 1f;
            Projectile.scale = 1f;
        }

        float target = 0.75f;
        if (Projectile.ai[2] == 1f) {
            Projectile.velocity *= 0.9f;
            target = 0f;
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        else {
            if (Main.rand.Next(OnDeath ? 10 : 3) == 0) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<TarMetaball>(), 0f, 0f, 50, default(Color), 1.3f);
            }
        }
        if (OnDeath) {
            target = 0.75f;
            if (Projectile.ai[2] == 1f) {
                target = 0f;
            }
            Projectile.tileCollide = Projectile.timeLeft < TIMELEFT - TIMELEFT / 10;
        }
        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, target, 0.25f);

        Projectile.scale = MathHelper.Lerp(Projectile.scale, target, 0.25f);
        Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], target, 0.25f);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            int partCount = 10;
            _fluidBodyParts = new FluidBodyPart[partCount];
            for (int i = 0; i < partCount; i++) {
                Vector2 position = (i / MathHelper.TwoPi).ToRotationVector2() * 5f;
                _fluidBodyParts[i] = new FluidBodyPart(position, Main.rand.GetRandomEnumValue<FluidBodyPartType>(), Main.rand.NextFloatRange(MathHelper.TwoPi));
            }

            return;
        }

        Projectile.localAI[1] += TimeSystem.LogicDeltaTime;
        for (int i = 0; i < _fluidBodyParts.Length; i++) {
            _fluidBodyParts[i].Rotation += TimeSystem.LogicDeltaTime * (i % 2 == 0).ToDirectionInt() * Projectile.direction;
            _fluidBodyParts[i].Rotation += 0.01f * Projectile.direction ;
            _fluidBodyParts[i].Velocity = Helper.Wave(Projectile.localAI[1], -1f, 1f, 5f, i).ToRotationVector2() * 5f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.Opacity >= 0.5f) {
            if (Projectile.ai[2] != 1f) {
                SoundEngine.PlaySound(PerfectMimic.SplashSound, Projectile.Center);
            }
            Projectile.ai[2] = 1f;
        }

        return false;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            return;
        }

        Texture2D texture;
        foreach (var part in _fluidBodyParts) {
            switch (part.Type) {
                default:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part1].Value;
                    break;
                case FluidBodyPartType.Part2:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part2].Value;
                    break;
                case FluidBodyPartType.Part3:
                    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part3].Value;
                    break;
            }
            SpriteBatch batch = Main.spriteBatch;
            Vector2 position = Projectile.Center;
            Rectangle clip = texture.Bounds;
            Vector2 origin = clip.Centered() + part.Position + part.Velocity;
            Color color = Color.Lerp(Lighting.GetColor(position.ToTileCoordinates()), Color.White, 0.25f);
            float velRotation = ((float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + (float)Math.PI / 2f);
            float rotation = velRotation + part.Rotation * 0.25f;
            Vector2 scale = new(Projectile.localAI[2], Projectile.scale);
            SpriteEffects effects = Projectile.FacedRight() ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = scale,
                    Rotation = rotation,
                    ImageFlip = effects
                });
            });
        }
    }
}
