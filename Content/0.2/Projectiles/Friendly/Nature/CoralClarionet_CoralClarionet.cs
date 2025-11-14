using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CoralClarionet : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static float SPAWNTIMEINTICKS => 10f;

    public enum CoralClarionetRequstedTextureType : byte {
        Part1,
        Part2,
        Part3,
        Part4
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)CoralClarionetRequstedTextureType.Part1, ResourceManager.NatureProjectileTextures + "CoralClarionet_1"),
         ((byte)CoralClarionetRequstedTextureType.Part2, ResourceManager.NatureProjectileTextures + "CoralClarionet_2"),
         ((byte)CoralClarionetRequstedTextureType.Part3, ResourceManager.NatureProjectileTextures + "CoralClarionet_3"),
         ((byte)CoralClarionetRequstedTextureType.Part4, ResourceManager.NatureProjectileTextures + "CoralClarionet_4")];

    public ref float WaveValue => ref Projectile.localAI[0];
    public ref float WaveValue2 => ref Projectile.localAI[1];
    public ref float SpawnValue => ref Projectile.localAI[2];

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.friendly = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        WaveValue2 = (float)(Projectile.whoAmI + Main.timeForVisualEffects) * TimeSystem.LogicDeltaTime;
        WaveValue = Projectile.whoAmI + (float)(Main.timeForVisualEffects / 10 % MathHelper.TwoPi);

        SpawnValue = Helper.Approach(SpawnValue, SPAWNTIMEINTICKS, 1f);
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<CoralClarionet>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D part1Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part1].Value,
                  part2Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part2].Value,
                  part3Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part3].Value,
                  part4Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part4].Value;
        Vector2 position = Projectile.Center;
        Rectangle clip = part1Texture.Bounds;
        Vector2 origin = clip.BottomCenter();
        SpriteBatch batch = Main.spriteBatch;
        float spawnProgress = SpawnValue / SPAWNTIMEINTICKS;
        float opacity = Utils.GetLerpValue(0f, 0.25f, spawnProgress, true);
        Color color = Color.White * opacity;
        Vector2 scale = Vector2.One * Ease.QuartOut(spawnProgress);
        scale.X *= Utils.Remap(spawnProgress * Ease.CubeOut(spawnProgress), 0f, 1f, 2f, 1f);
        scale.Y *= Utils.Remap(spawnProgress * Ease.CubeIn(spawnProgress), 0f, 1f, 0.75f, 1f);
        DrawInfo drawInfo = DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color,
            Scale = scale
        };
        float maxRotation = 0.05f;
        ShaderLoader.WavyShader.WaveFactor = WaveValue;
        ShaderLoader.WavyShader.StrengthX = 0.15f;
        ShaderLoader.WavyShader.StrengthY = 1.5f;
        ShaderLoader.WavyShader.DrawColor = color;
        ShaderLoader.WavyShader.Apply(batch, () => {
            drawInfo = drawInfo with { Rotation = Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 0f) };
            batch.Draw(part1Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 1f) };
            batch.Draw(part2Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 2f) };
            batch.Draw(part3Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 3f) };
            batch.Draw(part4Texture, position, drawInfo);
        });
    }
}
