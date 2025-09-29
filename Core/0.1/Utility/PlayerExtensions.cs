
using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Players;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class PlayerExtensions {
    public static bool WithinPlacementRange(this Player player, int x, int y) =>
        player.position.X / 16f - Player.tileRangeX - player.inventory[player.selectedItem].tileBoost - player.blockRange <= x
        && (player.position.X + player.width) / 16f + Player.tileRangeX + player.inventory[player.selectedItem].tileBoost - 1f + player.blockRange >= x
        && player.position.Y / 16f - Player.tileRangeY - player.inventory[player.selectedItem].tileBoost - player.blockRange <= y
        && (player.position.Y + player.height) / 16f + Player.tileRangeY + player.inventory[player.selectedItem].tileBoost - 2f + player.blockRange >= y;

    public static void AddBuffInStart(this Player self, int type, int time) {
        int[] newBuffType = new int[self.buffType.Length + 1];
        newBuffType[0] = type;
        Array.Copy(self.buffType, 0, newBuffType, 1, self.buffType.Length);
        self.buffType = newBuffType;
        int[] newBuffTime = new int[self.buffTime.Length + 1];
        newBuffTime[0] = time;
        Array.Copy(self.buffTime, 0, newBuffTime, 1, self.buffTime.Length);
        self.buffTime = newBuffTime;
    }

    public static bool HasSetBonusFrom<T>(this Player player) where T : ModItem {
        ModItem item = ItemLoader.GetItem(ModContent.ItemType<T>());
        if (item == null) {
            return false;
        }

        if (player.armor[0].type != item.Type) {
            return false;
        }

        bool result = item.IsArmorSet(player.armor[0], player.armor[1], player.armor[2]);
        return result;
    }

    public static bool CanTransfromIntoDruidForm<T>(this Player player, IDoubleTap.TapDirection direction) where T : ModItem => direction == Helper.CurrentDoubleTapDirectionForSetBonuses && HasSetBonusFrom<T>(player) && player.GetFormHandler().CanTransform;

    public static Vector2 PlayerMovementOffset(this Player player) {
        Vector2[] positions = [
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, -2f),
            new Vector2(0f, -2f),
            new Vector2(0f, -2f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, -2f),
            new Vector2(0f, -2f),
            new Vector2(0f, -2f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
        ];

        return new Vector2(positions[player.legFrame.Y / 56].X * player.direction, positions[player.legFrame.Y / 56].Y);
    }

    public static bool FindBuff(this Player player, int type, out int index) {
        index = player.FindBuffIndex(type);
        return index != -1;
    }

    public static bool HasBuff(this Player player, int type) => player.FindBuffIndex(type) != -1;

    public static Item GetSelectedItem(this Player player) => player.inventory[player.selectedItem];

    public static bool IsHoldingNatureWeapon(this Player player) {
        Item selectedItem = player.GetSelectedItem();
        if (!selectedItem.IsANatureWeapon()) {
            return false;
        }

        return true;
    }

    public static bool IsLocal(this Player player) => Main.myPlayer == player.whoAmI;

    public static void SetCompositeBothArms(this Player player, float armRotation, Player.CompositeArmStretchAmount compositeArmStretchAmount = Player.CompositeArmStretchAmount.Full) {
        player.SetCompositeArmBack(true, compositeArmStretchAmount, armRotation);
        player.SetCompositeArmFront(true, compositeArmStretchAmount, armRotation);
    }

    // adapted vanilla
    public static void LimitPointToPlayerReachableArea(this Player player, ref Vector2 pointPoisition, float maxX = 960f, float maxY = 600f) {
        Vector2 center = player.Center;
        Vector2 vector = pointPoisition - center;
        float num = Math.Abs(vector.X);
        float num2 = Math.Abs(vector.Y);
        float num3 = 1f;
        if (num > maxX) {
            float num4 = maxX / num;
            if (num3 > num4)
                num3 = num4;
        }

        if (num2 > maxY) {
            float num5 = maxY / num2;
            if (num3 > num5)
                num3 = num5;
        }

        Vector2 vector2 = vector * num3;
        pointPoisition = center + vector2;
    }

    public static Vector2 GetViableMousePosition(this Player player) {
        Vector2 result = Main.MouseWorld;
        player.LimitPointToPlayerReachableArea(ref result);
        return result;
    }

    public static Vector2 GetViableMousePosition(this Player player, float maxX = 960f, float maxY = 600f) {
        //float num14 = (float)Main.mouseX + Main.screenPosition.X;
        //float num15 = (float)Main.mouseY + Main.screenPosition.Y;
        //if (player.gravDir == -1f)
        //    num15 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
        Vector2 result = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;

        player.LimitPointToPlayerReachableArea(ref result, maxX, maxY);
        return result;
    }

    public static bool CheckVanitySet(this Player player, int head, int body, int legs) {
        if (player.armor[10].type == head &&
            player.armor[11].type == body &&
            player.armor[12].type == legs)
            return true;
        return false;
    }

    public static bool CheckArmorSlot(this Player player, int type, int slotID, int oppositeSlotID) {
        if (player.armor[slotID].type == type && player.armor[oppositeSlotID].type == ItemID.None)
            return true;
        return false;
    }

    public static bool CheckVanitySlot(this Player player, int type, int slotID) {
        if (player.armor[slotID].type == type)
            return true;
        return false;
    }

    public static void AddBuff<T>(this Player player, int timeToAdd) where T : ModBuff {
        player.AddBuff(ModContent.BuffType<T>(), timeToAdd);
    }

    public static void DelBuff<T>(this Player player) where T : ModBuff {
        if (player.FindBuff(ModContent.BuffType<T>(), out int buffIndex)) {
            player.DelBuff(buffIndex);
        }
    }
}
