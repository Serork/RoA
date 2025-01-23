using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Content.Dusts;
using RoA.Core.Utility;
using RoA.Utilities;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Waist)]
sealed class FeathersInABottle : NatureItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 22; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.accessory = true;
        Item.rare = ItemRarityID.Green;
        Item.value = 50000;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetJumpState<FeathersInABottleExtraJump>().Enable();

    internal sealed class FeathersInABottleExtraJump : ExtraJump {
        public override Position GetDefaultPosition() => AfterBottleJumps;

        public override bool CanStart(Player player) => player.GetModPlayer<WreathHandler>().HasEnough(0.25f);

        public override float GetDurationMultiplier(Player player) => 1f;

        public override void UpdateHorizontalSpeeds(Player player) {
            player.runAcceleration *= 2.25f;
            player.maxRunSpeed *= 2f;
        }

        public override void OnStarted(Player player, ref bool playSound) {
            var handler = player.GetModPlayer<WreathHandler>();
            handler.Consume(0.25f);

            int offsetY = player.height;
            if (player.gravDir == -1f)
                offsetY = 0;

            offsetY -= 16;

            int variant = 0;
            if (handler.IsPhoenixWreath) {
                variant = 2;
            }
            if (handler.SoulOfTheWoods) {
                variant = 1;
            }
            for (int i = 0; i < 10; i++) {
                Dust dust = Dust.NewDustDirect(player.position + new Vector2(-34f, offsetY), 102, 32, ModContent.DustType<FeatherDust>(), -player.velocity.X * 0.5f, player.velocity.Y * 0.5f, variant, 
                    default, 1f);
                dust.velocity = dust.velocity * 0.5f - player.velocity * new Vector2(0.1f, 0.3f);
                dust.customData = Main.rand.NextFloatRange(50f);
            }

            SpawnCloudPoof(player, player.Top + new Vector2(-16f, offsetY));
            SpawnCloudPoof(player, player.position + new Vector2(-36f, offsetY));
            SpawnCloudPoof(player, player.TopRight + new Vector2(4f, offsetY));
        }

        public override void OnEnded(Player player) {
            player.controlJump = false;
            ref ExtraJumpState state = ref player.GetJumpState(this);
            if (state.Enabled) {
                OnRefreshed(player);
                PlayerLoader.OnExtraJumpRefreshed(this, player);
                state.Available = true;
            }
        }

        private static void SpawnCloudPoof(Player player, Vector2 position) {
            string variant = string.Empty;
            var handler = player.GetModPlayer<WreathHandler>();
            if (handler.IsPhoenixWreath) {
                variant = "1";
            }
            if (handler.SoulOfTheWoods) {
                variant = "2";
            }
            Gore gore = Gore.NewGoreDirect(player.GetSource_FromThis(), position, -player.velocity, ($"FeatherinABottleGore{1 + Main.rand.Next(3)}" + variant).GetGoreType());
            gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
            gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;
        }

        public override void ShowVisuals(Player player) {
        }
    }
}