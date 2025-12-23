using Microsoft.Xna.Framework;

using RoA.Core.Data;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Extensions;

static partial class PlayerExtensions {
    public static Vector2 GetPlayerCorePoint(this Player player) {
        Vector2 vector = player.Bottom;
        Vector2 pos = player.MountedCenter;
        return Utils.Floor(vector + (pos - vector) + new Vector2(0f, player.gfxOffY));
    }

    public static bool HasProjectile<T>(this Player player, int count = 1) where T : ModProjectile => player.ownedProjectileCounts[ModContent.ProjectileType<T>()] >= count;

    public static bool IsAliveAndFree(this Player player) => player.IsAlive() && !player.CCed;
    public static bool IsAlive(this Player player) => player.active && !player.dead;

    public static bool IsHolding<T>(this Player player) where T : ModItem => player.HeldItem.type == ModContent.ItemType<T>();

    public static void UseBodyFrame(this Player player, PlayerFrame frameToUse) => player.bodyFrame.Y = 56 * (int)frameToUse;

    public static bool HasEquipped(this Player player, ushort slot, EquipType equipType) {
        var Mod = RoA.Instance;
        int legs;
        var armor = player.armor;
        legs = armor[2].legSlot;
        if (armor[12].legSlot >= 0)
            legs = armor[12].legSlot;
        int body;
        body = armor[1].bodySlot;
        if (armor[11].bodySlot >= 0)
            body = armor[11].bodySlot;
        int head;
        head = armor[0].headSlot;
        if (armor[10].headSlot >= 0)
            head = armor[10].headSlot;
        int check = head;
        if (equipType == EquipType.Body) {
            check = body;
        }
        else if (equipType == EquipType.Legs) {
            check = legs;
        }
        if (check == slot) {
            return true;
        }
        return false;
    }

    public static bool HasEquipped<T>(this Player player, EquipType equipType, ushort slot = 0) where T : ModItem {
        var Mod = RoA.Instance;
        int legs;
        var armor = player.armor;
        legs = armor[2].legSlot;
        if (armor[12].legSlot >= 0)
            legs = armor[12].legSlot;
        int body;
        body = armor[1].bodySlot;
        if (armor[11].bodySlot >= 0)
            body = armor[11].bodySlot;
        int head;
        head = armor[0].headSlot;
        if (armor[10].headSlot >= 0)
            head = armor[10].headSlot;
        int check = head;
        if (equipType == EquipType.Body) {
            check = body;
        }
        else if (equipType == EquipType.Legs) {
            check = legs;
        }
        if (check == EquipLoader.GetEquipSlot(Mod, typeof(T).Name, equipType)) {
            return true;
        }
        return false;
    }
}
