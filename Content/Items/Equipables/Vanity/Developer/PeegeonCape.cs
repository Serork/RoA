using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Back)]
sealed class PeegeonCape : ModItem {
    private sealed class PeegeonCapeWingsLogic : ModPlayer {
        public bool IsWingsActive;

        private bool flapSound;

        public override void ResetEffects() {
            IsWingsActive = false;
        }

        public override void FrameEffects() {
            if (!IsWingsActive) {
                return;
            }

            Player.wingTimeMax = 150;

            if (((Player.velocity.Y == 0f || Player.sliding) && Player.releaseJump) || (Player.autoJump && Player.justJumped)) {
                Player.wingTime = Player.wingTimeMax;
            }

            bool flag22 = Player.wingFrame == 3;
            if (flag22) {
                if (!flapSound)
                    SoundEngine.PlaySound(SoundID.Item24, Player.position);

                flapSound = true;
            }
            else {
                flapSound = false;
            }
        }
    }


    private static int _wingsSlot = -1;

    private static string WingsTextureName => ResourceManager.EmptyTexture;
    private static string WingsLayerName => $"{nameof(PeegeonCape)}_Wings";

    public override void Load() => _wingsSlot = EquipLoader.AddEquipTexture(Mod, WingsTextureName, EquipType.Wings, name: WingsLayerName);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Peegeon's Cape");
        // Tooltip.SetDefault("Allows flight\n'Great for impersonating RoA devs?' Sure!");

        Item.ResearchUnlockCount = 1;

        ArmorIDs.Wing.Sets.Stats[_wingsSlot] = new WingStats(150, 7f);
    }

    public override void SetDefaults() {
        int width = 24; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.accessory = true;

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<PeegeonCapeWingsLogic>().IsWingsActive = true;

        player.noFallDmg = true;
        player.wings = -1;
        player.wingsLogic = _wingsSlot;
        player.flapSound = true;
    }
}