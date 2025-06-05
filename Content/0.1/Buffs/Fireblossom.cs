using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Fireblossom : ModBuff {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) => false;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}