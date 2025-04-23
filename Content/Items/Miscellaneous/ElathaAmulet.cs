using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Items.Miscellaneous;

sealed class ElathaAmulet : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Elatha Scepter");
        //Tooltip.SetDefault("Changes the phases of the Moon");

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(20, 4));
    }

    public override void Load() {
        On_Player.GetItemDrawFrame += On_Player_GetItemDrawFrame;
    }

    private Rectangle On_Player_GetItemDrawFrame(On_Player.orig_GetItemDrawFrame orig, Player self, int type) {
        if (Main.dedServ)
            return Rectangle.Empty;

        if (type == ModContent.ItemType<ElathaAmulet>()) {
            byte usedFrame = 0;
            if (Main.moonPhase == 0 || Main.moonPhase == 1) usedFrame = 0;
            if (Main.moonPhase == 2 || Main.moonPhase == 3) usedFrame = 1;
            if (Main.moonPhase == 4 || Main.moonPhase == 5) usedFrame = 2;
            if (Main.moonPhase == 6 || Main.moonPhase == 7) usedFrame = 3;
            int height = 42;
            int y = (int)(usedFrame * height);
            Texture2D text = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = new(0, y, text.Width, height);
            return frame;
        }
        else {
            return orig(self, type);
        }
    }

    public override void SetDefaults() {
        Item.Size = new Vector2(16, 42);
        Item.rare = ItemRarityID.LightRed;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.reuseDelay = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item4;
        Item.mana = 30;

        Item.value = Item.sellPrice(0, 2, 0, 0);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D text = ModContent.Request<Texture2D>(Texture).Value;
        byte usedFrame = 0;
        if (Main.moonPhase == 0 || Main.moonPhase == 1) usedFrame = 0;
        if (Main.moonPhase == 2 || Main.moonPhase == 3) usedFrame = 1;
        if (Main.moonPhase == 4 || Main.moonPhase == 5) usedFrame = 2;
        if (Main.moonPhase == 6 || Main.moonPhase == 7) usedFrame = 3;
        int height = 42;
        frame.Y = (int)(usedFrame * height);
        frame.Height = height;
        spriteBatch.Draw(text, position,
            frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

        text = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
        spriteBatch.Draw(text, position,
            frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        Texture2D text = ModContent.Request<Texture2D>(Texture).Value;
        byte usedFrame = 0;
        if (Main.moonPhase == 0 || Main.moonPhase == 1) usedFrame = 0;
        if (Main.moonPhase == 2 || Main.moonPhase == 3) usedFrame = 1;
        if (Main.moonPhase == 4 || Main.moonPhase == 5) usedFrame = 2;
        if (Main.moonPhase == 6 || Main.moonPhase == 7) usedFrame = 3;
        int height = 42;
        Rectangle frame = new(0, 0, text.Width, 0) {
            Y = (int)(usedFrame * height),
            Height = height
        };
        Vector2 position = Item.Center - Main.screenPosition;
        spriteBatch.Draw(text, position + Vector2.UnitY * 2f,
            frame, lightColor, rotation, Item.Size / 2f, scale, SpriteEffects.None, 0f);

        text = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
        spriteBatch.Draw(text, position + Vector2.UnitY * 2f,
            frame, Color.White, rotation, Item.Size / 2f, scale, SpriteEffects.None, 0f);

        return false;
    }

    public override bool CanUseItem(Player player) => ElathaAmuletCooldownHandler.ElathaAmuletCooldown <= 0;

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            if (player.statMana >= 30) {
                if (Main.netMode == NetmodeID.SinglePlayer) {
                    ChangeMoonPhase(player);
                }
                else {
                    MultiplayerSystem.SendPacket(new ElathaAmuletUsagePacket(player));
                }
            }
        }

        return base.UseItem(player);
    }

    private class ElathaAmuletCooldownHandler : ModSystem {
        public static short ElathaAmuletCooldown;

        public override void Load() {
            On_Main.UpdateTime_StartNight += On_Main_UpdateTime_StartNight;
        }

        private void On_Main_UpdateTime_StartNight(On_Main.orig_UpdateTime_StartNight orig, ref bool stopEvents) {
            orig(ref stopEvents);

            if (ElathaAmuletCooldown > 0) {
                ElathaAmuletCooldown--;
            }
        }

        public override void ClearWorld() {
            ElathaAmuletCooldown = 0;
        }

        public override void SaveWorldData(TagCompound tag) {
            tag[RoA.ModName + nameof(ElathaAmuletCooldown)] = ElathaAmuletCooldown;
        }

        public override void LoadWorldData(TagCompound tag) {
            ElathaAmuletCooldown = tag.GetShort(RoA.ModName + nameof(ElathaAmuletCooldown));
        }
    }

    internal static void ChangeMoonPhase(Player player) {
        if (ElathaAmuletCooldownHandler.ElathaAmuletCooldown > 0) {
            return;
        }

        ElathaAmuletCooldownHandler.ElathaAmuletCooldown = 6;

        Main.moonPhase++;
        if (Main.moonPhase > 7) {
            Main.moonPhase = 0;
        }

        string message = Language.GetText("Mods.RoA.World.ElathaAmuletUsage").ToString();
        Main.NewText(message, 225, 75, 75);
    }
}
