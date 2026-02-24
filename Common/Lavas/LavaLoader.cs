//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using Mono.Cecil.Cil;

//using MonoMod.Cil;

//using ReLogic.Content;

//using RoA.Common.Players;
//using RoA.Content.Buffs;
//using RoA.Core;
//using RoA.Core.Utility;

//using System;
//using System.Collections.Generic;
//using System.Linq;

//using Terraria;
//using Terraria.Audio;
//using Terraria.DataStructures;
//using Terraria.GameContent;
//using Terraria.GameContent.Drawing;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Common.Lavas;

//sealed class LavaLoader : IInitializer {
//	private enum WaterTextureType : byte {
//		Block,
//		Slope
//	}

//    private static readonly HashSet<LavaStyle> _allLavaStyles = [];
//	private static readonly Dictionary<LavaStyle, Dictionary<WaterTextureType, Asset<Texture2D>>> _lavaStyleTextures = [];

//    private static LavaStyle? _activeStyle;

//    public static void RegisterLavaStyle(LavaStyle lavaStyle) {
//		_allLavaStyles.Add(lavaStyle);
//		_lavaStyleTextures.TryAdd(lavaStyle, []);
//		Dictionary<WaterTextureType, Asset<Texture2D>> textures = _lavaStyleTextures[lavaStyle];
//        string lavaStyleBlockTexturePath = lavaStyle.Texture + "_Block";
//        textures.Add(WaterTextureType.Block, ModContent.Request<Texture2D>(lavaStyleBlockTexturePath));
//		string lavaStyleSlopeTexturePath = lavaStyle.Texture + "_Slope";
//        textures.Add(WaterTextureType.Slope, ModContent.Request<Texture2D>(lavaStyleSlopeTexturePath));
//    }

//    public static bool TryGetActiveStyle(out LavaStyle? activeLavaStyle) {
//		activeLavaStyle = null;
//		if (_activeStyle != null) {
//			activeLavaStyle = _activeStyle;
//			return true;
//        }

//		return false;
//	}

//    public void Load(Mod mod) {
//		ResetEffectsManager.ResetEffectsEvent += UpdateActiveStyle;

//		//Terraria.GameContent.Liquid.IL_LiquidRenderer.DrawNormalLiquids += DrawSpecialLava;
//		//Terraria.GameContent.Liquid.IL_LiquidRenderer.InternalPrepareDraw += SwapLavaDrawEffects;
//        On_TileDrawing.DrawPartialLiquid += On_TileDrawing_DrawPartialLiquid;

//        On_Collision.LavaCollision += On_Collision_LavaCollision;
//        On_Collision.WetCollision += On_Collision_WetCollision;
//		//IL.Terraria.Main.DrawTiles += DrawSpecialLavaBlock;
//	}
	
//	private class LavaCollisionHandler : ModPlayer {
//        private byte _wetCount;

//        public override void PostUpdate() {
//			UpdateCustomLavaCollision();
//        }

//		private void UpdateCustomLavaCollision() {
//            if (!TryGetActiveStyle(out LavaStyle? activeLavaStyle)) {
//                return;
//            }

//            //Player.wet = Player.lavaWet = false;
//            int num82 = Player.height;
//            if (Player.waterWalk)
//                num82 -= 6;

//            bool inLava = false;
//            if (!Player.shimmering)
//                inLava = CustomLavaCollision(Player.position, Player.width, num82);

//            OnEnterLava(inLava);

//            if (Player.lavaTime > Player.lavaMax)
//                Player.lavaTime = Player.lavaMax;

//            if (Player.waterWalk2 && !Player.waterWalk)
//                num82 -= 6;

//            bool num85 = Collision.WetCollision(Player.position, Player.width, num82);

//            bool flag26 = Collision.honey;
//            bool shimmer = Collision.shimmer;

//            ushort lavaSplashDust = activeLavaStyle!.SplashDustType();
//            if (num85) {
//                OnEnterLiquid(inLava, lavaSplashDust);
//            }
//            else if (Player.wet) {
//                OnExitLiquid(lavaSplashDust);
//            }

