
using Microsoft.Xna.Framework;

using System;

using Terraria;

namespace RoA.Core.Utility;

static class PlayerExtensions {
    public static Vector2 PlayerMovementOffset(this Player player) {
        Vector2[] positions = [
            new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(-1f, -3f),
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
        if (!selectedItem.IsADruidicWeapon()) {
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
}
