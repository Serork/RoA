using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class ThunderKingsGrace : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 28);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsThunderKingsGraceEffectActive = true;

        player.GetJumpState<ThunderKingsGraceExtraJump>().Enable();
    }

    public class ThunderKingsGraceExtraJump : ExtraJump {
        public override Position GetDefaultPosition() => AfterBottleJumps;

        public static bool CanBeActive(Player player) => !player.GetFormHandler().IsInADruidicForm && !player.GetWreathHandler().StartSlowlyIncreasingUntilFull && player.GetWreathHandler().HasEnough(0.25f) &&
            player.GetCommon().IsThunderKingsGraceEffectActive;

        public override bool CanStart(Player player) => CanBeActive(player);

        public override float GetDurationMultiplier(Player player) => 1f;

        public override void UpdateHorizontalSpeeds(Player player) {
            player.runAcceleration *= 2.25f;
            player.maxRunSpeed *= 2f;
        }

        public override void ShowVisuals(Player player) {
            int num = player.height;
            if (player.gravDir == -1f)
                num = -6;

            int num2 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)num), player.width + 8, 4, ModContent.DustType<ChainedCloudDust>(), (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 0, default(Color), 1.5f);
            Main.dust[num2].velocity.X = Main.dust[num2].velocity.X * 0.5f - player.velocity.X * 0.1f;
            Main.dust[num2].velocity.Y = Main.dust[num2].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
        }

        public override void OnEnded(Player player) {
            ref ExtraJumpState state = ref player.GetJumpState(this);
            state.Available = true;
        }

        public override void OnStarted(Player player, ref bool playSound) {
            var handler = player.GetWreathHandler();
            handler.Consume(0.25f);

            //playSound = false;

            OnJumpEffects(player);

            if (player.IsLocal()) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<CloudPlatformAngry>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("chainedcloudjump")) {
                    Position = player.Bottom.ToTileCoordinates().ToWorldCoordinates()
                });
            }

            //int num22 = player.height;
            //if (player.gravDir == -1f)
            //    num22 = 0;

            //for (int num23 = 0; num23 < 10; num23++) {
            //    int num24 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num22 - 16f), 102, 32, ModContent.DustType<ChainedCloudDust>(), (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
            //    Main.dust[num24].velocity.X = Main.dust[num24].velocity.X * 0.5f - player.velocity.X * 0.1f;
            //    Main.dust[num24].velocity.Y = Main.dust[num24].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
            //}

            //int num25 = Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 16f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
            //Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
            //Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
            //num25 = Gore.NewGore(new Vector2(player.position.X - 36f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
            //Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
            //Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
            //num25 = Gore.NewGore(new Vector2(player.position.X + (float)player.width + 4f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
            //Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
            //Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
        }

        public static void OnJumpEffects(Player player) {

        }
    }
}