//            if (!Player.wet) {
//                Player.lavaWet = false;
//                Player.honeyWet = false;
//                Player.shimmerWet = false;
//            }

//            if (_wetCount > 0)
//                _wetCount--;

//            if (Player.wetSlime > 0)
//                Player.wetSlime--;

//            Player.wetCount = _wetCount;
//        }

//        private void OnEnterLava(bool inLava) {
//            if (inLava) {
//                if (!Player.lavaImmune && Player.IsLocal() && Player.hurtCooldowns[4] <= 0) {
//                    if (Player.lavaTime > 0) {
//                        Player.lavaTime--;
//                    }
//                    else {
//                        int num83 = 80;
//                        int num84 = 420;
//                        if (Main.remixWorld) {
//                            num83 = 200;
//                            num84 = 630;
//                        }

//                        if (!Player.ashWoodBonus || !Player.lavaRose) {
//                            if (Player.ashWoodBonus) {
//                                if (Main.remixWorld)
//                                    num83 = 145;

//                                num83 /= 2;
//                                num84 -= 210;
//                            }

//                            if (Player.lavaRose) {
//                                num83 -= 45;
//                                num84 -= 210;
//                            }

//                            if (num83 > 0)
//                                Player.Hurt(PlayerDeathReason.ByOther(2), num83, 0, pvp: false, quiet: false, 4);

//                            // here we apply collision buff
//                            //if (num84 > 0)
//                            //    Player.AddBuff(24, num84);
//                        }
//                    }
//                }

//                Player.lavaWet = true;
//            }
//            else {
//                Player.lavaWet = false;
//                if (Player.lavaTime < Player.lavaMax)
//                    Player.lavaTime++;
//            }
//        }

//        private void OnEnterLiquid(bool inLava, ushort lavaSplashDustType) {
//            if ((Player.onFire || Player.onFire3) && !Player.lavaWet) {
//                // here we delete our buffs
//                //for (int num88 = 0; num88 < Player.MaxBuffs; num88++) {
//                //    int num89 = Player.buffType[num88];
//                //    if (num89 == 24 || num89 == 323)
//                //        Player.DelBuff(num88);
//                //}
//            }

//            if (!Player.wet) {
//                if (_wetCount == 0) {
//                    _wetCount = 10;
//                    if (!Player.shimmering) {
//                        if (!inLava) {
//                            if (Player.shimmerWet) {
//                            }
//                            else if (Player.honeyWet) {
//                            }
//                            else {
//                                for (int num94 = 0; num94 < 50; num94++) {
//                                    int num95 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, Dust.dustWater());
//                                    Main.dust[num95].velocity.Y -= 3f;
//                                    Main.dust[num95].velocity.X *= 2.5f;
//                                    Main.dust[num95].scale = 0.8f;
//                                    Main.dust[num95].alpha = 100;
//                                    Main.dust[num95].noGravity = true;
//                                }

//                                SoundEngine.PlaySound(SoundID.Splash, Player.position);
//                            }
//                        }
//                        else {
//                            for (int num96 = 0; num96 < 20; num96++) {
//                                int num97 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, lavaSplashDustType);
//                                Main.dust[num97].velocity.Y -= 1.5f;
//                                Main.dust[num97].velocity.X *= 2.5f;
//                                Main.dust[num97].scale = 1.3f;
//                                Main.dust[num97].alpha = 100;
//                                Main.dust[num97].noGravity = true;
//                            }

//                            SoundEngine.PlaySound(SoundID.Splash, Player.position);
//                        }
//                    }
//                }

//                Player.wet = true;
//                if (Player.ShouldFloatInWater) {
//                    Player.velocity.Y /= 2f;
//                    if (Player.velocity.Y > 3f)
//                        Player.velocity.Y = 3f;
//                }
//            }
//        }

