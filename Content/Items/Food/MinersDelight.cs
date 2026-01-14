using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Food;

sealed class MinersDelight : ModItem {
    private static readonly ushort[] _buffPool = [BuffID.Regeneration, BuffID.Swiftness, BuffID.Calm, BuffID.BiomeSight, BuffID.Dangersense, BuffID.Invisibility, BuffID.Lucky, BuffID.Mining, BuffID.NightOwl, BuffID.Shine, (ushort)ModContent.BuffType<Brightstone>()],
                                     _debuffPool = [BuffID.Poisoned, BuffID.OnFire, BuffID.Bleeding, BuffID.Darkness, BuffID.Weak, BuffID.Tipsy, BuffID.Stinky];

    private static bool _addingDebuffs = false;

    public override void Load() {
        On_Player.AddBuff_DetermineBuffTimeToAdd += On_Player_AddBuff_DetermineBuffTimeToAdd;
    }

    private int On_Player_AddBuff_DetermineBuffTimeToAdd(On_Player.orig_AddBuff_DetermineBuffTimeToAdd orig, Player self, int type, int time1) {
        if (_addingDebuffs) {
            return time1;
        }
        return orig(self, type, time1);
    }

    public override void SetStaticDefaults() {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] = [
            new Color(88, 74, 91),
            new Color(68, 57, 77)
        ];
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(38, 34, BuffID.WellFed2, 14400, useGulpSound: true);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.Blue1, Item.buyPrice(0, 1));
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            player.AddBuff(BuffID.WellFed2, 14400);

            void addBuffs() {
                int buffTime = MathUtils.SecondsToFrames(30);
                int firstBuff = Main.rand.NextFromList(_buffPool),
                    secondBuff = Main.rand.NextFromList(_buffPool);
                while (secondBuff == firstBuff) {
                    secondBuff = Main.rand.NextFromList(_buffPool);
                }
                player.AddBuff(firstBuff, buffTime);
                player.AddBuff(secondBuff, buffTime);
            }
            void addDebuffs() {
                _addingDebuffs = true;
                int debuffTime = MathUtils.SecondsToFrames(10);
                int firstDebuff = Main.rand.NextFromList(_debuffPool),
                    secondDebuff = Main.rand.NextFromList(_debuffPool);
                while (secondDebuff == firstDebuff) {
                    secondDebuff = Main.rand.NextFromList(_debuffPool);
                }
                player.AddBuff(firstDebuff, debuffTime);
                player.AddBuff(secondDebuff, debuffTime);
                _addingDebuffs = false;
            }
            addBuffs();
            addDebuffs();
            return true;
        }

        return base.UseItem(player);
    }
}
