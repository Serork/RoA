using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class TarDroplet : ModGore {
    public override void SetStaticDefaults() {
        ChildSafety.SafeGore[Type] = true;
        GoreID.Sets.LiquidDroplet[Type] = true;
    }

    public override void OnSpawn(Gore gore, IEntitySource source) {
        gore.numFrames = 15;
        gore.behindTiles = true;
        gore.timeLeft = Gore.goreTime * 3;
    }

    // adapted vanilla
    public override bool Update(Gore gore) {
        if ((double)gore.position.Y < Main.worldSurface * 16.0 + 8.0)
            gore.alpha = 0;
        else
            gore.alpha = 100;

        int animationSpeed = 4;
        gore.frameCounter++;
        // on tile
        if (gore.frame <= 4) {
            int num3 = (int)(gore.position.X / 16f);
            int num4 = (int)(gore.position.Y / 16f) - 1;
            if (WorldGen.InWorld(num3, num4) && !Main.tile[num3, num4].HasTile)
                gore.active = false;

            if (gore.frame == 0)
                animationSpeed = 24 + Main.rand.Next(256);

            if (gore.frame == 1)
                animationSpeed = 24 + Main.rand.Next(256);

            if (gore.frame == 2)
                animationSpeed = 24 + Main.rand.Next(256);

            if (gore.frame == 3)
                animationSpeed = 24 + Main.rand.Next(96);

            if (gore.frame == 5)
                animationSpeed = 16 + Main.rand.Next(64);

            //if (gore.type == 716)
            //    animationSpeed *= 2;

            //if (gore.type == 717)
            //    animationSpeed *= 4;

            animationSpeed *= 2;

            //if ((type == 943 || (type >= 1160 && type <= 1162)) && frame < 6)
            //    animationSpeed = 4;

            if (gore.frameCounter >= animationSpeed) {
                gore.frameCounter = 0;
                gore.frame++;
                if (gore.frame == 5) {
                    int num5 = Gore.NewGore(null, gore.position, gore.velocity, gore.type);
                    Main.gore[num5].frame = 9;
                    Main.gore[num5].velocity *= 0f;
                }
            }
        }
        // start falling
        else if (gore.frame <= 6) {
            animationSpeed = 8;
            //if (gore.type == 716)
            //    animationSpeed *= 2;

            //if (gore.type == 717)
            //    animationSpeed *= 3;
            animationSpeed *= 3;

            if (gore.frameCounter >= animationSpeed) {
                gore.frameCounter = 0;
                gore.frame++;
                if (gore.frame == 7)
                    gore.active = false;
            }
        }
        // falls
        else if (gore.frame <= 9) {
            animationSpeed = 6;
            //if (gore.type == 716) {
            //    animationSpeed = (int)((double)animationSpeed * 1.5);
            //    gore.velocity.Y += 0.175f;
            //}
            //else if (gore.type == 717) {
            //    animationSpeed *= 2;
            //    gore.velocity.Y += 0.15f;
            //}
            //else if (gore.type == 943) {
            //    animationSpeed = (int)((double)animationSpeed * 1.5);
            //    gore.velocity.Y += 0.2f;
            //}
            //else {
            //    gore.velocity.Y += 0.2f;
            //}
            animationSpeed *= 2;
            gore.velocity.Y += 0.125f;

            if ((double)gore.velocity.Y < 0.5)
                gore.velocity.Y = 0.5f;

            if (gore.velocity.Y > 12f)
                gore.velocity.Y = 12f;

            if (gore.frameCounter >= animationSpeed) {
                gore.frameCounter = 0;
                gore.frame++;
            }

            if (gore.frame > 9)
                gore.frame = 7;
        }
        // fallen
        else {
            //if (type == 716)
            //    animationSpeed *= 2;
            //else if (type == 717)
            //    animationSpeed *= 6;
            animationSpeed *= 2;

            gore.velocity.Y += 0.1f;
            if (gore.frameCounter >= animationSpeed) {
                gore.frameCounter = 0;
                gore.frame++;
            }

            gore.velocity *= 0f;
            if (gore.frame > 14)
                gore.active = false;
        }

        Vector2 vector2 = gore.velocity;
        gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14);
        if (gore.velocity != vector2) {
            if (gore.frame < 10) {
                gore.frame = 10;
                gore.frameCounter = 0;
                //if (type != 716 && type != 717 && type != 943 && (type < 1160 || type > 1162))
                //    SoundEngine.PlaySound(39, (int)position.X + 8, (int)position.Y + 8, Main.rand.Next(2));
            }
        }
        else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14)) {
            if (gore.frame < 10) {
                gore.frame = 10;
                gore.frameCounter = 0;
                //if (type != 716 && type != 717 && type != 943 && (type < 1160 || type > 1162))
                //    SoundEngine.PlaySound(39, (int)position.X + 8, (int)position.Y + 8, 2);

                ((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(gore.position + new Vector2(8f, 8f));
            }

            int num34 = (int)(gore.position.X + 8f) / 16;
            int num35 = (int)(gore.position.Y + 14f) / 16;
            if (Main.tile[num34, num35] != null && Main.tile[num34, num35].LiquidAmount > 0) {
                gore.velocity *= 0f;
                gore.position.Y = num35 * 16 - (int)Main.tile[num34, num35].LiquidAmount / 16;
            }
        }

        gore.position += gore.velocity;

        return false;
    }
}