//        private void OnExitLiquid(ushort lavaSplashDustType) {
//            Player.wet = false;
//            if (Player.jump > Player.jumpHeight / 5 && Player.wetSlime == 0)
//                Player.jump = Player.jumpHeight / 5;

//            if (_wetCount == 0) {
//                _wetCount = 10;
//                if (!Player.shimmering) {
//                    if (!Player.lavaWet) {
//                        if (Player.shimmerWet) {
//                        }
//                        else if (Player.honeyWet) {
//                        }
//                        else {
//                            for (int num102 = 0; num102 < 50; num102++) {
//                                int num103 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2)), Player.width + 12, 24, Dust.dustWater());
//                                Main.dust[num103].velocity.Y -= 4f;
//                                Main.dust[num103].velocity.X *= 2.5f;
//                                Main.dust[num103].scale = 0.8f;
//                                Main.dust[num103].alpha = 100;
//                                Main.dust[num103].noGravity = true;
//                            }

//                            SoundEngine.PlaySound(SoundID.Splash, Player.position);
//                        }
//                    }
//                    else {
//                        for (int num104 = 0; num104 < 20; num104++) {
//                            int num105 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, lavaSplashDustType);
//                            Main.dust[num105].velocity.Y -= 1.5f;
//                            Main.dust[num105].velocity.X *= 2.5f;
//                            Main.dust[num105].scale = 1.3f;
//                            Main.dust[num105].alpha = 100;
//                            Main.dust[num105].noGravity = true;
//                        }

//                        SoundEngine.PlaySound(SoundID.Splash, Player.position);
//                    }
//                }
//            }
//        }
//    }

//	private static bool CustomLavaCollision(Vector2 Positions, int Width, int Height) {
//		if (!TryGetActiveStyle(out _)) {
//			return false;
//		}

//        int value = (int)(Positions.X / 16f) - 1;
//        int value2 = (int)((Positions.X + (float)Width) / 16f) + 2;
//        int value3 = (int)(Positions.Y / 16f) - 1;
//        int value4 = (int)((Positions.Y + (float)Height) / 16f) + 2;
//        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
//        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
//        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
//        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
//        Vector2 vector = default(Vector2);
//        for (int i = num; i < value2; i++) {
//            for (int j = value3; j < value4; j++) {
//                if (Main.tile[i, j] != null && Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j].LiquidType == LiquidID.Lava) {
//                    vector.X = i * 16;
//                    vector.Y = j * 16;
//                    int num2 = 16;
//                    float num3 = 256 - Main.tile[i, j].LiquidAmount;
//                    num3 /= 32f;
//                    vector.Y += num3 * 2f;
//                    num2 -= (int)(num3 * 2f);
//                    if (Positions.X + (float)Width > vector.X && Positions.X < vector.X + 16f && Positions.Y + (float)Height > vector.Y && Positions.Y < vector.Y + (float)num2)
//                        return true;
//                }
//            }
//        }

//		return false;
//    }

//    private bool On_Collision_WetCollision(On_Collision.orig_WetCollision orig, Vector2 Positions, int Width, int Height) {
//        //if (TryGetActiveStyle(out _)) {
//        //    return false;
//        //}

//        return orig(Positions, Width, Height);
//    }

//    private bool On_Collision_LavaCollision(On_Collision.orig_LavaCollision orig, Vector2 Positions, int Width, int Height) {
//		if (TryGetActiveStyle(out _)) {
//			return false;
//		}

//		return orig(Positions, Width, Height);
//    }

//    private void On_TileDrawing_DrawPartialLiquid(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Microsoft.Xna.Framework.Vector2 position, ref Microsoft.Xna.Framework.Rectangle liquidSize, int liquidType, ref Terraria.Graphics.VertexColors colors) {
//        if (liquidType == LiquidID.Lava && TryGetActiveStyle(out LavaStyle? activeLavaStyle)) {
//			int num = (int)tileCache.Slope;
//            bool flag = !TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType];
//            if (!behindBlocks)
//                flag = false;

