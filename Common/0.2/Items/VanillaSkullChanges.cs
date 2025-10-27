using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    private static int _skullFaceSlot;

    public override void SetDefaults(Item entity) {
        ResetSkullItemDefaults(entity);
    }

    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHead[93] = false;

        ArmorIDs.Face.Sets.PreventHairDraw[_skullFaceSlot] = true;
        ArmorIDs.Face.Sets.OverrideHelmet[_skullFaceSlot] = true;
    }

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _skullFaceSlot = EquipLoader.AddEquipTexture(Mod, ResourceManager.ItemTextures + "Skull_Face", EquipType.Face, name: "VanillaSkullFaceSlot");
    }

    private void ResetSkullItemDefaults(Item entity) {
        if (entity.type == ItemID.Skull) {
            int width = 20, height = 22;
            entity.Size = new Vector2(width, height);
            entity.headSlot = 93;
            entity.rare = ItemRarityID.White;
            entity.vanity = false;

            entity.accessory = true;

            entity.faceSlot = _skullFaceSlot;
        }
    }

    public override string IsArmorSet(Item head, Item body, Item legs) {
        if (head.type == ItemID.Skull && body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>()) {
            return "SkullArmorSet";
        }

        return base.IsArmorSet(head, body, legs);
    }

    public override void UpdateArmorSet(Player player, string set) {
        if (set == "SkullArmorSet") {
            player.GetCommon().ApplyVanillaSkullSetBonus = true;
        }
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded) {
        if (item.type == ItemID.Skull) {
            return player.GetCommon().PerfectClotActivated;
        }

        return base.CanEquipAccessory(item, player, slot, modded);
    }

    public override void UpdateEquip(Item item, Player player) {
        if (item.type == ItemID.Skull && player.GetCommon().PerfectClotActivated) {
            player.GetCommon().ApplyVanillaSkullSetBonus = true;
            player.GetCommon().StopFaceDrawing = true;
        }
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
        if (item.type == ItemID.Skull) {
            player.GetCommon().ApplyVanillaSkullSetBonus = true;
        }
    }
}
