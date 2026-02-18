using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Defaults;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Hardmode;

sealed class Taproot : ModItem {
    public record struct TaprootNPCHitInfo(short NPCTypeToApplyIncreasedDamage, float FinalDamageModifier, int FlatBonusDamage = 0);

    public static readonly HashSet<TaprootNPCHitInfo> NPCsThatTakeIncreasedDamage =
        [//misc
		new TaprootNPCHitInfo(NPCID.Tim, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.RuneWizard, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.EyeofCthulhu, 2f, 0),
		new TaprootNPCHitInfo(NPCID.ServantofCthulhu, 3f, 0),
		new TaprootNPCHitInfo(NPCID.SkeletronPrime, 2f, 0),
		new TaprootNPCHitInfo(NPCID.TheDestroyer, 3f, 0),
		new TaprootNPCHitInfo(NPCID.Probe, 3f, 0),
		new TaprootNPCHitInfo(NPCID.Retinazer, 2f, 0),
		new TaprootNPCHitInfo(NPCID.Spazmatism, 2f, 0),
		new TaprootNPCHitInfo(NPCID.MoonLordHead, 5f, 0),
        new TaprootNPCHitInfo(NPCID.MoonLordHand, 5f, 0),
        new TaprootNPCHitInfo(NPCID.MoonLordCore, 5f, 0),
        new TaprootNPCHitInfo(NPCID.MoonLordFreeEye, 5f, 0),
		//dungeon
		new TaprootNPCHitInfo(NPCID.AngryBones, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.DarkCaster, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CursedSkull, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.DungeonSlime, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.SkeletronHead, 2f, 0),
		new TaprootNPCHitInfo(NPCID.SkeletronHand, 3f, 0),
		//dungeon HM
		new TaprootNPCHitInfo(NPCID.BlueArmoredBones, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.BlueArmoredBonesMace, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.BlueArmoredBonesNoPants, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.BlueArmoredBonesSword, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.RustyArmoredBonesAxe, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.RustyArmoredBonesFlail, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.RustyArmoredBonesSword, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.RustyArmoredBonesSwordNoArmor, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.HellArmoredBones, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.HellArmoredBonesSpikeShield, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.HellArmoredBonesMace, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.HellArmoredBonesSword, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.Paladin, 2f, 0),
		new TaprootNPCHitInfo(NPCID.Necromancer, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.RaggedCaster, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.DiabolistRed, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.DiabolistWhite, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.SkeletonCommando, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.SkeletonSniper, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.TacticalSkeleton, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.GiantCursedSkull, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BoneLee, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.DungeonSpirit, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CultistDevote, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CultistArcherBlue, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CultistBoss, 2f, 0),
		new TaprootNPCHitInfo(NPCID.AncientDoom, 3f, 0),
		new TaprootNPCHitInfo(NPCID.AncientCultistSquidhead, 3f, 0),
		new TaprootNPCHitInfo(NPCID.CultistDragonHead, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.CultistDragonBody1, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.CultistDragonBody2, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.CultistDragonBody3, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.CultistDragonBody4, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.CultistDragonTail, 1.6f, 0),
		//corruption
		new TaprootNPCHitInfo(NPCID.EaterofSouls, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CorruptGoldfish, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.DevourerHead, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.DevourerBody, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.DevourerTail, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.EaterofWorldsHead, 2f, 0),
		new TaprootNPCHitInfo(NPCID.EaterofWorldsBody, 2f, 0),
		new TaprootNPCHitInfo(NPCID.EaterofWorldsTail, 2f, 0),
		//corruption HM
		new TaprootNPCHitInfo(NPCID.Corruptor, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CorruptSlime, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.Slimeling, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.Slimer, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.SeekerHead, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.SeekerBody, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.SeekerTail, 1.6f, 0),
        new TaprootNPCHitInfo(NPCID.DarkMummy, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.Clinger, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CursedHammer, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BigMimicCorruption, 2f, 0),
		new TaprootNPCHitInfo(NPCID.DesertGhoulCorruption, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.PigronCorruption, 1.6f, 0),
		//crimson
		new TaprootNPCHitInfo(NPCID.BloodCrawler, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.Crimera, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.FaceMonster, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CrimsonGoldfish, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BrainofCthulhu, 2f, 0),
		new TaprootNPCHitInfo(NPCID.Creeper, 3f, 0),
		//crimson HM
		new TaprootNPCHitInfo(NPCID.Herpling, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.Crimslime, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BloodJelly, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BloodFeeder, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BloodMummy, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.CrimsonAxe, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.IchorSticker, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.FloatyGross, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.BigMimicCrimson, 2f, 0),
		new TaprootNPCHitInfo(NPCID.DesertGhoulCrimson, 1.6f, 0),
		new TaprootNPCHitInfo(NPCID.PigronCrimson, 1.6f, 0)
		];

    public override void SetDefaults() {
        Item.SetSizeValues(32, 60);

        {
            Item.autoReuse = true;
            Item.useStyle = 5;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
            Item.value = Item.buyPrice(0, 45);
            Item.rare = 8;
            Item.DamageType = DamageClass.Ranged;
        }

        Item.UseSound = SoundID.Item102;
        Item.crit = 7;
        Item.damage = 80;
        Item.knockBack = 3f;
        Item.shootSpeed = 7.75f;
        Item.useAnimation = 20;
        Item.useTime = 20;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        type = ModContent.ProjectileType<TaprootArrow>();
    }

    public override Vector2? HoldoutOffset() => new Vector2(0f, 0f);
}
