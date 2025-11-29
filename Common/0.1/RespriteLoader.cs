using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Configs;
using RoA.Core;

using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed partial class RespriteLoader : ILoadable {
    public void Load(Mod mod) {
        string texturePath = ResourceManager.ItemTextures;
        int id = 4;
        if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
            UnloadInner();

            return;
        }

        texturePath = ResourceManager.ItemTextures;
        TextureAssets.Item[ItemID.Daybloom] = ModContent.Request<Texture2D>(texturePath + "Daybloom");
        TextureAssets.Item[ItemID.Blinkroot] = ModContent.Request<Texture2D>(texturePath + "Blinkroot");
        TextureAssets.Item[ItemID.Deathweed] = ModContent.Request<Texture2D>(texturePath + "Deathweed");
        TextureAssets.Item[ItemID.Fireblossom] = ModContent.Request<Texture2D>(texturePath + "Fireblossom");
        TextureAssets.Item[ItemID.Moonglow] = ModContent.Request<Texture2D>(texturePath + "Moonglow");
        TextureAssets.Item[ItemID.Shiverthorn] = ModContent.Request<Texture2D>(texturePath + "Shiverthorn");
        TextureAssets.Item[ItemID.Waterleaf] = ModContent.Request<Texture2D>(texturePath + "Waterleaf");

        texturePath = ResourceManager.TreeTileTextures;
        id = 4;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>(texturePath + $"Tree_Branches_{id}");
        id = 12;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>(texturePath + $"Tree_Branches_{id}");
        id = 16;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>(texturePath + $"Tree_Branches_{id}");
        id = 17;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>(texturePath + $"Tree_Branches_{id}");
        id = 18;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>(texturePath + $"Tree_Branches_{id}");

        LoadV02Resprites();
    }

    public partial void LoadV02Resprites();

    private void UnloadInner() {
        TextureAssets.Item[ItemID.Daybloom] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Daybloom}");
        TextureAssets.Item[ItemID.Blinkroot] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Blinkroot}");
        TextureAssets.Item[ItemID.Deathweed] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Deathweed}");
        TextureAssets.Item[ItemID.Fireblossom] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Fireblossom}");
        TextureAssets.Item[ItemID.Moonglow] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Moonglow}");
        TextureAssets.Item[ItemID.Shiverthorn] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Shiverthorn}");
        TextureAssets.Item[ItemID.Waterleaf] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Waterleaf}");

        int id = 4;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>($"Terraria/Images/Tree_Branches_{id}");
        id = 12;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>($"Terraria/Images/Tree_Branches_{id}");
        id = 16;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>($"Terraria/Images/Tree_Branches_{id}");
        id = 17;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>($"Terraria/Images/Tree_Branches_{id}");
        id = 18;
        TextureAssets.TreeBranch[id] = ModContent.Request<Texture2D>($"Terraria/Images/Tree_Branches_{id}");

        UnloadV02Resprites();
    }

    public partial void UnloadV02Resprites();

    public void Unload() {
        UnloadInner();
    }
}
