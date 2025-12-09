using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Configs;
using RoA.Core;
using RoA.Core.Defaults;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class RespriteLoader : ILoadable {
    private class RespriteLoader_Resizer : GlobalItem {
        public override bool InstancePerEntity => true;

        public override void SetDefaults(Item entity) {
            if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
                return;
            }

            int type = entity.type;
            switch (type) {
                case ItemID.Daybloom:
                    entity.SetSizeValues(20, 22);
                    break;
                case ItemID.Blinkroot:
                    entity.SetSizeValues(22, 24);
                    break;
                case ItemID.Deathweed:
                    entity.SetSizeValues(22, 24);
                    break;
                case ItemID.Fireblossom:
                    entity.SetSizeValues(18, 22);
                    break;
                case ItemID.Moonglow:
                    entity.SetSizeValues(20, 24);
                    break;
                case ItemID.Shiverthorn:
                    entity.SetSizeValues(20, 24);
                    break;
                case ItemID.Waterleaf:
                    entity.SetSizeValues(24, 24);
                    break;

                case ItemID.TerraBlade:
                    entity.SetSizeValues(46, 54);
                    break;

                case ItemID.CursedFlames:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.CrystalStorm:
                    entity.SetSizeValues(28, 32);
                    break;
                case ItemID.GoldenShower:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.BookofSkulls:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.DemonScythe:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.WaterBolt:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.MagnetSphere:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.RazorbladeTyphoon:
                    entity.SetSizeValues(28, 30);
                    break;
                case ItemID.LunarFlareBook:
                    entity.SetSizeValues(28, 32);
                    break;
            }
        }
    }

    public void Load(Mod mod) {
        string texturePath = ResourceManager.ItemTextures;
        int id;
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

        TextureAssets.Item[ItemID.Skull] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "Skull");
        TextureAssets.ArmorHead[ArmorIDs.Head.Skull] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "Skull_Head");
        TextureAssets.Item[ItemID.TerraBlade] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "TerraBlade");

        TextureAssets.Item[ItemID.CursedFlames] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.CursedFlames}");
        TextureAssets.Item[ItemID.CrystalStorm] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.CrystalStorm}");
        TextureAssets.Item[ItemID.GoldenShower] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.GoldenShower}");
        TextureAssets.Item[ItemID.BookofSkulls] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.BookofSkulls}");
        TextureAssets.Item[ItemID.DemonScythe] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.DemonScythe}");
        TextureAssets.Item[ItemID.WaterBolt] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.WaterBolt}");
        TextureAssets.Item[ItemID.MagnetSphere] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.MagnetSphere}");
        TextureAssets.Item[ItemID.RazorbladeTyphoon] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.RazorbladeTyphoon}");
        TextureAssets.Item[ItemID.LunarFlareBook] = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + $"Item_{ItemID.LunarFlareBook}");
    }

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

        TextureAssets.Item[ItemID.Skull] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.Skull}");
        TextureAssets.ArmorHead[ArmorIDs.Head.Skull] = ModContent.Request<Texture2D>($"Terraria/Images/Armor_Head_{ArmorIDs.Head.Skull}");
        TextureAssets.Item[ItemID.TerraBlade] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.TerraBlade}");

        TextureAssets.Item[ItemID.CursedFlames] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.CursedFlames}");
        TextureAssets.Item[ItemID.CrystalStorm] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.CrystalStorm}");
        TextureAssets.Item[ItemID.GoldenShower] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.GoldenShower}");
        TextureAssets.Item[ItemID.BookofSkulls] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.BookofSkulls}");
        TextureAssets.Item[ItemID.DemonScythe] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.DemonScythe}");
        TextureAssets.Item[ItemID.WaterBolt] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.WaterBolt}");
        TextureAssets.Item[ItemID.MagnetSphere] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.MagnetSphere}");
        TextureAssets.Item[ItemID.RazorbladeTyphoon] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.RazorbladeTyphoon}");
        TextureAssets.Item[ItemID.LunarFlareBook] = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.LunarFlareBook}");
    }

    public void Unload() {
        UnloadInner();
    }
}
