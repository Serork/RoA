using RoA.Common.Tiles;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RoA.Content.Backgrounds;

[Autoload(Side = ModSide.Client)]
sealed class BackwoodsScreenShaderData(string passName) : ScreenShaderData(passName) {
    public override void Apply() {
        if (ModContent.GetInstance<TileCount>().BackwoodsTiles > 0) {
            UseTargetPosition(Main.LocalPlayer.Center);
        }

        base.Apply();
    }
}
