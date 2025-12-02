using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace RoA.Content.Items.LiquidsSpecific;

sealed class TarDyeArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
    public static Vector3? Color1, Color2;

    public override void Apply(Entity entity, DrawData? drawData) {
        UseImage(ResourceManager.NoiseRGB);
        UseSecondaryColor(new Color(62, 53, 70));
        //GameShaders.Armor.GetShaderFromItemId(ItemID.ShiftingPearlSandsDye).UseColor(98 / 255f, 85 / 255f, 101 / 255f).UseSecondaryColor(46 / 255, 34 / 255f, 47 / 255f).UseOpacity(0.5f).Apply(entity, drawData);
        Shader.Parameters["rgbNoise"]?.SetValue(ResourceManager.NoiseRGB.Value);
        Shader.Parameters["liquidColor1"]?.SetValue(Color1 ?? new Vector3(62 / 255f + 0.3f, 53 / 255f + 0.3f, 70 / 255f + 0.3f));
        Shader.Parameters["liquidColor2"]?.SetValue(Color2 ?? new Vector3(98 / 255f + 0.3f, 85 / 255f + 0.3f, 101 / 255f + 0.3f));
        base.Apply(entity, drawData);

        Color1 = Color2 = null;
    }
}