using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Common.Claws;

sealed class ClawsStats : ModPlayer {
    private Color _firstSlashColor, _secondSlashColor;

    public (Color, Color) SlashColors => (_firstSlashColor, _secondSlashColor);

    public void SetColors(Color firstSlashColor, Color secondSlashColor) {
        _firstSlashColor = firstSlashColor;
        _secondSlashColor = secondSlashColor;
    }
}
