using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Items.LiquidsSpecific;
using RoA.Core;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class TerraDye : ModItem {
    public static Asset<Texture2D> TerraShaderMap { get; private set; } = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            GameShaders.Armor.BindShader(Type, new TerraDyeArmorShaderData(ShaderLoader.TerraDye, "TerraDyePass"));

            TerraShaderMap = ModContent.Request<Texture2D>(ResourceManager.Textures + "TerraMap");
        }
    }

    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice();
        Item.rare = ItemRarityID.Green;
    }
}
