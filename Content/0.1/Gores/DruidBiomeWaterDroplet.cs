using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class DruidBiomeWaterDroplet : ModGore {
    public override void OnSpawn(Gore gore, IEntitySource source) {
        gore.numFrames = 15;
        gore.behindTiles = true;
        gore.timeLeft = Gore.goreTime * 3;
    }

    // adapted vanilla
    public override bool Update(Gore gore) {
        gore.alpha = gore.position.Y >= Main.worldSurface * 16.0 + 8.0 ? 100 : 0;
        int num1 = 4;
        ++gore.frameCounter;
        if (gore.frame <= 4) {
            int x = (int)(gore.position.X / 16.0);
            int y = (int)(gore.position.Y / 16.0) - 1;
            if (WorldGen.InWorld(x, y) && !Main.tile[x, y].HasTile)
                gore.active = false;
            if (gore.frame == 0 || gore.frame == 1 || gore.frame == 2)
                num1 = 24 + Main.rand.Next(256);
            if (gore.frame == 3)
                num1 = 24 + Main.rand.Next(96);
            if (gore.frameCounter >= num1) {
                gore.frameCounter = 0;
                ++gore.frame;
                if (gore.frame == 5) {
                    int index = Gore.NewGore(null, gore.position, gore.velocity, gore.type);
                    Main.gore[index].frame = 9;
                    Main.gore[index].velocity *= 0.0f;
                }
            }
        }
        else if (gore.frame <= 6) {
            int num2 = 8;
            if (gore.frameCounter >= num2) {
                gore.frameCounter = 0;
                ++gore.frame;
                if (gore.frame == 7)
                    gore.active = false;
            }
        }
        else if (gore.frame <= 9) {
            int num2 = 6;
            gore.velocity.Y += 0.2f;
            if (gore.velocity.Y < 0.5)
                gore.velocity.Y = 0.5f;
            if (gore.velocity.Y > 12.0)
                gore.velocity.Y = 12f;
            if (gore.frameCounter >= num2) {
                gore.frameCounter = 0;
                ++gore.frame;
            }
            if (gore.frame > 9)
                gore.frame = 7;
        }
        else {
            gore.velocity.Y += 0.1f;
            if (gore.frameCounter >= num1) {
                gore.frameCounter = 0;
                ++gore.frame;
            }
            gore.velocity *= 0.0f;
            if (gore.frame > 14)
                gore.active = false;
        }
        Vector2 velocity = gore.velocity;
        gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14);
        if (gore.velocity != velocity) {
            if (gore.frame < 10) {
                gore.frame = 10;
                gore.frameCounter = 0;
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Drip_" + Main.rand.Next(2).ToString()) {
                    Identifier = "Terraria/Drip"
                }, new Vector2?(gore.position + new Vector2(8f, 8f)));
            }
        }
        else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14)) {
            if (gore.frame < 10) {
                gore.frame = 10;
                gore.frameCounter = 0;
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Drip_" + Main.rand.Next(2).ToString()) {
                    Identifier = "Terraria/Drip"
                }, new Vector2?(gore.position + new Vector2(8f, 8f)));
            }
            int x = (int)(gore.position.X + 8.0) / 16;
            int y = (int)(gore.position.Y + 14.0) / 16;
            if (Main.tile[x, y] != null && Main.tile[x, y].LiquidAmount > 0) {
                gore.velocity *= 0.0f;
                gore.position.Y = y * 16 - Main.tile[x, y].LiquidAmount / 16;
            }
        }
        gore.position += gore.velocity;
        return false;
    }
}
