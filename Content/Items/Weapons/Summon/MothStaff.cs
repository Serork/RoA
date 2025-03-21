using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Summon;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

sealed class MothStaff : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Moth Staff");
        //Tooltip.SetDefault("Summöns a moth to fight for you");
        ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DoubleFocusCharm>();
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "FreeUpPetsAndMinions")]
    public extern static void Player_FreeUpPetsAndMinions(Player player, Item sItem);

    public override void SetDefaults() {
        int width = 40; int height = 36;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.None;
        Item.holdStyle = ItemHoldStyleID.HoldFront;
        Item.useTime = Item.useAnimation = 30;
        Item.autoReuse = false;

        Item.mana = 10;
        Item.channel = true;
        Item.noMelee = true;

        Item.DamageType = DamageClass.Summon;
        Item.damage = 15;
        Item.knockBack = 4f;

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item77;

        Item.shoot = ModContent.ProjectileType<Moth>();
        Item.buffType = ModContent.BuffType<Buffs.Moth>();

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        player.AddBuff(Item.buffType, 2);
        player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);

        return false;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (player.itemTime == 0) {
            player.ApplyItemTime(Item);
        }
        UpdatePositionInPlayersHand(player);
    }

    internal static void OnUse_Effects(Player player, Item item) {
        player.ApplyItemAnimation(item);
        SoundEngine.PlaySound(item.UseSound, player.Center);

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 5, player.Center));
        }

        Vector2 position = GetDustsPosition(player);
        for (int num615 = 0; num615 < 16; num615++) {
            int num616 = Dust.NewDust(position - Vector2.UnitY * 6f, 4, 4,
                 DustID.Flare, 0f, 0f, 0, new Color(255, 255, 255), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[num616].noGravity = true;
            Dust dust2 = Main.dust[num616];
            dust2.scale *= 1.25f;
            dust2 = Main.dust[num616];
            dust2.velocity *= Main.rand.NextFloat(2f, 4f);
        }
    }

    public override void HoldStyle(Player player, Rectangle heldItemFrame) {
        if (player.altFunctionUse == 1 && player.ItemTimeIsZero) {
            if (player.controlUseItem) {
                OnUse_Effects(player, Item);
                player.MinionNPCTargetAim(doNotDisableIfTheTargetIsTheSame: false);
            }
        }
        else {
            if (player.controlUseItem && Main.mouseLeft && player.ItemTimeIsZero) {
                OnUse_Effects(player, Item);

                Player_FreeUpPetsAndMinions(player, Item);

            }
        }
        UpdatePositionInPlayersHand(player);
    }

    private static Vector2 GetDustsPosition(Player player) => new Vector2(player.MountedCenter.X + 30f * player.direction - (player.direction != 1 ? 8f : 0f), player.itemLocation.Y + (16f - 32f - 2f) * player.gravDir);

    private static void UpdatePositionInPlayersHand(Player player) {
        player.itemLocation.X = player.MountedCenter.X + 4f * player.direction;
        player.itemLocation.Y = player.MountedCenter.Y + 9f;
        player.itemRotation = 0f;
    }

    public override void HoldItem(Player player) {
        if (player.itemAnimation < 10) {
            Vector2 position = GetDustsPosition(player);
            Lighting.AddLight(position, 0.4f, 0.2f, 0f);
            if (Main.rand.Next(4) == 0) {
                Dust dust = Dust.NewDustDirect(position, 4, 4, DustID.Flare, 0f, -10f * player.gravDir, 0, new Color(255, 255, 255), Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }
    }
}
