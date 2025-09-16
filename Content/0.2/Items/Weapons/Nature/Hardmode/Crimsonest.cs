using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class Crimsonest : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 36);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(-1, 40, autoReuse: true, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Bloodly>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());
        Item.UseSound = SoundID.NPCDeath19 with { Volume = 3f, Pitch = -0.2f };

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        player.bodyFrame.Y = player.bodyFrame.Height * 3;

        float num16 = 1f - (float)player.itemAnimation / (float)player.itemAnimationMax;
        num16 *= 3f;
        num16 = MathUtils.Clamp01(num16);
        num16 = Ease.CircOut(num16);
        num16 = Ease.SineIn(num16);
        CompositeArmStretchAmount compositeArmStretchAmount2 = CompositeArmStretchAmount.Full;

        float rotation = Utils.AngleLerp(-MathHelper.PiOver4 / 2f, MathHelper.PiOver4, num16) + MathHelper.PiOver4;
        rotation += MathHelper.PiOver4 * 0.75f;
        rotation *= -player.direction;
        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount2, rotation);
        player.SetCompositeArmBack(enabled: true, CompositeArmStretchAmount.Full, rotation);
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            player.GetModPlayer<Crimsonest_AttackEncounter>().AttackCount++;
        }

        return base.UseItem(player);
    }
}

sealed class Crimsonest_AttackEncounter : ModPlayer {
    public int AttackCount;
    public bool CanReveal;

    public override void ResetEffects() {
        CanReveal = AttackCount >= Bloodly.AMOUNTNEEDFORATTACK - 1;
        if (AttackCount >= Bloodly.AMOUNTNEEDFORATTACK) {
            AttackCount = 0;
        }
    }

    public override void PostUpdate() {
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<Bloodly>()] <= 0) {
            AttackCount = 0;
        }
    }

    public override void OnEnterWorld() {
        AttackCount = -1;
    }
}