//            if (flag || num == 0) {
//                Main.tileBatch.Draw(_lavaStyleTextures[activeLavaStyle][WaterTextureType.Block].Value, position, liquidSize, colors, default(Vector2), 1f, SpriteEffects.None);
//                return;
//            }

//            liquidSize.X += 18 * (num - 1);
//			Texture2D slopeTexture = _lavaStyleTextures[activeLavaStyle][WaterTextureType.Slope].Value;
//            switch (num) {
//                case 1:
//                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
//                    break;
//                case 2:
//                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
//                    break;
//                case 3:
//                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
//                    break;
//                case 4:
//                    Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
//                    break;
//            }

//			return;
//        }

//		orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
//    }

//    private void UpdateActiveStyle() {
//		if (Main.gameMenu) {
//			return;
//		}

//		_activeStyle = _allLavaStyles.FirstOrDefault(n => n.ChooseLavaStyle());
//	}

//	private void DrawSpecialLava(ILContext il) {
//		var c = new ILCursor(il);
//		c.TryGotoNext(n => n.MatchLdloc(8), n => n.MatchLdcI4(2));
//		c.Index += 2;
//		c.Emit(OpCodes.Ldloc, 8);
//		c.EmitDelegate<Func<int, int>>(LavaBody);
//		c.Emit(OpCodes.Stloc, 8);
//		//c.Emit(OpCodes.Ldloc, 8);
//	}

//	private int LavaBody(int arg) {
//		if (TryGetActiveStyle(out LavaStyle? activeLavaStyle)) {
//			return activeLavaStyle!.Slot;
//		}

//		return arg;
//	}

//	private void DrawSpecialLavaBlock(ILContext il) {
//		var c = new ILCursor(il);
//		c.TryGotoNext(n => n.MatchLdsfld(typeof(Main), "liquidTexture"));
//		c.Index += 3;
//		c.Emit(OpCodes.Ldloc, 16);
//		c.Emit(OpCodes.Ldloc, 15);

//		c.Emit(OpCodes.Ldloc, 142);
//		c.Emit(OpCodes.Ldloc, 141);
//		c.Emit(OpCodes.Ldloc, 140);
//		c.Emit(OpCodes.Ldloc, 143);

//		c.EmitDelegate<Func<Texture2D, int, int, Tile, Tile, Tile, Tile, Texture2D>>(LavaBlockBody);
//	}

//	private Texture2D LavaBlockBody(Texture2D arg, int x, int y, Tile up, Tile left, Tile right, Tile down) {
//		if (_activeStyle is null)
//			return arg;

//		if (arg != ModContent.Request<Texture2D>("Terraria/Liquid_1").Value)
//			return arg;

//		string path = _activeStyle.texturePath;
//		_activeStyle.DrawBlockEffects(x, y, up, left, right, down);
//		return _lavaStyleTextures[_activeStyle][WaterTextureType.Block].Value;
//	}

//	private void SwapLavaDrawEffects(ILContext il) {
//		var c = new ILCursor(il);
//		c.TryGotoNext(n => n.MatchLdsfld<Dust>("lavaBubbles"));
//		c.Index += 3;

//		int savedIndex = c.Index;

//		c.TryGotoNext(n => n.MatchLdloc(4)); //I know this looks bad but this is reliable enough. Local 4 is ptr2 in source
//		ILLabel label = il.DefineLabel(c.Next);

//		c.Index = savedIndex;
//		c.Emit(OpCodes.Ldloc, 66); //num25, x coordinate iteration variable
//		c.Emit(OpCodes.Ldloc, 67); //num26, y coordinate iteration variable
//		c.EmitDelegate<Func<int, int, bool>>(SwapLava);
//		c.Emit(OpCodes.Brtrue, label);
//	}

//	private bool SwapLava(int x, int y) {
//		if (TryGetActiveStyle(out LavaStyle? activeLavaStyle)) {
//			return activeLavaStyle!.DrawEffects(x, y);
//		}

//		return false;
//	}
//}