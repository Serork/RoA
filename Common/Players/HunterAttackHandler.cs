using Microsoft.Xna.Framework;

using RoA.Common.BackwoodsSystems;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Projectiles;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class HunterAttackPlayer : ModPlayer {
    private int _cooldown;

    public override void PostUpdate() {
        if (Main.myPlayer != Player.whoAmI) {
            return;
        }

        if (_cooldown > 0) {
            _cooldown--;
            return;
        }

        void setUpPosition(Vector2 basePosition, ref Vector2 position) {
            position += Main.rand.RandomPointInArea(Main.screenWidth / 2f, Main.screenHeight / 2f);
            int attempts = 1000;
            bool flag = false;
            for (int j2 = (int)position.Y / 16; j2 < (int)position.Y / 16 + 2; j2++) {
                if (Main.tileSolid[WorldGenHelper.GetTileSafely((int)position.X / 16, j2).TileType]) {
                    flag = true;
                    break;
                }
            }
            while (Main.tileSolid[WorldGenHelper.GetTileSafely((int)position.X / 16, (int)position.Y / 16).TileType] ||
                Lighting.GetColor((int)position.X / 16, (int)position.Y / 16).ToVector3().Length() >= 0.5f ||
                Vector2.Distance(basePosition, position) < 200f || !flag) {
                position = basePosition;
                position += Main.rand.RandomPointInArea(Main.screenWidth / 2f, Main.screenHeight / 2f);
                if (--attempts <= 0) {
                    break;
                }
            }
        }
        if (!Main.dayTime && HunterSpawnSystem.ShouldSpawnHunterAttack && Main.rand.NextBool(450)) {
            if (Main.rand.NextChance(0.85)) {
                Player player = Player;
                if (!player.InModBiome<BackwoodsBiome>()) {
                    return;
                }

                int num = 40;
                num = Main.DamageVar(num, 0f - player.luck);
                float knockBack = 2f;
                Vector2 position = player.GetPlayerCorePoint();
                setUpPosition(player.GetPlayerCorePoint(), ref position);
                bool flag2 = position.Y / 16 < BackwoodsVars.FirstTileYAtCenter + 20;
                if (flag2 && Main.rand.NextBool(10)) {
                    if (Collision.CanHit(player.position, player.width, player.height, position, 0, 0)) {
                        int proj = Projectile.NewProjectile(new EntitySource_Misc("hunterattack"),
                            position.X, position.Y,
                            num, knockBack,
                            ModContent.ProjectileType<HunterProjectile1>(), num, knockBack, player.whoAmI, ai2: -1f);
                        _cooldown = 600;
                    }
                }
            }
            else {
                foreach (NPC npc in Main.ActiveNPCs) {
                    if (!(!npc.dontTakeDamage && npc.lifeMax > 5 && !npc.friendly && !npc.townNPC)) {
                        continue;
                    }

                    if (npc.target != 255 && Main.player[npc.target] != null && !Main.player[npc.target].InModBiome<BackwoodsBiome>()) {
                        continue;
                    }

                    int num = 40;
                    float knockBack = 2f;
                    Vector2 position = npc.Center;
                    setUpPosition(npc.Center, ref position);
                    bool flag2 = position.Y / 16 < BackwoodsVars.FirstTileYAtCenter + 20;
                    if (flag2 && Main.rand.NextBool(1)) {
                        if (Collision.CanHit(npc.position, npc.width, npc.height, position, 0, 0)) {
                            int proj = Projectile.NewProjectile(new EntitySource_Misc("hunterattack"),
                                position.X, position.Y,
                                num, knockBack,
                                ModContent.ProjectileType<HunterProjectile1>(), num, knockBack, Player.whoAmI, ai2: npc.whoAmI);
                            _cooldown = 600;
                        }
                    }
                }
            }
        }
    }
}
