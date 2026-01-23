using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class ObsidianStopwatchClock : ModProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    public enum ObsidianStopwatchClock_RequstedTextureType : byte {
        Base,
        Arrow1,
        Arrow2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ObsidianStopwatchClock_RequstedTextureType.Base, ResourceManager.MiscellaneousProjectileTextures + "ClockEffect"),
         ((byte)ObsidianStopwatchClock_RequstedTextureType.Arrow1, ResourceManager.MiscellaneousProjectileTextures + "ClockEffect_Hand1"),
         ((byte)ObsidianStopwatchClock_RequstedTextureType.Arrow2, ResourceManager.MiscellaneousProjectileTextures + "ClockEffect_Hand2")];

    public ref float Arrow1Rotation => ref Projectile.localAI[0];
    public ref float Arrow2Rotation => ref Projectile.localAI[1];

    private Vector2 _spawnPosition;

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = Projectile.scale = 0f;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => false;

    public override void AI() {
        float lerpValue = 0.025f;
        if (Projectile.scale == 0f) {
            _spawnPosition = Projectile.Center;
            Arrow1Rotation = MathHelper.TwoPi * Main.rand.NextFloat();
            Arrow2Rotation = MathHelper.TwoPi * Main.rand.NextFloat();
        }
        Projectile.scale = Helper.Approach(Projectile.scale, 1f, lerpValue);
        if (Projectile.scale < 1f) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, lerpValue);

            Projectile.Center = Vector2.Lerp(_spawnPosition, Projectile.GetOwnerAsPlayer().GetPlayerCorePoint(), Ease.CubeOut(Projectile.Opacity) * 0.15f);
        }
        else {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, lerpValue);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }

        Projectile.velocity *= 0f;

        Arrow1Rotation += 0.05f;
        Arrow2Rotation += 0.15f;
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<ObsidianStopwatchClock>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D baseTexture = indexedTextureAssets[(byte)ObsidianStopwatchClock_RequstedTextureType.Base].Value,
                  arrow1Texture = indexedTextureAssets[(byte)ObsidianStopwatchClock_RequstedTextureType.Arrow1].Value,
                  arrow2Texture = indexedTextureAssets[(byte)ObsidianStopwatchClock_RequstedTextureType.Arrow2].Value;

        SpriteBatch batch = Main.spriteBatch;
        Vector2 position = Projectile.Center;

        Player self = Projectile.GetOwnerAsPlayer();

        float offset = self.whoAmI;
        float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
        Color color = Main.hslToRgb(hue, 1f, 0.5f);
        if (self.GetModPlayer<SmallMoonPlayer>().HasContributor) {
            color = Color.Lerp(self.GetModPlayer<SmallMoonPlayer>().smallMoonColor, self.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
        }
        color.A = 25;
        color *= 0.5f;
        Color result = Lighting.GetColor(position.ToTileCoordinates());
        Color color2 = Color.Lerp(result, result.MultiplyRGBA(color), 0.25f);

        color = color2 * Ease.CubeOut(Projectile.Opacity);

        Rectangle clip = baseTexture.Bounds;
        Vector2 origin = clip.Centered();
        Vector2 scale = Vector2.One * Ease.QuadOut(Projectile.scale) * MathHelper.Lerp(1f, 1.375f, Projectile.Opacity);
        DrawInfo baseDrawInfo = new() {
            Clip = clip,
            Origin = origin,
            Color = color,
            Scale = scale
        };

        batch.Draw(baseTexture, position, baseDrawInfo);
        batch.Draw(arrow1Texture, position, baseDrawInfo with {
            Rotation = Arrow1Rotation
        });
        batch.Draw(arrow2Texture, position, baseDrawInfo with {
            Rotation = Arrow2Rotation
        });
    }
}
