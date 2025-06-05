using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RoA.Content.Backgrounds;

[Autoload(Side = ModSide.Client)]
sealed class VignetteScreenShaderData : ScreenShaderData {
    public VignetteScreenShaderData(Effect effect, string pass)
        : base(new Ref<Effect>(effect), pass) {
    }
}