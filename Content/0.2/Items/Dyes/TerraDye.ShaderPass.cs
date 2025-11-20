using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Dyes;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace RoA.Content.Items.LiquidsSpecific;

sealed class TerraDyeArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
    public override void Apply(Entity entity, DrawData? drawData) {
        Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 10f);
        Main.instance.GraphicsDevice.Textures[1] = TerraDye.TerraShaderMap.Value;

        base.Apply(entity, drawData);
    }
}