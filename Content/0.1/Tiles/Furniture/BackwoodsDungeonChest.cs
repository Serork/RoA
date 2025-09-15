using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class BackwoodsDungeonChest : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        //Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        AdjTiles = [TileID.Containers];

        Color mapColor = new(124, 127, 140);
        AddMapEntry(mapColor, CreateMapEntryName(), MapChestName);
        AddMapEntry(mapColor, Language.GetText("Mods.RoA.Tiles.BackwoodsDungeonChest2.MapEntry"));

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        HitSound = SoundID.Dig;

        //RegisterItemDrop(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest>(), 1);
        //RegisterItemDrop(ItemID.Chest);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorInvalidTiles = [
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            ];
        TileObjectData.addTile(Type);
    }

    public static string MapChestName(string name, int i, int j) {
        int left = i;
        int top = j;
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0) {
            left--;
        }

        if (tile.TileFrameY != 0) {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        if (chest < 0) {
            return Language.GetTextValue("LegacyChestType.0");
        }

        if (Main.chest[chest].name == string.Empty) {
            return name;
        }

        return name + ": " + Main.chest[chest].name;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override ushort GetMapOption(int i, int j) => this.IsLockedChest(i, j) ? (ushort)0 : (ushort)1;

    public override bool IsLockedChest(int i, int j) => (int)Main.tile[i, j].TileFrameX / 36 == 0;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.BackwoodsDungeonChest>());
    }

    public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) {
        bool flag = Main.tile[i, j].TileFrameX == 0;
        if (!flag && Main.tile[i, j].TileFrameX <= 36 && NPC.downedPlantBoss) {
            frameXAdjustment = -36;
            return true;
        }
        return false;
    }

    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
        dustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();

        if (!NPC.downedPlantBoss) {
            return false;
        }

        frameXAdjustment = 36;

        AchievementsHelper.NotifyProgressionEvent(20);
        return true;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    public override bool RightClick(int i, int j) {
        Terraria.Player localPlayer = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        int num1 = i;
        int num2 = j;
        if ((int)tile.TileFrameX % 36 != 0)
            --num1;
        if (tile.TileFrameY != (short)0)
            --num2;
        if (localPlayer.sign >= 0) {
            SoundEngine.PlaySound(in SoundID.MenuClose);
            localPlayer.sign = -1;
            Main.editSign = false;
            Main.npcChatText = "";
        }
        if (Main.editChest) {
            SoundEngine.PlaySound(in SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = "";
        }
        if (localPlayer.editedChestName) {
            NetMessage.SendData(33, text: NetworkText.FromLiteral(Main.chest[localPlayer.chest].name), number: localPlayer.chest, number2: 1f);
            localPlayer.editedChestName = false;
        }
        bool flag = this.IsLockedChest(num1, num2);
        if (Main.netMode == 1 && !flag) {
            if (num1 == localPlayer.chestX && num2 == localPlayer.chestY && localPlayer.chest >= 0) {
                localPlayer.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(in SoundID.MenuClose);
            }
            else {
                NetMessage.SendData(31, number: num1, number2: (float)num2);
                Main.stackSplit = 600;
            }
        }
        else if (flag) {
            int num3 = ModContent.ItemType<BackwoodsDungeonKey>();
            for (int index = 0; index < 58; ++index) {
                if (localPlayer.inventory[index].type == num3 && localPlayer.inventory[index].stack > 0 && Chest.Unlock(num1, num2)) {
                    --localPlayer.inventory[index].stack;
                    if (localPlayer.inventory[index].stack <= 0)
                        localPlayer.inventory[index].TurnToAir();
                    if (Main.netMode == 1) {
                        NetMessage.SendData(52, number: localPlayer.whoAmI, number2: 1f, number3: (float)num1, number4: (float)num2);
                        break;
                    }
                    break;
                }
            }
        }
        else {
            int chest = Chest.FindChest(num1, num2);
            if (chest >= 0) {
                Main.stackSplit = 600;
                if (chest == localPlayer.chest) {
                    localPlayer.chest = -1;
                    SoundEngine.PlaySound(in SoundID.MenuClose);
                }
                else {
                    localPlayer.chest = chest;
                    Main.playerInventory = true;
                    if (PlayerInput.GrappleAndInteractAreShared)
                        PlayerInput.Triggers.JustPressed.Grapple = false;
                    Main.recBigList = false;
                    localPlayer.chestX = num1;
                    localPlayer.chestY = num2;
                    SoundStyle style = localPlayer.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick;
                    SoundEngine.PlaySound(in style);
                }
                Recipe.FindRecipes();
            }
        }
        return true;
    }

    public override LocalizedText DefaultContainerName(int frameX, int frameY) {
        int option = frameX / 36 + 1;
        return Language.GetText("Mods.RoA.Tiles.BackwoodsDungeonChest2.MapEntry");
    }

    public override void MouseOver(int i, int j) {
        Terraria.Player localPlayer = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int num1 = i;
        int num2 = j;
        if ((int)tile.TileFrameX % 36 != 0)
            --num1;
        if (tile.TileFrameY != (short)0)
            --num2;
        int chest = Chest.FindChest(num1, num2);
        localPlayer.cursorItemIconID = -1;
        if (chest < 0) {
            localPlayer.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else {
            bool isLocked = IsLockedChest(num1, num2);
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
            localPlayer.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
            if (localPlayer.cursorItemIconText == defaultName) {
                localPlayer.cursorItemIconID = !isLocked ? (ushort)ModContent.ItemType<Items.Placeable.Furniture.BackwoodsDungeonChest>() : ModContent.ItemType<BackwoodsDungeonKey>();

                localPlayer.cursorItemIconText = "";
            }
        }
        localPlayer.noThrow = 2;
        localPlayer.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j) {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        if (player.cursorItemIconText == string.Empty) {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = 0;
        }
    }
}