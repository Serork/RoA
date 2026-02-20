using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class ShadewoodStaff : CaneBaseItem<ShadewoodStaff.ShadewoodStaffBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<EvilBranch>();

    public static void SetDefaultsInner(Item item) {
        item.SetSizeValues(36);
        item.SetUsableValues(-1, 44, useSound: SoundID.Item7);
        item.SetWeaponValues(8, 4f);

        NatureWeaponHandler.SetPotentialDamage(item, 28);
        NatureWeaponHandler.SetFillingRateModifier(item, 0.2f);

        item.rare = ItemRarityID.Green;

        item.value = Item.sellPrice(0, 0, 40, 0);
    }

    protected override void SafeSetDefaults() => SetDefaultsInner(Item);

    public sealed class ShadewoodStaffBase : EvilStaffBase {
        protected override ushort Dust1Type => (ushort)ModContent.DustType<EvilStaff1>();
        protected override ushort Dust2Type => (ushort)DustID.Shadewood;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2)
            => ai2 = 1f;
    }
}

sealed class EbonwoodStaff : CaneBaseItem<EbonwoodStaff.EbonwoodStaffBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<EvilBranch>();

    protected override void SafeSetDefaults() {
        ShadewoodStaff.SetDefaultsInner(Item);

        Item.SetSizeValues(34, 38);
    }

    public sealed class EbonwoodStaffBase : EvilStaffBase {
        protected override ushort Dust1Type => (ushort)ModContent.DustType<EvilStaff2>();
        protected override ushort Dust2Type => (ushort)DustID.Ebonwood;
    }
}

abstract class EvilStaffBase : CaneBaseProjectile {
    private Vector2 _mousePosition;

    protected virtual ushort Dust1Type { get; }
    protected virtual ushort Dust2Type { get; }

    //protected override byte TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2);

    protected override Vector2 CorePositionOffsetFactor() => new(0.05f, 0.05f);

    protected override bool ShouldWaitUntilProjDespawn() => false;

    protected override void SetSpawnProjectileSettings2(Player player, ref int damage, ref float knockBack) => damage = NatureWeaponHandler.GetBasePotentialDamage(AttachedNatureWeapon!, player);

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteVector2(_mousePosition);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _mousePosition = reader.ReadVector2();
    }

    protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
        int num27 = Main.rand.Next(4, 10);
        for (float num29 = 0f; num29 < (float)num27; num29++) {
            Vector2 size = new(24f, 24f);
            Rectangle r = Utils.CenteredRectangle(corePosition, size);
            bool flag = !Main.rand.NextBool(3);
            int num30 = Dust.NewDust(r.TopLeft(), r.Width, r.Height, flag ? Dust1Type : Dust2Type, 0f, 0f, 0, default);
            Dust dust2 = Main.dust[num30];
            if (Main.rand.NextChance(0.5) || flag) {
                dust2.noLight = true;
            }
            dust2.velocity *= Main.rand.NextFloat(0.75f, 1.5f);
            Main.dust[num30].noGravity = true;
            Main.dust[num30].scale = 0.9f + Main.rand.NextFloat() * 1.2f;
            Main.dust[num30].fadeIn = Main.rand.NextFloat() * 1.2f * Main.rand.NextFloat(0.75f, 1f);
            dust2 = Main.dust[num30];
            dust2.scale *= Main.rand.NextFloat(0.75f, 1f) * 1.25f;
            if (num30 != 6000) {
                Dust dust12 = Dust.CloneDust(num30);
                dust2 = dust12;
                dust2.scale /= 2f;
                dust2 = dust12;
                dust2.fadeIn *= 0.85f;
            }
        }
    }

    protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
        for (int i = 0; i < 2; i++) {
            Vector2 randomPosition = Main.rand.NextVector2Unit();
            float areaSize = step * 2f;
            float speed = step;
            float scale = Math.Clamp(step * 1.25f, 0.35f, 0.85f) * 1.15f;
            bool flag = !Main.rand.NextBool(3);
            Dust dust = Dust.NewDustPerfect(corePosition - Vector2.One * 1f + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                            flag ? Dust1Type : Dust2Type,
                                            randomPosition.RotatedBy(player.direction * -MathHelper.PiOver2) * speed,
                                            Scale: scale);
            if (Main.rand.NextChance(0.5) || flag) {
                dust.noLight = true;
            }
            dust.noGravity = true;
            dust.velocity *= 0.4f;
        }

        if (player.whoAmI == Main.myPlayer) {
            EvilBranch.GetPos(player, out Point point, out Point point2);
            _mousePosition = point2.ToWorldCoordinates();
            Projectile.netUpdate = true;
        }
        if (Main.rand.NextChance(Math.Min(0.5f, 0.5f * step * 1.5f))) {
            bool flag = Main.rand.NextBool(3);
            Point mousePositionInTiles = _mousePosition.ToTileCoordinates();
            Dust dust = Dust.NewDustPerfect(_mousePosition - new Vector2(0f, -2f + Main.rand.NextFloat() * 4f),
                flag ? Dust2Type : TileHelper.GetKillTileDust(mousePositionInTiles.X, mousePositionInTiles.Y, WorldGenHelper.GetTileSafely(mousePositionInTiles)));
            if (Main.rand.NextChance(0.5)) {
                dust.noLight = true;
            }
            dust.velocity *= 0.5f + Main.rand.NextFloatRange(0.1f);
            dust.velocity.Y -= 1.5f * Main.rand.NextFloat();
            if (!flag) {
                dust.scale *= 0.9f + Main.rand.NextFloatRange(0.1f);
            }
            else {
                dust.scale *= 1.2f + Main.rand.NextFloatRange(0.1f);
            }
        }
    }
}