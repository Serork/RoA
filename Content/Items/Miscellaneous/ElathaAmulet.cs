using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Items.Miscellaneous;

sealed class ElathaAmulet : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Elatha Scepter");
		//Tooltip.SetDefault("Changes the phases of the Moon");
	}

	public override void SetDefaults() {
		Item.Size = new Vector2(16, 40);
        Item.rare = ItemRarityID.Green;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.reuseDelay = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item28;
        Item.mana = 30;
	}

    public override bool CanUseItem(Player player) => ElathaAmuletCooldownHandler.ElathaAmuletCooldown <= 0;

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            if (player.statMana >= 30) {
                if (Main.netMode == NetmodeID.SinglePlayer) {
                    ChangeMoonPhase(player);
                }
                else { 
                    MultiplayerSystem.SendPacket(new ElathaAmuletUsage(player));
                }
            }
        }

        return base.UseItem(player);
    }

    private sealed class ElathaAmuletCooldownHandler : ModSystem {
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
            tag[nameof(ElathaAmuletCooldown)] = ElathaAmuletCooldown;
        }

        public override void LoadWorldData(TagCompound tag) {
            ElathaAmuletCooldown = tag.GetShort(nameof(ElathaAmuletCooldown));
        }
    }

    internal static void ChangeMoonPhase(Player player) {
        if (ElathaAmuletCooldownHandler.ElathaAmuletCooldown > 0) {
            return;
        }

        ElathaAmuletCooldownHandler.ElathaAmuletCooldown = 8;

        Main.moonPhase++;
        if (Main.moonPhase > 7) {
            Main.moonPhase = 0;
        }

        string message = Language.GetText("Mods.RoA.World.ElathaAmuletUsage").ToString();
        Main.NewText(message, 225, 75, 75);
    }
}
