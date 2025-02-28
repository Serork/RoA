using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class EbonwoodStaff : BaseRodItem<EbonwoodStaff.EbonwoodStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<EvilBranch>();

    protected override void SafeSetDefaults() {
        Item.SetSize(44);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);

        Item.rare = ItemRarityID.Green;
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class EbonwoodStaffBase : BaseRodProjectile {
        private Vector2 _mousePosition;

        //protected override byte TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2);

        protected override Vector2 CorePositionOffsetFactor() => new(0.05f, 0.05f);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override void SetSpawnProjectileSettings2(Player player, ref int damage, ref float knockBack) => damage = NatureWeaponHandler.GetBasePotentialDamage(Item, player);

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
                int num30 = Dust.NewDust(r.TopLeft(), r.Width, r.Height, flag ? ModContent.DustType<EvilStaff2>() : DustID.Ebonwood, 0f, 0f, 0, default);
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
                Dust dust = Dust.NewDustPerfect(corePosition + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                                flag ? ModContent.DustType<EvilStaff2>() : DustID.Ebonwood,
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
                Dust dust = Dust.NewDustPerfect(_mousePosition - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f),
                    flag ? DustID.Ebonwood : TileHelper.GetKillTileDust(mousePositionInTiles.X, mousePositionInTiles.Y, WorldGenHelper.GetTileSafely(mousePositionInTiles)));
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
}

sealed class ShadewoodStaff : BaseRodItem<ShadewoodStaff.ShadewoodStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<EvilBranch>();

    protected override void SafeSetDefaults() {
        Item.SetSize(44);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);

        Item.rare = ItemRarityID.Green;
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class ShadewoodStaffBase : BaseRodProjectile {
        private Vector2 _mousePosition;

        //protected override byte TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2);

        protected override Vector2 CorePositionOffsetFactor() => new(0.05f, 0.05f);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2)
            => ai2 = 1f;

        protected override void SetSpawnProjectileSettings2(Player player, ref int damage, ref float knockBack) => damage = NatureWeaponHandler.GetBasePotentialDamage(Item, player);

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
                int num30 = Dust.NewDust(r.TopLeft(), r.Width, r.Height, flag ? ModContent.DustType<EvilStaff1>() : DustID.Shadewood, 0f, 0f, 0, default);
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
                Dust dust = Dust.NewDustPerfect(corePosition + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                                flag ? ModContent.DustType<EvilStaff1>() : DustID.Shadewood,
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
                Dust dust = Dust.NewDustPerfect(_mousePosition - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f),
                    flag ? DustID.Shadewood : TileHelper.GetKillTileDust(mousePositionInTiles.X, mousePositionInTiles.Y, WorldGenHelper.GetTileSafely(mousePositionInTiles)));
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
}