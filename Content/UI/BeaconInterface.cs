namespace RoA.Content.UI;

//sealed class BeaconInterface : UILayer {
//    private static List<(BeaconTE, Vector2)> _activeBeaconUIs = [];

//    public BeaconInterface() : base("BeaconInterface", "Vanilla: Inventory", InterfaceScaleType.Game) { }

//    public override void OnUnload() {
//        _activeBeaconUIs.Clear();
//        _activeBeaconUIs = null;
//    }

//    public static bool HasOpened(BeaconTE beaconTE) => _activeBeaconUIs.Any(x => x.Item1 == beaconTE);

//    public static void ToggleUI(int i, int j, BeaconTE beaconTE) {
//        BeaconInterface beaconInterface = ModContent.GetInstance<BeaconInterface>();
//        if (!_activeBeaconUIs.Contains((beaconTE, new Point(i, j).ToWorldCoordinates()))) {
//            beaconInterface.ActivateSelectively(i, j, beaconTE);
//        }
//        else {
//            bool flag = Main.InSmartCursorHighlightArea(i, j, out bool actuallySelected);
//            if ((flag && (!actuallySelected || !beaconTE.HasGemInIt)) || !flag) {
//                beaconInterface.DeactivateSelectively(i, j, beaconTE);
//            }
//        }
//    }

//    private void ActivateSelectively(int i, int j, BeaconTE beaconTE) {
//        if (!Active) {
//            Activate();
//        }

//        _activeBeaconUIs.Add((beaconTE, new Point(i, j).ToWorldCoordinates()));

//        SoundEngine.PlaySound(SoundID.MenuOpen);
//    }

//    private void DeactivateSelectively(int i, int j, BeaconTE beaconTE) {
//        if (!Active) {
//            return;
//        }

//        _activeBeaconUIs.Remove((beaconTE, new Point(i, j).ToWorldCoordinates()));

//        SoundEngine.PlaySound(SoundID.MenuClose);
//    }

//    public override bool OnUIUpdate(GameTime gameTime) {
//        if (Active) {
//            if (_activeBeaconUIs.Count < 1) {
//                Deactivate();
//                return false;
//            }
//            else {
//                for (int i = 0; i < _activeBeaconUIs.Count; i++) {
//                    (BeaconTE, Vector2) activeBeacon = _activeBeaconUIs[i];
//                    BeaconTE beaconTE = activeBeacon.Item1;
//                    Vector2 beaconPosition = activeBeacon.Item2;
//                    Point beaconTilePosition = beaconPosition.ToTileCoordinates();
//                    int x = beaconTilePosition.X;
//                    int y = beaconTilePosition.Y;
//                    float distance = 85f;
//                    bool flag = !WorldGenHelper.ActiveTile(x, y, ModContent.TileType<Beacon>()) || Vector2.Distance(Main.LocalPlayer.Center, beaconPosition) > distance;
//                    if (flag) {
//                        DeactivateSelectively(x, y, beaconTE);
//                    }
//                }
//            }
//        }

//        return true;
//    }

//    protected override bool DrawSelf() {
//        SpriteBatch spriteBatch = Main.spriteBatch;
//        foreach ((BeaconTE, Vector2) activeBeacon in _activeBeaconUIs) {
//            Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.GUITextures + "Beacon_Icons").Value;
//            SpriteFrame frame = new(8, 1);
//            BeaconTE beaconTE = activeBeacon.Item1;
//            Vector2 beaconPosition = activeBeacon.Item2;
//            byte variant = beaconTE.GetVariant();
//            int gemType = beaconTE.GetItemID(num3: 0);
//            bool hasGemInIt = beaconTE.HasGemInIt;
//            frame = frame.With(hasGemInIt ? (byte)(variant - 1) : variant, 0);
//            Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
//            Vector2 origin = sourceRectangle.Size() / 2f;
//            Vector2 position = beaconPosition + Vector2.UnitX - origin - Vector2.UnitY * sourceRectangle.Height;
//            DrawColor color = DrawColor.White;
//            Vector2 drawPosition = position - Main.screenPosition;
//            spriteBatch.DrawSelf(texture, drawPosition, frame.GetSourceRectangle(texture), color);
//            texture = ModContent.Request<Texture2D>(ResourceManager.GUITextures + "Beacon_Icons_Gems").Value;
//            Player player = Main.LocalPlayer;
//            if (!hasGemInIt) {
//                if (player.noThrow != 2) {
//                    Beacon.UpdateVariants();
//                }
//                frame = frame.With((byte)Beacon.VariantToShow, 0);
//                spriteBatch.DrawSelf(texture, drawPosition, frame.GetSourceRectangle(texture), color.MultiplyRGBA(DrawColor.Black with { A = 100 }));
//            }
//            gemType = beaconTE.GetItemID();
//            Vector2 mousePosition = Main.MousePosition;
//            bool flag;
//            Vector2 adjustedBeaconPosition = beaconPosition;
//            Point tilePosition = beaconPosition.ToTileCoordinates();
//            adjustedBeaconPosition.Y -= 8f;
//            adjustedBeaconPosition.X -= 8f;
//            flag = mousePosition.Between(adjustedBeaconPosition, adjustedBeaconPosition + Vector2.One * 26f);
//            bool flag4 = Main.InSmartCursorHighlightArea(tilePosition.X, tilePosition.Y, out bool actuallySelected);
//            bool flag5 = mousePosition.Between(position, position + sourceRectangle.Size());
//            if (flag5 ||
//                flag || 
//                actuallySelected) {
//                Item item = player.GetSelectedItem();
//                if (!Main.mouseItem.IsEmpty()) {
//                    item = Main.mouseItem;
//                    player.mouseInterface = true;
//                }
//                if (flag5 && hasGemInIt) {
//                    if (Main.mouseRight && Main.mouseRightRelease) {
//                        SoundEngine.PlaySound(SoundID.MenuTick, adjustedBeaconPosition);
//                        beaconTE.DropGem(player, (int)adjustedBeaconPosition.X, (int)adjustedBeaconPosition.Y);
//                        beaconTE.RemoveGem();
//                    }
//                }
//                short gemTypeToInsert = (short)item.type;
//                if (!BeaconTE.Gems.Contains(gemTypeToInsert)) {
//                }
//                else {
//                    if (Main.mouseLeft && Main.mouseLeftRelease && !player.mouseInterface) {
//                        bool flag2 = gemType != item.type;
//                        if (flag2 || !hasGemInIt) {
//                            if (hasGemInIt) {
//                                SoundEngine.PlaySound(SoundID.MenuTick, adjustedBeaconPosition);
//                                beaconTE.DropGem(player, (int)adjustedBeaconPosition.X, (int)adjustedBeaconPosition.Y);
//                            }
//                            if (--item.stack < 0) {
//                                item.TurnToAir();
//                            }
//                            SoundEngine.PlaySound(SoundID.MenuTick, adjustedBeaconPosition);
//                            beaconTE.InsertGem(gemTypeToInsert);
//                        }
//                        else if (!flag2) {
//                            SoundEngine.PlaySound(SoundID.MenuTick, adjustedBeaconPosition);
//                            beaconTE.DropGem(player, (int)adjustedBeaconPosition.X, (int)adjustedBeaconPosition.Y);
//                            beaconTE.RemoveGem();
//                        }
//                    }
//                }
//            }
//        }

//        return base.DrawSelf();
//    }
//}
