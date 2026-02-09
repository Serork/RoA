using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class ChainedCloud : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 28);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsChainedCloudEffectActive = true;

        player.GetJumpState<ChainedCloudExtraJump>().Enable();
        player.GetJumpState<ChainedCloudExtraJump2>().Enable();
    }

    class ChainedCloudExtraJump2 : ChainedCloudExtraJump {

    }
    class ChainedCloudExtraJump : ExtraJump {
        public override Position GetDefaultPosition() => AfterBottleJumps;

        public override bool CanStart(Player player) => player.GetCommon().IsChainedCloudEffectActive;

        public override float GetDurationMultiplier(Player player) => 0.75f;

        public override void UpdateHorizontalSpeeds(Player player) {

        }

        public override void ShowVisuals(Player player) {
            int num = player.height;
            if (player.gravDir == -1f)
                num = -6;

            int num2 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)num), player.width + 8, 4, 16, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
            Main.dust[num2].velocity.X = Main.dust[num2].velocity.X * 0.5f - player.velocity.X * 0.1f;
            Main.dust[num2].velocity.Y = Main.dust[num2].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
        }

        private void UpdateMaxCloudPlatforms(Player player) {
            IEnumerable<Projectile> list2 = TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatform>(checkProjectile => checkProjectile.owner != player.whoAmI);
            List<Projectile> list = [];
            foreach (Projectile projectile in list2) {
                list.Add(projectile);
            }
            int num = 0;
            int count = list2.Count();
            while (list.Count >= 2 && ++num < count) {
                Projectile projectile = list[0];
                for (int j = 1; j < list.Count; j++) {
                    if (list[j].timeLeft < projectile.timeLeft) {
                        projectile = list[j];
                    }
                }
                projectile.Kill();
                list.Remove(projectile);
            }
            TrackedEntitiesSystem.UpdateTrackedEntityLists();
        }

        public override void OnStarted(Player player, ref bool playSound) {
            UpdateMaxCloudPlatforms(player);
            if (player.IsLocal()) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<CloudPlatform>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("chainedcloudjump")) {
                    Position = player.Bottom.ToTileCoordinates().ToWorldCoordinates()
                });
            }

            //int num22 = player.height;
            //if (player.gravDir == -1f)
            //    num22 = 0;

            //for (int num23 = 0; num23 < 10; num23++) {
            //    int num24 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num22 - 16f), 102, 32, 16, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
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

        public override void OnEnded(Player player) {

        }
    }
}
