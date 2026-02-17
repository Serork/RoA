using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Back)]
sealed class Coffin : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Coffin");
        //Tooltip.SetDefault("Shoot bones in random positions on taking damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 32; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;

        Item.accessory = true;
        Item.defense = 3;

        Item.useTurn = true;
        Item.autoReuse = true;

        Item.useAnimation = Item.useTime = 15;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.createTile = ModContent.TileType<Tiles.Decorations.Coffin>();
        Item.consumable = true;

        Item.value = Item.sellPrice(0, 1, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<CoffinPlayer>().boneTrousle = true;
        //player.moveSpeed -= 0.2f;
    }
}

sealed class CoffinPlayer : ModPlayer {
    public bool boneTrousle;
    public sbyte hurtCount;

    public override void ResetEffects() => boneTrousle = false;

    public override void PostUpdate() {
    }

    private void SpawnBones() {
        if (Player.whoAmI == Main.myPlayer) {
            IEntitySource source = Player.GetSource_Misc("coffinspawn");
            int index = 0, max = 4;

            int damage = 24;
            float knockback = 7f;

            void spawn(int type) {
                Vector2 boneVel = Vector2.One.RotatedBy(MathHelper.Pi * (float)index / max + Main.rand.NextFloatRange(MathHelper.PiOver2));
                boneVel *= 2.5f + Main.rand.NextFloat(2.5f);
                Projectile.NewProjectile(source, Player.GetPlayerCorePoint().X, Player.GetPlayerCorePoint().Y, boneVel.X, boneVel.Y, type,
                    damage, knockback,
                    Player.whoAmI);
                index++;
            }
            spawn(ModContent.ProjectileType<BoneSkull>());
            spawn(ModContent.ProjectileType<BoneBody>());
            for (int i = 0; i < 2; i++) {
                spawn(ModContent.ProjectileType<BoneArm>());
                spawn(ModContent.ProjectileType<BoneLeg>());
            }
        }
    }

    public override void OnHurt(Player.HurtInfo info) {
        if (boneTrousle) {
            hurtCount++;
            if (hurtCount == 1) {
                SpawnBones();
                SoundEngine.PlaySound(SoundID.NPCHit2, Player.Center);
            }
            if (hurtCount > 2) hurtCount = 0;
        }
    }
}