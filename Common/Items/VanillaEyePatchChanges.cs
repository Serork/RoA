using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Items;

sealed class VanillaEyePatchChanges : GlobalItem {
    public enum EyePatchMode : byte {
        LeftEye = 0,
        RightEye = 1,
        BothEyes = 2,
        Count
    }

    public EyePatchMode _currentEyePatchMode;

    public EyePatchMode CurrentEyePatchMode {
        get => _currentEyePatchMode;
        set => _currentEyePatchMode = (EyePatchMode)Utils.Clamp((byte)value, (byte)EyePatchMode.LeftEye, (byte)EyePatchMode.Count);
    }

    public override void Load() {
        On_ItemSlot.Handle_ItemArray_int_int += On_ItemSlot_Handle_ItemArray_int_int;
        On_ItemSlot.OverrideHover_ItemArray_int_int += On_ItemSlot_OverrideHover_ItemArray_int_int;
    }

    private void On_ItemSlot_OverrideHover_ItemArray_int_int(On_ItemSlot.orig_OverrideHover_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);

        if (context == ItemSlot.Context.EquipAccessory) {
            if (!Main.mouseRight)
                return;

            if (Main.mouseRightRelease) {
                Item item = inv[slot];
                var handler = item.GetGlobalItem<VanillaEyePatchChanges>();
                handler.CurrentEyePatchMode++;
                if (handler.CurrentEyePatchMode > EyePatchMode.BothEyes) {
                    handler.CurrentEyePatchMode = EyePatchMode.LeftEye;
                }
                Main.NewText(handler.CurrentEyePatchMode);
            }
        }
    }

    public override void SaveData(Item item, TagCompound tag) {
        tag["eyepatchmode"] = (byte)item.GetGlobalItem<VanillaEyePatchChanges>().CurrentEyePatchMode;
    }

    public override void LoadData(Item item, TagCompound tag) {
        item.GetGlobalItem<VanillaEyePatchChanges>().CurrentEyePatchMode = (EyePatchMode)tag.GetByte("eyepatchmode");
    }

    public override void NetSend(Item item, BinaryWriter writer) {
        writer.Write((byte)item.GetGlobalItem<VanillaEyePatchChanges>().CurrentEyePatchMode);
    }

    public override void NetReceive(Item item, BinaryReader reader) {
        item.GetGlobalItem<VanillaEyePatchChanges>().CurrentEyePatchMode = (EyePatchMode)reader.ReadByte();
    }

    private void On_ItemSlot_Handle_ItemArray_int_int(On_ItemSlot.orig_Handle_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.EyePatch;

    public override void SetDefaults(Item entity) {
        entity.headSlot = -1;

        entity.vanity = false;

        entity.accessory = true;
    }

    public override void UpdateEquip(Item item, Player player) {
        
    }

    public override void RightClick(Item item, Player player) {

    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        var handler = item.GetGlobalItem<VanillaEyePatchChanges>();
        EyePatchMode currentEyePatchMode = handler.CurrentEyePatchMode;
        bool onChosenSide = player.GetViableMousePosition().X < player.GetPlayerCorePoint().X;
        if (currentEyePatchMode == EyePatchMode.RightEye) {
            onChosenSide = player.GetViableMousePosition().X > player.GetPlayerCorePoint().X;
        }
        else if (_currentEyePatchMode == EyePatchMode.BothEyes) {
            onChosenSide = true;
        }
        if (item.DamageType == DamageClass.Ranged && onChosenSide) {
            switch (currentEyePatchMode) {
                case EyePatchMode.LeftEye:
                case EyePatchMode.RightEye:
                    damage = (int)(damage * 1.2f);
                    break;
                case EyePatchMode.BothEyes:
                    damage = (int)(damage * 1.1f);
                    break;
            }
        }
    }
}
