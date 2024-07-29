using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Common.Druid.Claws;

sealed class ClawsStats : ModPlayer {
    public (Color, Color) SlashColors { get; private set; }

    public void SetColors(Color firstSlashColor, Color secondSlashColor) => SlashColors = (firstSlashColor, secondSlashColor);
}
