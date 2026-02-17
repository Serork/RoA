using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Neck)]
sealed class SoulOfTheWoods : NatureItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BloodshedAxe>();
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 38;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 2, 25, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().SoulOfTheWoods = true;

        int type = ModContent.ProjectileType<RootRing>();
        int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(40);
        int knockback = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(0f);
        if (player.GetWreathHandler().IsFull2 && player.ownedProjectileCounts[type] < 1) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "FireWoosh") { Volume = 0.75f }, player.Center);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves1") { Volume = 0.75f, Pitch = -0.3f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.75f, Pitch = 0.3f }, player.Center);
            Projectile.NewProjectile(player.GetSource_Accessory(Item), player.GetPlayerCorePoint(), Vector2.Zero, type,
               damage, knockback, player.whoAmI);
        }
    }
}