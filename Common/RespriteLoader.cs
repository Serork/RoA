using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class RespriteLoader : ILoadable {
    public void Load(Mod mod) {
        string texturePath = ResourceManager.ItemsTextures;
        TextureAssets.Item[ItemID.Daybloom] = ModContent.Request<Texture2D>(texturePath + "Daybloom");
        TextureAssets.Item[ItemID.Blinkroot] = ModContent.Request<Texture2D>(texturePath + "Blinkroot");
        TextureAssets.Item[ItemID.Deathweed] = ModContent.Request<Texture2D>(texturePath + "Deathweed");
        TextureAssets.Item[ItemID.Fireblossom] = ModContent.Request<Texture2D>(texturePath + "Fireblossom");
        TextureAssets.Item[ItemID.Moonglow] = ModContent.Request<Texture2D>(texturePath + "Moonglow");
        TextureAssets.Item[ItemID.Shiverthorn] = ModContent.Request<Texture2D>(texturePath + "Shiverthorn");
        TextureAssets.Item[ItemID.Waterleaf] = ModContent.Request<Texture2D>(texturePath + "Waterleaf");
    }

    public void Unload() { }
}
