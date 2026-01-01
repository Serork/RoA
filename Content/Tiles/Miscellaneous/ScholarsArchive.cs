using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Content.Items.Placeable.Miscellaneous;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ScholarsArchive : ModTile, TileHooks.IPreDraw {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ScholarsArchiveTE>().Hook_AfterPlacement, -1, 0, true);
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        TileObjectData.addTile(Type);
        AdjTiles = [TileID.Bookcases];
        AddMapEntry(new Color(213, 189, 185), CreateMapEntryName());

        Main.tileSpelunker[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 2000;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        Main.tileOreFinderPriority[Type] = 550;

        TileID.Sets.InteractibleByNPCs[Type] = true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        ScholarsArchiveTE? scholarsArchiveTE = TileHelper.GetTE<ScholarsArchiveTE>(i, j);
        if (scholarsArchiveTE is not null) {
            void dropSpellTome(ScholarsArchiveTE.ArchiveSpellTomeType spellTome) {
                int toDrop = -1;
                Vector2 offset = Vector2.Zero;
                switch (spellTome) {
                    case ScholarsArchiveTE.ArchiveSpellTomeType.Bookworms:
                        offset = new(10, 20);
                        toDrop = ModContent.ItemType<Bookworms>();
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.Bane:
                        offset = new(16, 22);
                        toDrop = ModContent.ItemType<Bane>();
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.BookofSkulls:
                        offset = new(22, 24);
                        toDrop = ItemID.BookofSkulls;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.WaterBolt:
                        offset = new(26, 24);
                        toDrop = ItemID.WaterBolt;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.DemonScythe:
                        offset = new(32, 22);
                        toDrop = ItemID.DemonScythe;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.CrystalStorm:
                        offset = new(36, 20);
                        toDrop = ItemID.CrystalStorm;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.CursedFlames:
                        offset = new(10, 40);
                        toDrop = ItemID.CursedFlames;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.GoldenShower:
                        offset = new(14, 42);
                        toDrop = ItemID.GoldenShower;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.MagnetSphere:
                        offset = new(20, 44);
                        toDrop = ItemID.MagnetSphere;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.RazorbladeTyphoon:
                        offset = new(24, 44);
                        toDrop = ItemID.RazorbladeTyphoon;
                        break;
                    case ScholarsArchiveTE.ArchiveSpellTomeType.LunarFlareBook:
                        offset = new(30, 42);
                        toDrop = ItemID.LunarFlareBook;
                        break;
                }
                if (toDrop == -1) {
                    return;
                }
                Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16 + offset, toDrop);
            }
            ScholarsArchiveTE.ArchiveSpellTomeType[] spellTomes = [
                ScholarsArchiveTE.ArchiveSpellTomeType.Bookworms,
                ScholarsArchiveTE.ArchiveSpellTomeType.Bane,
                ScholarsArchiveTE.ArchiveSpellTomeType.BookofSkulls,
                ScholarsArchiveTE.ArchiveSpellTomeType.WaterBolt,
                ScholarsArchiveTE.ArchiveSpellTomeType.DemonScythe,
                ScholarsArchiveTE.ArchiveSpellTomeType.CrystalStorm,
                ScholarsArchiveTE.ArchiveSpellTomeType.CursedFlames,
                ScholarsArchiveTE.ArchiveSpellTomeType.GoldenShower,
                ScholarsArchiveTE.ArchiveSpellTomeType.MagnetSphere,
                ScholarsArchiveTE.ArchiveSpellTomeType.RazorbladeTyphoon,
                ScholarsArchiveTE.ArchiveSpellTomeType.LunarFlareBook
            ];
            for (int i2 = 0; i2 < spellTomes.Length; i2++) {
                if (scholarsArchiveTE.HasSpellTome(spellTomes[i2])) {
                    dropSpellTome(spellTomes[i2]);
                }
            }
        }

        ModContent.GetInstance<ScholarsArchiveTE>().Kill(i, j);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Miscellaneous.ScholarsArchive>());
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {

    }

    public override void MouseOver(int i, int j) {
        if (!HasSpellTomeInHand(out int selectedItemType, out Item selectedItem)) {
            return;
        }

        Player player = Main.LocalPlayer;
        player.cursorItemIconID = selectedItemType;
        if (player.cursorItemIconID != -1) {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }
    }

    private bool HasSpellTomeInHand(out int selectedItemType, out Item selectedItem) {
        Player player = Main.LocalPlayer;
        if (!player.ItemTimeIsZero) {
            selectedItemType = -1;
            selectedItem = null!;

            return false;
        }

        int[] spellTomesItemTypes = [
            ModContent.ItemType<Bookworms>(),
            ModContent.ItemType<Bane>(),
            ItemID.BookofSkulls,
            ItemID.WaterBolt,
            ItemID.DemonScythe,
            ItemID.CrystalStorm,
            ItemID.CursedFlames,
            ItemID.GoldenShower,
            ItemID.MagnetSphere,
            ItemID.RazorbladeTyphoon,
            ItemID.LunarFlareBook
        ];

        selectedItem = player.GetSelectedItem();
        if (!Main.mouseItem.IsEmpty()) {
            selectedItem = Main.mouseItem;
        }
        selectedItemType = selectedItem.type;
        return spellTomesItemTypes.Contains(selectedItemType);
    }

    public override bool RightClick(int i, int j) {
        Point16 tePosition = TileHelper.GetTileTopLeft2<ScholarsArchive>(i, j);
        ScholarsArchiveTE? scholarsArchiveTE = TileHelper.GetTE<ScholarsArchiveTE>(tePosition.X, tePosition.Y);
        if (scholarsArchiveTE is null) {
            return false;
        }

        if (HasSpellTomeInHand(out int selectedItemType, out Item selectedItem)) {
            Player player = Main.LocalPlayer;
            player.releaseUseItem = false;
            player.mouseInterface = true;
            player.PlayDroppedItemAnimation(20);

            if (selectedItemType == ModContent.ItemType<Bookworms>()) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.Bookworms;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ModContent.ItemType<Bane>()) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.Bane;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.BookofSkulls) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.BookofSkulls;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.WaterBolt) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.WaterBolt;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.DemonScythe) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.DemonScythe;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.CrystalStorm) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.CrystalStorm;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.CursedFlames) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.CursedFlames;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.GoldenShower) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.GoldenShower;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.MagnetSphere) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.MagnetSphere;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.RazorbladeTyphoon) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.RazorbladeTyphoon;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }
            else if (selectedItemType == ItemID.LunarFlareBook) {
                ScholarsArchiveTE.ArchiveSpellTomeType toInsert = ScholarsArchiveTE.ArchiveSpellTomeType.LunarFlareBook;
                if (scholarsArchiveTE.HasSpellTome(toInsert)) {
                    return false;
                }
                scholarsArchiveTE.InsertSpellTome(toInsert);
            }

            if (--selectedItem.stack <= 0) {
                selectedItem.TurnToAir();
            }
        }

        return true;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        TileHelper.AddPostNonSolidTileDrawPoint(this, i, j);

        return base.PreDraw(i, j, spriteBatch);
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        Point16 tePosition = TileHelper.GetTileTopLeft2<ScholarsArchive>(i, j);
        ScholarsArchiveTE? scholarsArchiveTE = TileHelper.GetTE<ScholarsArchiveTE>(tePosition.X, tePosition.Y);
        if (scholarsArchiveTE is null) {
            return;
        }

        void drawSpellTome(int frame) {
            Tile tile = Main.tile[i, j];
            Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
            texture ??= TextureAssets.Tile[Type].Value;
            Color color = Color.White;
            Rectangle clip = new(tile.TileFrameX, tile.TileFrameY + 72 * frame, 16, 16);
            spriteBatch.Draw(texture,
                             new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + TileHelper.ScreenOffset,
                             clip,
                             color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        ScholarsArchiveTE.ArchiveSpellTomeType[] spellTomes = [
            ScholarsArchiveTE.ArchiveSpellTomeType.Bookworms,
            ScholarsArchiveTE.ArchiveSpellTomeType.Bane,
            ScholarsArchiveTE.ArchiveSpellTomeType.BookofSkulls,
            ScholarsArchiveTE.ArchiveSpellTomeType.WaterBolt,
            ScholarsArchiveTE.ArchiveSpellTomeType.DemonScythe,
            ScholarsArchiveTE.ArchiveSpellTomeType.CrystalStorm,
            ScholarsArchiveTE.ArchiveSpellTomeType.CursedFlames,
            ScholarsArchiveTE.ArchiveSpellTomeType.GoldenShower,
            ScholarsArchiveTE.ArchiveSpellTomeType.MagnetSphere,
            ScholarsArchiveTE.ArchiveSpellTomeType.RazorbladeTyphoon,
            ScholarsArchiveTE.ArchiveSpellTomeType.LunarFlareBook
        ];
        for (int i2 = 0; i2 < spellTomes.Length; i2++) {
            if (scholarsArchiveTE.HasSpellTome(spellTomes[i2])) {
                drawSpellTome(i2 + 1);
            }
        }
    }

    void TileHooks.IPreDraw.PreDrawExtra(SpriteBatch spriteBatch, Point16 tilePosition) {
        int i = tilePosition.X,
            j = tilePosition.Y;
        Texture2D glowTexture = _glowTexture.Value;
        bool flag = false;
        int type = ModContent.TileType<ScholarsArchive>();
        if (Main.tile[i, j].TileType == type &&
            Main.tile[i + 1, j].TileType == type &&
            Main.tile[i - 1, j].TileType == type &&
            Main.tile[i, j - 1].TileType != type) {
            flag = true;
        }
        if (flag) {
            float waveFrequency = 10f;
            float offset = i * j;

            byte a = 100;
            Rectangle clip = Utils.Frame(glowTexture, 1, 3, frameY: 1);
            Vector2 origin = clip.Centered();
            SpriteBatch batch = Main.spriteBatch;
            Vector2 position = new Point16(i, j).ToWorldCoordinates() - Vector2.UnitY * 6f;         
            float rotation = (float)(Main.timeForVisualEffects * 0.1 + offset) * 0.05f;
            Color lightingColor = Lighting.GetColor(position.ToTileCoordinates());
            lightingColor = Color.Lerp(lightingColor, Color.White, 0.1f);
            Color color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset)).ModifyRGB(Helper.Wave(0.9f, 1.1f, waveFrequency, offset));
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor) with { A = a },
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);

            color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset - 2f)).ModifyRGB(Helper.Wave(0.8f, 1.1f, waveFrequency, offset + 2f));
            clip = Utils.Frame(glowTexture, 1, 3, frameY: 0);
            origin = clip.Centered();
            rotation = (float)(Main.timeForVisualEffects * 0.5 + offset) * 0.05f;
            drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor) with { A = a },
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);

            color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset - 4f)).ModifyRGB(Helper.Wave(1f, 1.1f, waveFrequency, offset + 4f));
            clip = Utils.Frame(glowTexture, 1, 3, frameY: 2);
            origin = clip.Centered();
            rotation = (float)(-Main.timeForVisualEffects * 1 + offset * 2) * 0.05f;
            drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor) with { A = a },
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);
        }
    }
}
