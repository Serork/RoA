using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Dusts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class SnowDust2 : ModDust, IDrawDustPrePlayer {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Snow;

    public override bool PreDraw(Dust dust) => false;

    void IDrawDustPrePlayer.DrawPrePlayer(Dust dust) {
        Main.EntitySpriteDraw(DustLoader.GetDust(dust.type).Texture2D.Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
    }
}