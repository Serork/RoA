using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class WeepingTulip : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Weeping Tulip");
        //Tooltip.SetDefault("Launches an explosive enchanted petal");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        int width = 34; int height = 34;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 26;
        Item.autoReuse = false;

        Item.noMelee = true;

        Item.damage = 10;
        Item.knockBack = 1.5f;

        Item.value = Item.sellPrice(0, 0, 80, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item65;

        Item.shootSpeed = 8f;
        Item.shoot = ModContent.ProjectileType<TulipPetalOld>();

        NatureWeaponHandler.SetPotentialDamage(Item, 30);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.15f);
    }

    //public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
    //    Vector2 velocity2 = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
    //    position += velocity2 * 25;
    //}

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float offset = Item.width * 1.4f;
        if (!Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * offset * 1.1f, 0, 0)) {
            return false;
        }

        position += Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero) * offset;
        velocity = position.DirectionTo(player.GetWorldMousePosition()) * velocity.Length();
        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, 2);
        if (Main.rand.NextChance(0.6)) {
            float offset2 = 6f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = position + randomOffset;

            //ushort dustType = CoreDustType();
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
                                            Alpha: 2);
            dust.customData = Main.rand.NextFloatRange(50f);
        }
        return false;
    }
}

sealed class SweetTulip : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sweet Tulip");
        //Tooltip.SetDefault("Launches a bee attracting petal");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        int width = 32; int height = 34;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 30;
        Item.autoReuse = false;

        Item.noMelee = true;

        Item.damage = 11;
        Item.knockBack = 1.5f;

        Item.value = Item.sellPrice(0, 0, 40, 0);

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item65;

        Item.shootSpeed = 8f;
        Item.shoot = ModContent.ProjectileType<TulipPetalOld>();

        NatureWeaponHandler.SetPotentialDamage(Item, 26);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.35f);
    }

    //public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
    //    Vector2 velocity2 = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
    //    position += velocity2 * 25;
    //}

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float offset = Item.width * 1.4f;
        if (!Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * offset * 1.1f, 0, 0)) {
            return false;
        }

        position += Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero) * offset;
        velocity = position.DirectionTo(player.GetWorldMousePosition()) * velocity.Length();
        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, 1);
        if (Main.rand.NextChance(0.6)) {
            float offset2 = 6f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = position + randomOffset;

            //ushort dustType = CoreDustType();
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
                                            Alpha: 1);
            dust.customData = Main.rand.NextFloatRange(50f);
        }
        return false;
    }
}

sealed class ExoticTulip : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Exotic Tulip");
        //Tooltip.SetDefault("Launches a petal with a flowery trail");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        int width = 34; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 40;
        Item.autoReuse = false;

        Item.noMelee = true;

        Item.damage = 4;
        Item.knockBack = 1.5f;

        Item.value = Item.sellPrice(0, 0, 25, 0);

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item65;

        Item.shootSpeed = 8f;
        Item.shoot = ModContent.ProjectileType<TulipPetalOld>();

        NatureWeaponHandler.SetPotentialDamage(Item, 18);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    //public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
    //    Vector2 velocity2 = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
    //    position += velocity2 * 25;
    //}

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float offset = Item.width * 1.4f;
        if (!Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * offset * 1.1f, 0, 0)) {
            return false;
        }

        position += Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero) * offset;
        velocity = position.DirectionTo(player.GetWorldMousePosition()) * velocity.Length();
        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, 0);
        if (Main.rand.NextChance(0.6)) {
            float offset2 = 6f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = position + randomOffset;

            //ushort dustType = CoreDustType();
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
                                            Alpha: 0);
            dust.customData = Main.rand.NextFloatRange(50f);
        }
        return false;
    }
}

//sealed class ExoticTulip : TulipBaseItem<ExoticTulip.ExoticTulipBase> {
//    protected override void SafeSetDefaults() {
//        Item.SetWeaponValues(6, 1.5f);
//        Item.SetUsableValues(-1, 60, useSound: SoundID.Item65);

//        NatureWeaponHandler.SetPotentialDamage(Item, 20);

//        base.SafeSetDefaults();
//    }

//    public sealed class ExoticTulipBase : TulipBase {
//        protected override ushort CoreDustType() => (ushort)DustID.PurpleTorch;

//        protected override byte DustFrameXUsed() => 0;
//    }
//}

//sealed class SweetTulip : TulipBaseItem<SweetTulip.SweepTulipBase> {
//    protected override void SafeSetDefaults() {
//        Item.SetWeaponValues(6, 1.5f);
//        Item.SetUsableValues(-1, 60, useSound: SoundID.Item65);

//        NatureWeaponHandler.SetPotentialDamage(Item, 20);

//        base.SafeSetDefaults();
//    }

//    public sealed class SweepTulipBase : TulipBase {
//        protected override ushort CoreDustType() => (ushort)DustID.YellowTorch;

//        protected override byte DustFrameXUsed() => 1;
//    }
//}

//sealed class WeepingTulip : TulipBaseItem<WeepingTulip.WeepingTulipBase> {
//    protected override void SafeSetDefaults() {
//        Item.SetWeaponValues(6, 1.5f);
//        Item.SetUsableValues(-1, 45, useSound: SoundID.Item65);

//        NatureWeaponHandler.SetPotentialDamage(Item, 20);

//        base.SafeSetDefaults();
//    }

//    public sealed class WeepingTulipBase : TulipBase {
//        protected override ushort CoreDustType() => (ushort)DustID.BlueTorch;

//        protected override byte DustFrameXUsed() => 2;
//    }
//}

//abstract class TulipBaseItem<T> : CaneBaseItem<T> where T : CaneBaseProjectile {
//    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<TulipFlower>();

