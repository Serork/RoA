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
using Terraria.GameContent;

namespace RoA.Content.Projectiles.Enemies;

[Tracked]
sealed class ElderSnailTrail : ModProjectile_NoTextureLoad, IRequestAssets {
    public enum ElderSnailTrailRequstedTextureType : byte {
        Trail1,
        Trail2,
        Trail3,
        Count
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ElderSnailTrailRequstedTextureType.Trail1, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail1"),
         ((byte)ElderSnailTrailRequstedTextureType.Trail2, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail2"),
         ((byte)ElderSnailTrailRequstedTextureType.Trail3, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail3")];

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float TextureVariant => ref Projectile.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;

        Projectile.hostile = true;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            TextureVariant = Main.rand.Next((byte)ElderSnailTrailRequstedTextureType.Count);
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<ElderSnailTrail>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Color color = new(120, 120, 120, 120);
        SpriteBatch batch = Main.spriteBatch;
        ulong seed = Main.TileFrameSeed ^ (((ulong)Projectile.position.X << 32) | (uint)Projectile.position.Y);
        Texture2D texture = indexedTextureAssets[(byte)TextureVariant].Value;
        for (int i = 0; i < 4; i++) {
            Vector2 position = Projectile.Center + new Vector2(Utils.RandomInt(ref seed, -2, 3), 0f);
            Rectangle clip = texture.Bounds;
            Vector2 origin = clip.Centered();
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color
            });
        }
    }
}
