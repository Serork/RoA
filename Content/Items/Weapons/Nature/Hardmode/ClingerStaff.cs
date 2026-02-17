using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class ClingerStaff : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 38);
        Item.SetWeaponValues(40, 2f);
        Item.SetUsableValues(ItemUseStyleID.Swing, 40);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<ClingerHideway>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice(gold: 8));

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        //Vector2 getOffset() => Vector2.One.RotatedByRandom(MathHelper.TwoPi) * SPAWNOFFSET;
        Vector2 mousePosition = player.GetWorldMousePosition();
        Vector2 spawnPosition = player.GetPlayerCorePoint();
        int maxChecks = 30;
        while (maxChecks-- > 0 && !WorldGenHelper.SolidTile(spawnPosition.ToTileCoordinates())) {
            spawnPosition += spawnPosition.DirectionTo(mousePosition) * WorldGenHelper.TILESIZE;
        }
        position = spawnPosition;
        //foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<ClingerHideway>((checkProjectile) => checkProjectile.owner != player.whoAmI)) {
        //    while (projectile.Center.Distance(position) < SPAWNOFFSET) {
        //        position = mousePosition + getOffset();
        //    }
        //}

        UpdateMaxClingers(player);
    }

    private void UpdateMaxClingers(Player player) {
        IEnumerable<Projectile> list2 = TrackedEntitiesSystem.GetTrackedProjectile<ClingerHideway>(checkProjectile => checkProjectile.owner != player.whoAmI);
        List<Projectile> list = [];
        foreach (Projectile projectile in list2) {
            list.Add(projectile);
        }
        int num = 0;
        int count = list2.Count();
        while (list.Count > ClingerHideway.MAXAVAILABLE - 1 && ++num < count) {
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
}
