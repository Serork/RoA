﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Crafting;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Content.MapIcons;

sealed class BeaconMapLayer : ModMapLayer {
    public override void Draw(ref MapOverlayDrawContext context, ref string text) {
        List<BeaconTE> beacons = [];
        foreach (TileEntity value in TileEntity.ByPosition.Values) {
            if (value is BeaconTE beaconTE) {
                beacons.Add(beaconTE);
            }
        }
        foreach (BeaconTE beaconTE in beacons) {
            Point16 tilePosition = new(beaconTE.Position.X - 1, beaconTE.Position.Y - 4);
            float num = 1f;
            float scaleIfSelected = num;
            int i = beaconTE.Position.X;
            int j = beaconTE.Position.Y - 1;
            if (Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(Beacon.GetLargeGemItemID(i, j))) {
                /*if (Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(Beacon.GetLargeGemItem(i, j))) */ {
                    scaleIfSelected = num * 2f;
                }
                Texture2D value = ModContent.Request<Texture2D>(ResourceManager.GUITextures + "Beacon_Icons").Value;
                bool num2 = (Main.DroneCameraTracker == null || !Main.DroneCameraTracker.IsInUse());
                Color color = Color.White;
                if (!num2) {
                    color = Color.Gray * 0.5f;
                }
                int frameX = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54 - 1;
                if (frameX < 0) {
                    continue;
                }
                SpriteFrame frame = new(8, 1, (byte)frameX, 0) {
                    PaddingY = 0
                };
                if (context.Draw(value, tilePosition.ToVector2() + new Vector2(1.5f, 2f),
                    color,
                    frame, num, scaleIfSelected, Alignment.Center).IsMouseOver) {
                    Main.cancelWormHole = true;
                    text = /*Language.GetTextValue("Mods.RoA.Map.TeleportTo") + " " + */Beacon.GetMapText(i, j).Value + " " + Language.GetTextValue("Mods.RoA.Map.Beacon");
                    if (Main.mouseLeft && Main.mouseLeftRelease) {
                        Main.mouseLeftRelease = false;
                        Main.mapFullscreen = false;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        SoundEngine.PlaySound(SoundID.MenuClose);
                        PlayerInput.LockGamepadButtons("MouseLeft");
                        Beacon.TeleportPlayerTo(i, j, Main.LocalPlayer);
                    }
                }
            }
        }
    }
}
