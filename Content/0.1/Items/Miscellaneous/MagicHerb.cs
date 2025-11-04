using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Content.Items.Miscellaneous;

class MagicHerb3 : MagicHerb1 { }
class MagicHerb2 : MagicHerb1 { }
class MagicHerb1 : ModItem {
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawItem_GetBasics")]
    public extern static void Main_DrawItem_GetBasics(Main main, Item item, int slot, out Texture2D texture, out Microsoft.Xna.Framework.Rectangle frame, out Microsoft.Xna.Framework.Rectangle glowmaskFrame);

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Magic Herb");
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        ItemID.Sets.IsAPickup[Type] = true;

        ItemID.Sets.IgnoresEncumberingStone[Type] = true;

        Item.ResearchUnlockCount = 0;
    }

    public override void Load() {
        On_Main.DrawItem += On_Main_DrawItem;
        On_Item.DespawnIfMeetingConditions += On_Item_DespawnIfMeetingConditions;
    }

    private void On_Item_DespawnIfMeetingConditions(On_Item.orig_DespawnIfMeetingConditions orig, Item self, int i) {
        orig(self, i);

        if (self.ModItem != null && self.ModItem is MagicHerb1 && self.timeSinceItemSpawned > 900) {
            for (int l = 0; l < 10; l++) {
                Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X, self.velocity.Y, 200,
                    Color.Lime, 0.9f);
            }

            self.active = false;
            self.type = 0;
            self.stack = 0;
            if (Main.netMode == 2)
                NetMessage.SendData(21, -1, -1, null, i);
        }
    }

    private void On_Main_DrawItem(On_Main.orig_DrawItem orig, Main self, Item item, int whoami) {
        orig(self, item, whoami);

        if (!item.active || item.IsAir)
            return;

        List<int> magicHerbIds = [ModContent.ItemType<MagicHerb1>(), ModContent.ItemType<MagicHerb2>(), ModContent.ItemType<MagicHerb3>()];
        if (!magicHerbIds.Contains(item.type)) {
            return;
        }

        Main.instance.LoadItem(item.type);
        Main_DrawItem_GetBasics(self, item, whoami, out var texture, out var frame, out var glowmaskFrame);
        Vector2 vector = frame.Size() / 2f;
        Vector2 vector2 = new Vector2((float)(item.width / 2) - vector.X, item.height - frame.Height);
        Vector2 vector3 = item.position - Main.screenPosition + vector + vector2;
        float num = item.velocity.X * 0.2f;
        if (item.shimmered)
            num = 0f;

        float scale = 1f;
        Microsoft.Xna.Framework.Color color = Lighting.GetColor(item.Center.ToTileCoordinates());
        Microsoft.Xna.Framework.Color currentColor = item.GetAlpha(color);
        if (item.shimmered) {
            currentColor.R = (byte)(255f * (1f - item.shimmerTime));
            currentColor.G = (byte)(255f * (1f - item.shimmerTime));
            currentColor.B = (byte)(255f * (1f - item.shimmerTime));
            currentColor.A = (byte)(255f * (1f - item.shimmerTime));
        }
        else if (item.shimmerTime > 0f) {
            currentColor.R = (byte)((float)(int)currentColor.R * (1f - item.shimmerTime));
            currentColor.G = (byte)((float)(int)currentColor.G * (1f - item.shimmerTime));
            currentColor.B = (byte)((float)(int)currentColor.B * (1f - item.shimmerTime));
            currentColor.A = (byte)((float)(int)currentColor.A * (1f - item.shimmerTime));
        }

        ItemSlot.GetItemLight(ref currentColor, ref scale, item);
        int num2 = item.glowMask;
        if (!Main.gamePaused && Main.instance.IsActive && (magicHerbIds.Contains(item.type)) && color.R > 60 && (float)Main.rand.Next(500) - (Math.Abs(item.velocity.X) + Math.Abs(item.velocity.Y)) * 10f < (float)((int)color.R / 50)) {
            int type = 43;
            Microsoft.Xna.Framework.Color newColor = Microsoft.Xna.Framework.Color.White;
            int alpha = 254;
            float scale2 = 0.5f;
            switch (item.type) {
                case 71:
                    type = 244;
                    break;
                case 72:
                    type = 245;
                    break;
                case 73:
                    type = 246;
                    break;
                case 74:
                    type = 247;
                    break;
            }

            int num3 = Dust.NewDust(item.position, item.width, item.height, type, 0f, 0f, alpha, newColor, scale2);
            Main.dust[num3].velocity *= 0f;
        }
    }

    public override Color? GetAlpha(Color lightColor) => Color.White * 0.9f;

    public override void SetDefaults() {
        int width = 20; int height = width;
        Item.Size = new Vector2(width, height);
    }

    public override bool ItemSpace(Player player) => true;

    public override bool CanPickup(Player player) => true;

    public override void PostUpdate() {
        _ = Main.rand.Next(90, 111) * 0.01f * (Main.essScale * 0.5f);
        Lighting.AddLight(Item.Center, Color.LightGreen.ToVector3() * 0.5f * Main.essScale);
    }

    public override bool OnPickup(Player player) {
        bool flag = false;
        foreach (Player checkPlayer in Main.ActivePlayers) {
            if (checkPlayer.GetWreathHandler().IsActualFull1) {
                flag = true;
                break;
            }
        }
        if (player.GetWreathHandler().IsActualFull1) {
            player.statLife += 40;
            if (Main.myPlayer == player.whoAmI) {
                player.HealEffect(40);
            }
            player.GetWreathHandler().ForcedHardReset();
        }
        else {
            player.statLife += 20;
            if (Main.myPlayer == player.whoAmI) {
                player.HealEffect(20);
            }
        }
        SoundEngine.PlaySound(SoundID.Grab, player.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "HealQuick") { Volume = 0.8f, PitchVariance = 0.2f }, player.Center);
        if (flag) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 9, player.Center));
            }
        }
        return false;
    }
}
