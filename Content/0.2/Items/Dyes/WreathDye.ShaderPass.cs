using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace RoA.Content.Items.Dyes;

sealed class WreathDyeArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
    public override void Apply(Entity entity, DrawData? drawData) {
        base.Apply(entity, drawData);
    }
}