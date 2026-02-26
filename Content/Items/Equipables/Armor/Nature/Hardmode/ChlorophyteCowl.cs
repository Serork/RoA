using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class ChlorophyteCowl : NatureItem, IDoubleTap {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 26);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateArmorSet(Player player) {

    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {

    }

    public override void ArmorSetShadows(Player player) {

    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.ChlorophytePlateMail && legs.type == ItemID.ChlorophyteGreaves;

    public override void EquipFrameEffects(Player player, EquipType type) {

    }
}