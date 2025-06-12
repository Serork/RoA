using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Items;

sealed class ItemSwapSystem : ILoadable {
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_projectilesToInteractWith")]
    public extern static ref List<int> Player__projectilesToInteractWith(Player player);


    public static ushort[] SwapToOnRightClick = ItemID.Sets.Factory.CreateUshortSet(0);

    public void Load(Mod mod) {
        On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        On_Player.ItemCheck_ManageRightClickFeatures += On_Player_ItemCheck_ManageRightClickFeatures;
    }

    private void On_Player_ItemCheck_ManageRightClickFeatures(On_Player.orig_ItemCheck_ManageRightClickFeatures orig, Player self) {
        orig(self);

        bool flag = self.selectedItem != 58 && self.controlUseTile && Main.myPlayer == self.whoAmI && !self.tileInteractionHappened && self.releaseUseItem && !self.controlUseItem && !self.mouseInterface && !CaptureManager.Instance.Active && !Main.HoveringOverAnNPC && !Main.SmartInteractShowingGenuine;
        bool flag2 = flag;
        if (!ItemID.Sets.ItemsThatAllowRepeatedRightClick[self.inventory[self.selectedItem].type] && !Main.mouseRightRelease)
            flag2 = false;

        if (flag2 && self.altFunctionUse == 0) {
            for (int i = 0; i < Player__projectilesToInteractWith(self).Count; i++) {
                Projectile projectile = Main.projectile[Player__projectilesToInteractWith(self)[i]];
                if (projectile.Hitbox.Contains(Main.MouseWorld.ToPoint()) || Main.SmartInteractProj == projectile.whoAmI) {
                    flag = false;
                    flag2 = false;
                    break;
                }
            }
        }

        if (flag2 && self.altFunctionUse == 0 && self.itemTime == 0 && self.itemAnimation == 0) {
            int type2 = self.inventory[self.selectedItem].type;
            if (SwapToOnRightClick[type2] > 0) {
                self.releaseUseTile = false;
                Main.mouseRightRelease = false;
                SoundEngine.PlaySound(SoundID.Grab);
                self.inventory[self.selectedItem].ChangeItemType(SwapToOnRightClick[type2]);
                Recipe.FindRecipes();
            }
        }
    }

    private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item) {
        orig(item);

        int type = item.type;
        if (SwapToOnRightClick[type] > 0) {
            item.ChangeItemType(SwapToOnRightClick[type]);

            SoundEngine.PlaySound(SoundID.Grab);
            Main.stackSplit = 30;
            Main.mouseRightRelease = false;
            Recipe.FindRecipes();
        }
    }

    public void Unload() { }
}
