using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class ElderTorch : ModItem {

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "EndOngoingTorchGodEvent")]
    public extern static void Player_EndOngoingTorchGodEvent(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RelightTorches")]
    public extern static void Player_RelightTorches(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "torchFunTimer")]
    public extern static ref int Player_torchFunTimer(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "numberOfTorchAttacksMade")]
    public extern static ref int Player_numberOfTorchAttacksMade(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "maxTorchAttacks")]
    public extern static ref int Player_maxTorchAttacks(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_torchAttackPosX")]
    public extern static ref int[] Player_torchAttackPosX(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_torchAttackPosY")]
    public extern static ref int[] Player_torchAttackPosY(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "unlitTorchX")]
    public extern static ref int[] Player_unlitTorchX(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "unlitTorchY")]
    public extern static ref int[] Player_unlitTorchY(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "luckyTorchCounter")]
    public extern static ref int Player_luckyTorchCounter(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "TorchAttack")]
    public extern static void Player_TorchAttack(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "torchGodCooldown")]
    public extern static ref int Player_torchGodCooldown(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_nextTorchLuckCheckCenter")]
    public extern static ref Vector2 Player__nextTorchLuckCheckCenter(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "UpdateTorchLuck_ConsumeCountersAndCalculate")]
    public extern static void Player_UpdateTorchLuck_ConsumeCountersAndCalculate(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "nearbyTorches")]
    public extern static ref int Player_nearbyTorches(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "nearbyTorch")]
    public extern static ref bool[] Player_nearbyTorch(Player self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "dryCoralTorch")]
    public extern static ref bool Player_dryCoralTorch(Player self);

    public override void Load() {
        On_Player.TorchAttack += On_Player_TorchAttack;
        On_Player.TryRecalculatingTorchLuck += On_Player_TryRecalculatingTorchLuck;
    }

    private void On_Player_TryRecalculatingTorchLuck(On_Player.orig_TryRecalculatingTorchLuck orig, Player self) {
        ref int luckyTorchCounter = ref Player_luckyTorchCounter(self);
        ref int torchGodCooldown = ref Player_torchGodCooldown(self);
        ref Vector2 _nextTorchLuckCheckCenter = ref Player__nextTorchLuckCheckCenter(self);
        if (self.happyFunTorchTime) {
            luckyTorchCounter = 0;
            Player_TorchAttack(self);
            return;
        }

        if (torchGodCooldown > 0)
            torchGodCooldown--;

        Vector2 nextTorchLuckCheckCenter = _nextTorchLuckCheckCenter;
        if (false | ((double)nextTorchLuckCheckCenter.Y < Main.worldSurface * 16.0) | self.dead) {
            Player_UpdateTorchLuck_ConsumeCountersAndCalculate(self);
            return;
        }

        ref int nearbyTorches = ref Player_nearbyTorches(self);

        int num = 1;
        int num2 = 40;
        int num3 = (int)nextTorchLuckCheckCenter.Y / 16 - num2;
        int value = (int)nextTorchLuckCheckCenter.X / 16 - num2;
        int value2 = (int)nextTorchLuckCheckCenter.X / 16 + num2;
        value = Utils.Clamp(value, 10, Main.maxTilesX - 10);
        value2 = Utils.Clamp(value2, 10, Main.maxTilesX - 10);

        ref bool[] nearbyTorch = ref Player_nearbyTorch(self);
        ref bool dryCoralTorch = ref Player_dryCoralTorch(self);

        for (int i = 0; i < num; i++) {
            int num4 = num3 + i + luckyTorchCounter * num;
            if (num4 < 10 || num4 > Main.maxTilesY - 10)
                continue;

            for (int j = value; j <= value2; j++) {
                Tile tile = Main.tile[j, num4];

                if (tile.TileType >= TileID.Count) {
                    if (tile.HasTile && TileID.Sets.Torch[tile.TileType])
                        self.NearbyModTorch.Add(tile.TileType);

                    //continue;
                }

                bool flag = tile.TileType == 4 || tile.TileType == ModContent.TileType<Tiles.Crafting.ElderTorch>();
                if (!tile.HasTile || !flag || tile.TileFrameX < 0 || tile.TileFrameY < 0)
                    continue;

                if (tile.TileFrameX < 66)
                    nearbyTorches++;

                int num5 = tile.TileFrameY / 22;
                if (num5 < TorchID.Count) {
                    nearbyTorch[num5] = true;
                    if (num5 == 17 && (tile.LiquidAmount == 0 || tile.LiquidType != 0))
                        dryCoralTorch = true;
                }
            }

            if (num4 >= (int)nextTorchLuckCheckCenter.Y / 16 + num2) {
                Player_UpdateTorchLuck_ConsumeCountersAndCalculate(self);
                return;
            }
        }

        luckyTorchCounter++;
    }

    private void On_Player_TorchAttack(On_Player.orig_TorchAttack orig, Player self) {
        if (self.whoAmI != Main.myPlayer)
            return;

        if ((double)self.position.Y < Main.worldSurface * 16.0) {
            Player_EndOngoingTorchGodEvent(self);
            return;
        }

        ref int torchFunTimer = ref Player_torchFunTimer(self);
        ref int numberOfTorchAttacksMade = ref Player_numberOfTorchAttacksMade(self);
        ref int[] _torchAttackPosX = ref Player_torchAttackPosX(null);
        ref int[] _torchAttackPosY = ref Player_torchAttackPosY(null);
        ref int[] unlitTorchX = ref Player_unlitTorchX(self);
        ref int[] unlitTorchY = ref Player_unlitTorchY(self);

        int maxTorchAttacks = Player_maxTorchAttacks(null);

        self.AddBuff(80, 2);
        torchFunTimer++;
        if (torchFunTimer <= 20)
            return;

        torchFunTimer = 0;
        int num = 0;
        int num2 = 100;
        int value = (int)self.Center.X / 16 - num2;
        int value2 = (int)self.Center.X / 16 + num2;
        int value3 = (int)self.Center.Y / 16 - num2;
        int value4 = (int)self.Center.Y / 16 + num2;
        int num3 = Utils.Clamp(value, 10, Main.maxTilesX - 10);
        value2 = Utils.Clamp(value2, 10, Main.maxTilesX - 10);
        value3 = Utils.Clamp(value3, 10, Main.maxTilesY - 10);
        value4 = Utils.Clamp(value4, 10, Main.maxTilesY - 10);
        for (int i = num3; i <= value2; i++) {
            for (int j = value3; j <= value4; j++) {
                Tile tile = Main.tile[i, j];
                if (tile != null && (tile.HasTile & (tile.TileType == 4 || tile.TileType == ModContent.TileType<Tiles.Crafting.ElderTorch>())) && tile.TileFrameX < 66) {
                    _torchAttackPosX[num] = i;
                    _torchAttackPosY[num] = j;
                    num++;
                    if (num >= _torchAttackPosX.Length)
                        break;
                }
            }

            if (num >= _torchAttackPosX.Length)
                break;
        }

        if (num == 0 || numberOfTorchAttacksMade >= maxTorchAttacks) {
            Player_RelightTorches(self);
            self.happyFunTorchTime = false;
            if (Main.netMode == 1)
                NetMessage.SendData(4, -1, -1, null, self.whoAmI);

            if (numberOfTorchAttacksMade >= 95) {
                int number = Item.NewItem(self.GetSource_Misc(context: "TorchGod"), (int)self.position.X, (int)self.position.Y, self.width, self.height, 5043);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number, 1f);
            }
        }
        else {
            if (num <= 0)
                return;

            int num4 = Main.rand.Next(num);
            int num5 = _torchAttackPosX[num4];
            int num6 = _torchAttackPosY[num4];
            if ((Main.tile[num5, num6].TileType == 4 || Main.tile[num5, num6].TileType == ModContent.TileType<Tiles.Crafting.ElderTorch>()) && Main.tile[num5, num6].TileFrameX < 66) {
                float num7 = 8f;
                int num8 = 20;
                if (num8 < 10)
                    num8 = 10;

                int num9 = (int)MathHelper.Clamp(Main.tile[num5, num6].TileFrameY / 22, 0f, TorchID.Count - 1);
                num9 = TorchID.Dust[num9];
                if (Main.tile[num5, num6].TileType == ModContent.TileType<Tiles.Crafting.ElderTorch>()) {
                    num9 = ModContent.DustType<ElderTorchDust>();
                }
                Main.tile[num5, num6].TileFrameX += 66;
                unlitTorchX[numberOfTorchAttacksMade] = num5;
                unlitTorchY[numberOfTorchAttacksMade] = num6;
                numberOfTorchAttacksMade++;
                NetMessage.SendTileSquare(-1, num5, num6);
                Vector2 vector = new Vector2(num5 * 16 + 8, num6 * 16);
                Vector2 vector2 = self.Center - vector;
                float num10 = vector2.Length();
                vector2.Normalize();
                vector2 *= num7;
                int num11 = Projectile.NewProjectile(self.GetSource_Misc(context: "TorchGod"), vector, vector2, 949, num8, 1f, self.whoAmI, num9, num10);
                Main.projectile[num11].ai[0] = num9;
                Main.projectile[num11].ai[1] = num10;
                Main.projectile[num11].netUpdate = true;
                if ((num == 1 && numberOfTorchAttacksMade >= 95) || numberOfTorchAttacksMade >= maxTorchAttacks)
                    torchFunTimer = -180;
            }
        }
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 100;

        ItemID.Sets.SingleUseInGamepad[Type] = true;
        ItemID.Sets.Torches[Type] = true;

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
    }

    public override void SetDefaults() {
        Item.DefaultToTorch(ModContent.TileType<Tiles.Crafting.ElderTorch>(), 0, false);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Torches;
    }

    public override void HoldItem(Player player) {
        if (player.wet) {
            return;
        }

        if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30)) {
            Dust dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, ModContent.DustType<ElderTorchDust>(), 0f, 0f, 100);
            if (!Main.rand.NextBool(3)) {
                dust.noGravity = true;
            }

            dust.velocity *= 0.3f;
            dust.velocity.Y -= 1.5f;
            dust.position = player.RotatedRelativePoint(dust.position);
        }

        Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
        Lighting.AddLight(position, 0.25f, 0.65f, 0.85f);
    }

    public override void PostUpdate() {
        if (!Item.wet) {
            Lighting.AddLight(Item.Center, 0.25f, 0.65f, 0.85f);
        }
    }
}
