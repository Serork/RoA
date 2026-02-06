using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace RoA.Content.Items.LiquidsSpecific;

sealed class TerraDyeArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
    public static float Opacity = 1f;

    public override void Apply(Entity entity, DrawData? drawData) {
        Shader.Parameters["uOpacity"].SetValue(Opacity);
        Shader.Parameters["uTime"].SetValue(TimeSystem.TimeForVisualEffects * 7.5f);
        Main.instance.GraphicsDevice.Textures[1] = TerraDye.TerraShaderMap.Value;

        base.Apply(entity, drawData);

        Opacity = 1f;
    }
}