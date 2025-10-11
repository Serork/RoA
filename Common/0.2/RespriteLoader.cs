using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed partial class RespriteLoader {
    public partial void LoadV02Resprites() {
        TextureAssets.Item[ItemID.Skull] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "Skull");
        TextureAssets.ArmorHead[ArmorIDs.Head.Skull] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "Skull_Head");
    }

    public partial void UnloadV02Resprites() {
        TextureAssets.Item[ItemID.Daybloom] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Skull}");
        TextureAssets.ArmorHead[ArmorIDs.Head.Skull] = ModContent.Request<Texture2D>($"Terraria/Images/Armor_Head_{ArmorIDs.Head.Skull}");
    }
}
