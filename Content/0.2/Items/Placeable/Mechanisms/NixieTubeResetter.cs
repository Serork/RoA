using RoA.Content.Tiles.Mechanisms;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Mechanisms;

class NixieTubeResetter : NixieTubeIncreaser {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.placeStyle = 6;
    }
}

