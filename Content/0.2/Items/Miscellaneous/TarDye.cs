﻿using Microsoft.Xna.Framework;

using RoA.Common;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class TarDye : ModItem {
    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            GameShaders.Armor.BindShader(Type, new TarDyeArmorShaderData(ShaderLoader.TarDye, "TarDyePass"));
        }
    }

    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice();
        Item.rare = 3;
    }
}