//    protected override void SafeSetDefaults() {
//        Item.SetSizeValues(34);

//        NatureWeaponHandler.SetFillingRate(Item, 0.4f);
//    }
//}

//abstract class TulipBase : CaneBaseProjectile {
//    public sealed class TulipBaseExtraData : ModPlayer {
//        public Vector2 TempMousePosition { get; set; } = Vector2.Zero;

//        public Vector2 SpawnPositionRandomlySelected => GetTilePosition(Player, TempMousePosition).ToWorldCoordinates();
//        public Vector2 SpawnPositionMid => GetTilePosition(Player, TempMousePosition, false).ToWorldCoordinates();
//    }

//    private TulipBaseExtraData TulipBaseData => Main.player[Projectile.owner].GetModPlayer<TulipBaseExtraData>();

//    protected abstract ushort CoreDustType();

//    protected abstract byte DustFrameXUsed();

//    protected override Vector2 CorePositionOffsetFactor() => new(0.08f, 0.13f);

//    protected override bool ShouldStopUpdatingRotationAndDirection() => false;

//    protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
//        spawnPosition = Main.MousePosition;
//        ai0 = DustFrameXUsed();
//    }

//    protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
//        float offset = 10f;
//        Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition + randomOffset;
//        ushort dustType = CoreDustType();
//        float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 1.25f * Ease.ExpoInOut(Math.Max(step, 0.25f)) + 0.25f;
//        Dust dust = Dust.NewDustPerfect(spawnPosition, dustType, Scale: MathHelper.Clamp(velocityFactor * 1.25f, 1f, 1.75f));
//        dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
//        dust.velocity *= 0.9f;
//        dust.noGravity = true;

//        if (player.whoAmI == Main.myPlayer) {
//            if (step <= 0.5f) {
//                bool isWeepingTulip = DustFrameXUsed() == 2;
//                TulipBaseData.TempMousePosition = isWeepingTulip ? player.GetViableMousePosition(480f, 300f) : player.GetViableMousePosition(240f, 150f);
//            }
//            Projectile.netUpdate = true;
//        }

//        SpawnGroundDusts(dustType, TulipBaseData, velocityFactor);
//    }

//    protected override void SpawnCoreDustsWhileShotProjectileIsActive(float step, Player player, Vector2 corePosition) => SpawnGroundDusts(CoreDustType(), TulipBaseData, 1f);

//    protected override void SpawnDustsWhenReady(Player player, Vector2 corePosition) {
//        for (int i = 0; i < 12; i++) {
//            float offset = 10f;
//            Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset),
//                    spawnPosition = corePosition + randomOffset;

//            ushort dustType = CoreDustType();
//            byte frameX = DustFrameXUsed();
//            Dust dust = Dust.NewDustPerfect(spawnPosition,
//                                            ModContent.DustType<Dusts.Tulip>(),
//                                            (spawnPosition - corePosition).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
//                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
//                                            Alpha: frameX);
//            dust.customData = Main.rand.NextFloatRange(50f);
//        }
//    }

//    public override void SendExtraAI(BinaryWriter writer) {
//        base.SendExtraAI(writer);

//        writer.WriteVector2(TulipBaseData.TempMousePosition);
//    }

//    public override void ReceiveExtraAI(BinaryReader reader) {
//        base.ReceiveExtraAI(reader);

//        TulipBaseData.TempMousePosition = reader.ReadVector2();
//    }

//    public static void SpawnGroundDusts(ushort dustType, TulipBaseExtraData tulipBaseExtraData, float velocityFactor) {
//        Vector2 position = tulipBaseExtraData.SpawnPositionRandomlySelected, velocity = (tulipBaseExtraData.TempMousePosition + Main.rand.NextVector2Circular(8f, 8f) - position).SafeNormalize(-Vector2.UnitY) * 3f * velocityFactor;
//        Dust dust = Dust.NewDustPerfect(position - Vector2.UnitY * 8f + Main.rand.NextVector2Circular(8f, 8f),
//                                        dustType,
//                                        Scale: Main.rand.NextFloat(1.5f, 2f));
//        dust.velocity = velocity;
//        dust.velocity *= 0.9f;
//        dust.noGravity = true;
//    }

//    // adapted vanilla
//    public static Point GetTilePosition(Player player, Vector2 targetSpot, bool randomlySelected = true) {
//        Point point;
//        Vector2 center = player.Center;
//        Vector2 endPoint = targetSpot;
//        int samplesToTake = 3;
//        float samplingWidth = 4f;
//        Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out Vector2 vectorTowardsTarget, out float[] samples);
//        float num = float.PositiveInfinity;
//        for (int i = 0; i < samples.Length; i++) {
//            if (samples[i] < num) {
//                num = samples[i];
//            }
//        }

//        targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
//        point = targetSpot.ToTileCoordinates();
//        while (!WorldGen.SolidTile2(point.X, point.Y)) {
//            point.Y++;
//        }
//        point.Y -= 1;
//        Rectangle value = new Rectangle(point.X, point.Y, 1, 1);
//        value.Inflate(1, 1);
//        Rectangle value2 = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
//        value2.Inflate(-40, -40);
//        value = Rectangle.Intersect(value, value2);
//        List<Point> list = new List<Point>();
//        for (int j = value.Left; j <= value.Right; j++) {
//            for (int k = value.Top; k <= value.Bottom; k++) {
//                if (!WorldGen.SolidTile2(j, k)) {
//                    continue;
//                }
//                list.Add(new Point(j, k));
//            }
//        }
//        int index = Main.rand.Next(list.Count);
//        return randomlySelected ? list[index] : list[list.Count / 2];
//    }
//}