using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;

namespace RoA.Content.Backgrounds;

sealed class VignetteScreenShaderData : ScreenShaderData {
    public VignetteScreenShaderData(Effect effect, string pass)
        : base(new Ref<Effect>(effect), pass) {
	}
}