using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class BackwoodsFogMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Placeable.BackwoodsFogMusicBox>();

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        ModTile modTile = TileLoader.GetTile(Type);
        if (modTile != null && tile.TileFrameX > 18) {
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) {
                zero = Vector2.Zero;
            }
            int height = tile.TileFrameY == 36 ? 18 : 16;
            int tileX = tile.TileFrameX;
            if (tileX == 36) {
                tileX = 0;
            }
            if (tileX == 54) {
                tileX = 18;
            }
            Color color = Lighting.GetColor(i, j);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                                  new Rectangle(tileX, tile.TileFrameY + Main.tileFrame[modTile.Type] * 36, 16, height),
                                  color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            color = Color.Lerp(color, Color.White, 0.5f);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value,
                      new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                      new Rectangle(tileX, tile.TileFrameY + Main.tileFrame[modTile.Type] * 36, 16, height),
                      color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            bool flag = tile.TileFrameX == 36;
            if (flag) {
                Vector2 position = new Vector2(i * 16 + 16, j * 16 - 4);
                float value = 0.2f;
                int frame = Main.tileFrame[modTile.Type];
                switch (frame) {
                    case 1 or 5:
                        value = 0.4f;
                        break;
                    case 2 or 4:
                        value = 0.6f;
                        break;
                    case 3:
                        value = 0.8f;
                        break;
                }
                Lighting.AddLight(position,
                    (Color.Green * value).ToVector3() * 0.75f);

                int type = ModContent.DustType<NaturesHeartDust>();
                if (!WorldGenHelper.GetTileSafely(i, j + 1).ActiveTile(Type)) {
                    if (Main.rand.NextBool(10)) {
                        for (int num740 = 0; num740 < 1; num740++) {
                            if (Main.rand.NextChance(value / 2f)) {
                                int num741 = Dust.NewDust(new Vector2(position.X + 2, position.Y + 8), 6, 4, type);
                                Dust dust2 = Main.dust[num741];
                                dust2.velocity *= 0.5f;
                                dust2.scale *= 0.9f;
                                Main.dust[num741].noGravity = true;
                            }
                        }
                    }
                    if (Main.rand.NextBool(10)) {
                        for (int num742 = 0; num742 < 1; num742++) {
                            if (Main.rand.NextChance(value / 2f)) {
                                int num743 = Dust.NewDust(new Vector2(position.X + 2, position.Y + 8), 6, 4, type);
                                Dust dust2 = Main.dust[num743];
                                dust2.velocity *= 2.5f;
                                dust2.velocity *= 0.5f;
                                dust2 = Main.dust[num743];
                                dust2.scale *= 0.7f;
                                Main.dust[num743].noGravity = true;
                            }
                        }
                    }
                }

                if (!(Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4))) {
                    if (tile.TileFrameY % 36 == 0 && (int)Main.timeForVisualEffects % 7 == 0 && Main.rand.NextBool(3)) {
                        int goreType = Main.rand.Next(570, 573);
                        Vector2 velocity = new Vector2(Main.WindForVisuals * 2f, -0.5f);
                        velocity.X *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
                        velocity.Y *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
                        if (goreType == 572) {
                            position.X -= 8f;
                        }

                        if (goreType == 571) {
                            position.X -= 4f;
                        }

                        if (!Main.dedServ) {
                            Gore.NewGore(new EntitySource_TileUpdate(i, j), position, velocity, goreType, 0.8f);
                        }
                    }
                }
            }

            return false;
        }

        return true;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        frameCounter++;
        if (frameCounter > 10) {
            frameCounter = 0;

            frame++;
            if (frame > 5) {
                frame = 0;
            }
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {

    }
}