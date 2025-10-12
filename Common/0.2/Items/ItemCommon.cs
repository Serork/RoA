using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    public override bool InstancePerEntity => true;
}
