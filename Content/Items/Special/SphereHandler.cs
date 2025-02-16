using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace RoA.Content.Items.Special;

sealed class SphereHandler : GlobalItem {
    private const float DISTTOALTAR = 150f;

    private static List<int> _spheresToHandle = [ModContent.ItemType<SphereOfPyre>(), ModContent.ItemType<SphereOfCondor>(), ModContent.ItemType<SphereOfQuake>(),
                                                 ModContent.ItemType<SphereOfAspiration>(), ModContent.ItemType<SphereOfStream>(), ModContent.ItemType<SphereOfShock>()];

    public override bool InstancePerEntity => true;

    private int _breathCD, _breathCD2;
    private int _breath = 200, _breathMax = 200;
    private int _breath2 = 200, _breathMax2 = 200;
    private int _flyTime, _flyTimeMax = 200;
    private int _terraTime, _terraTimeMax = 200;
    private int _cdToTransformation;

    private int _breathCDMax {
        get {
            int num = 7;
            return num;
        }
    }

    public override void Load() {
        On_Item.Prefix += On_Item_Prefix;
        On_ChatManager.DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float += On_ChatManager_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float;
        On_Main.DrawInterface_Resources_Breath += On_Main_DrawInterface_Resources_Breath;
    }

    private void On_Main_DrawInterface_Resources_Breath(On_Main.orig_DrawInterface_Resources_Breath orig) {
        orig();

        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.BeginWorld();
        List<Item> items = Main.item.Where(x => x.active && _spheresToHandle.Contains(x.type)).ToList();
        foreach (Item item in items) {
            DrawStream(item, Main.spriteBatch);
            DrawPyre(item, Main.spriteBatch);
            DrawTerra(item, Main.spriteBatch);
            DrawCondor(item, Main.spriteBatch);
        }
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
    }

    public override void Unload() {
        _spheresToHandle.Clear();
        _spheresToHandle = null;

        _lerpColors.Clear();
        _lerpColors = null;
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {
        if (_cdToTransformation > 0) {
            if (item.beingGrabbed || item.shimmered) {
                _cdToTransformation = 0;
            }
            _cdToTransformation--;
            _cdToTransformation = Math.Max(0, _cdToTransformation);
            return;
        }

        if (item.beingGrabbed) {
            _terraTime = _breath = _breath2 = _flyTime = 0;
        }

        UpdateStream(item);
        UpdatePyre(item);
        if (UpdateCondor(item)) {
            float value = 1f - (float)_flyTime / _flyTimeMax;
            gravity *= value;
            maxFallSpeed *= value;
        }
        if (UpdateTerra(item)) {
            Vector2 position = AltarHandler.GetAltarPosition().ToWorldCoordinates();
            position += Helper.VelocityToPoint(position, item.Center, 1f);
            float value2 = position.Distance(item.Center) / DISTTOALTAR;
            float value = 1f - (float)_terraTime / _terraTimeMax;
            value *= value2;
            gravity *= value;
            maxFallSpeed *= value;
        }
    }

    #region TERRA
    private bool UpdateTerra(Item item) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return false;
        }
        bool quakeSphere = item.type == ModContent.ItemType<SphereOfQuake>();
        bool validToHandle = sphere && !quakeSphere;
        if (!validToHandle) {
            return false;
        }
        Vector2 altarPosition = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        if (altarPosition.Distance(item.Center) < DISTTOALTAR && Collision.CanHit(item.position, item.width, item.height,
            altarPosition, 2, 2)) {
            item.velocity.X *= 0.95f;
            item.velocity.Y *= 0.95f;

            if (item.beingGrabbed) {
                _terraTime = 0;
            }

            _terraTime += 1;
            if (_terraTime >= _terraTimeMax) {
                _terraTime = 0;
                item.ChangeItemType(ModContent.ItemType<SphereOfQuake>());
                _cdToTransformation = 100;
                for (int i = 0; i < 20; i++) {
                    int num = Dust.NewDust(item.Center, 1, 1, 309);
                    Main.dust[num].scale *= 1.5f;
                    Main.dust[num].color = new(30, 177, 77);
                }
            }
            return true;
        }
        _terraTime = 0;
        return false;
    }

    private static void DrawTerra(Item item, SpriteBatch spriteBatch) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool quakeSphere = item.type == ModContent.ItemType<SphereOfQuake>();
        bool validToHandle = sphere && !quakeSphere;
        if (!validToHandle) {
            return;
        }

        var handler = item.GetGlobalItem<SphereHandler>();
        int _terraTime = handler._terraTime;
        int _terraMax = handler._terraTimeMax;
        if (_terraTime == 0) {
            return;
        }
        _terraTime = _terraMax - _terraTime;
        if (_terraTime < _terraMax) {
            int num = 50;
            for (int i = 1; i < _terraMax / num + 1; i++) {
                int num2 = 255;
                float num3 = 1f;
                if (_terraTime >= i * num) {
                    num2 = 255;
                }
                else {
                    float num4 = (float)(_terraTime - (i - 1) * num) / (float)num;
                    num2 = (int)(30f + 225f * num4);
                    if (num2 < 30)
                        num2 = 30;

                    num3 = num4 / 4f + 0.75f;
                    if ((double)num3 < 0.75)
                        num3 = 0.75f;
                }

                int num5 = 0;
                int num6 = 0;
                if (i > 10) {
                    num5 -= 260;
                    num6 += 26;
                }
                Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.GUITextures + "TerraLeaf").Value;
                int height = texture.Height;
                int width = texture.Width;
                spriteBatch.Draw(texture, item.Center - Main.screenPosition + new Vector2((float)(26 * (i - 1) + num5) - 48,
                    -50f + ((float)height - (float)height * num3) / 2f + (float)num6), new Microsoft.Xna.Framework.Rectangle(0, 0, width, height), new Microsoft.Xna.Framework.Color(num2, num2, num2, num2), 0f, default(Vector2), num3, SpriteEffects.None, 0f);
            }
        }
    }
    #endregion

    #region CONDOR
    private bool UpdateCondor(Item item) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return false;
        }
        bool condorStream = item.type == ModContent.ItemType<SphereOfCondor>();
        bool validToHandle = sphere && !condorStream;
        if (!validToHandle) {
            return false;
        }
        if (item.wet || item.lavaWet || _terraTime > 0) {
            _flyTime = 0;
            return false;
        }

        if ((item.velocity.Y < 0.1f && _flyTime < _flyTimeMax * 0.8f) || item.beingGrabbed) {
            _flyTime = 0;
        }
        if (item.velocity.Y < 4f && _flyTime < 50) {
            _flyTime = 0;
        }
        else {
            _flyTime += 2;
            if (_flyTime >= _flyTimeMax) {
                _flyTime = 0;
                item.ChangeItemType(ModContent.ItemType<SphereOfCondor>());
                _cdToTransformation = 100;
                for (int i = 0; i < 20; i++) {
                    int num = Dust.NewDust(item.Center, 1, 1, 309);
                    Main.dust[num].scale *= 1.5f;
                    Main.dust[num].color = new(59, 183, 208);
                }
            }

            return true;
        }

        _flyTime = 0;
        return false;
    }

    private static void DrawCondor(Item item, SpriteBatch spriteBatch) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool condorStream = item.type == ModContent.ItemType<SphereOfCondor>();
        bool validToHandle = sphere && !condorStream;
        if (!validToHandle) {
            return;
        }

        var handler = item.GetGlobalItem<SphereHandler>();
        int _flyTime = handler._flyTime;
        int _flyTimeMax = handler._flyTimeMax;
        if (_flyTime == 0) {
            return;
        }
        _flyTime = _flyTimeMax - _flyTime;
        if (_flyTime < _flyTimeMax) {
            int num = 50;
            for (int i = 1; i < _flyTimeMax / num + 1; i++) {
                int num2 = 255;
                float num3 = 1f;
                if (_flyTime >= i * num) {
                    num2 = 255;
                }
                else {
                    float num4 = (float)(_flyTime - (i - 1) * num) / (float)num;
                    num2 = (int)(30f + 225f * num4);
                    if (num2 < 30)
                        num2 = 30;

                    num3 = num4 / 4f + 0.75f;
                    if ((double)num3 < 0.75)
                        num3 = 0.75f;
                }

                int num5 = 0;
                int num6 = 0;
                if (i > 10) {
                    num5 -= 260;
                    num6 += 26;
                }
                Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.GUITextures + "CondorFeather").Value;
                int height = texture.Height;
                int width = texture.Width;
                spriteBatch.Draw(texture, item.Center - Main.screenPosition + 
                    new Vector2((float)(26 * (i - 1) + num5) - 45f,
                    -50f + ((float)height - (float)height * num3) / 2f + (float)num6), new Microsoft.Xna.Framework.Rectangle(0, 0, width, height), new Microsoft.Xna.Framework.Color(num2, num2, num2, num2), 0f, default(Vector2), 
                    num3, SpriteEffects.None, 0f);
            }
        }
    }
    #endregion

    #region SHOCK
    private static List<Color> _lerpColors = [new Color(175, 241, 205), new Color(60, 222, 190)];

    private sealed class LerpColorHandler : ModPlayer {
        private float _lerpColorProgress;
        private Color _lerpColor;

        public Color GetLerpColor(List<Color> from) {
            _lerpColorProgress += 0.01f;
            int colorCount = from.Count;
            for (int i = 0; i < colorCount; i++) {
                float part = 1f / colorCount;
                float min = part * i;
                float max = part * (i + 1);
                if (_lerpColorProgress >= min && _lerpColorProgress <= max) {
                    _lerpColor = Color.Lerp(from[i], from[i == colorCount - 1 ? 0 : (i + 1)], Utils.Remap(_lerpColorProgress, min, max, 0f, 1f, true));
                }
            }
            if (_lerpColorProgress > 1f) {
                _lerpColorProgress = 0f;
            }
            return _lerpColor;
        }
    }

    private Vector2 On_ChatManager_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float(On_ChatManager.orig_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float orig, SpriteBatch spriteBatch, ReLogic.Graphics.DynamicSpriteFont font, string text, Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, float maxWidth, float spread) {
        if (_spheresToHandle.Contains(Main.reforgeItem.type)) {
            if (text.Contains(Colors.AlphaDarken(Colors.CoinCopper).Hex3()) &&
                text.Contains(Lang.inter[18].Value)) {
                text = "[c/" + Main.LocalPlayer.GetModPlayer<LerpColorHandler>().GetLerpColor(_lerpColors).Hex3() + ":" + "???" + "] ";
            }
        }

        return orig(spriteBatch, font, text, position, baseColor, rotation, origin, baseScale, maxWidth, spread);
    }

    private bool On_Item_Prefix(On_Item.orig_Prefix orig, Item self, int prefixWeWant) {
        if (_spheresToHandle.Contains(self.type) && self.type != ModContent.ItemType<SphereOfShock>()) {
            return true;
        }

        return orig(self, prefixWeWant);
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        if (_spheresToHandle.Contains(item.type)) {
            return false;
        }

        return base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
    }

    public override void PostReforge(Item reforgeItem) {
        if (_spheresToHandle.Contains(reforgeItem.type)) {
            Main.reforgeItem.ChangeItemType(ModContent.ItemType<SphereOfShock>());
            Player player = Main.LocalPlayer;
            Item item = player.GetItem(Main.myPlayer, Main.reforgeItem, GetItemSettings.GetItemInDropItemCheck);
            if (!item.IsEmpty()) {
                Main.reforgeItem.position = player.Center;
                int whoAmI = Item.NewItem(player.GetSource_Misc("reforgeitemdrop"), (int)player.position.X, (int)player.position.Y, player.width, player.height,
                    Main.reforgeItem.type, Main.reforgeItem.stack,
                    noBroadcast: false, Main.reforgeItem.prefix, noGrabDelay: true);
                Main.item[whoAmI].newAndShiny = false;
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, whoAmI, 1f);
                }
                Main.reforgeItem = new Item();
                return;
            }
            for (int i = 0; i < 20; i++) {
                int num = Dust.NewDust(player.Center, 1, 1, 309);
                Main.dust[num].scale *= 1.5f;
                Main.dust[num].color = new(60, 222, 190);
            }
            PopupText.NewText(PopupTextContext.ItemReforge, reforgeItem, reforgeItem.stack, noStack: true);
            Main.reforgeItem = item;
            SoundEngine.PlaySound(SoundID.Grab);
        }
    }
    #endregion

    #region STREAM
    private void UpdateStream(Item item) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool streamSphere = item.type == ModContent.ItemType<SphereOfStream>();
        bool validToHandle = sphere && !streamSphere;
        if (item.wet && !item.lavaWet && !item.shimmerWet) {
            if (streamSphere) {
                if (item.velocity.Y > 0.86f) {
                    item.velocity.Y *= 0.9f;
                }
                item.velocity.Y = item.velocity.Y - 0.6f;
                if (item.velocity.Y < -2f) {
                    item.velocity.Y = -2f;
                }
            }
            if (validToHandle) {
                item.velocity *= 0.9f;
                if (item.velocity.Length() < 1f) {
                    item.velocity *= 0.95f;

                    if (item.beingGrabbed) {
                        _breathCD = _breath = 0;
                    }

                    _breathCD++;
                    if (_breathCD >= _breathCDMax) {
                        _breathCD = 0;
                        _breath -= 5;
                        if (_breath <= 0) {
                            _breath = 0;

                            item.ChangeItemType(ModContent.ItemType<SphereOfStream>());
                            _cdToTransformation = 100;
                            for (int i = 0; i < 20; i++) {
                                int num = Dust.NewDust(item.Center, 1, 1, 309);
                                Main.dust[num].scale *= 1.5f;
                                Main.dust[num].color = new(57, 136, 232);
                            }
                        }
                    }
                }
            }
        }
        else if (!streamSphere) {
            if (_breath != _breathMax) {
                _breath = _breathMax;
            }

            _breathCD = 0;
        }
    }

    private static void DrawStream(Item item, SpriteBatch spriteBatch) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool streamSphere = item.type == ModContent.ItemType<SphereOfStream>();
        bool validToHandle = sphere && !streamSphere;
        if (!validToHandle) {
            return;
        }

        var handler = item.GetGlobalItem<SphereHandler>();
        int _breath = handler._breath;
        int _breathMax = handler._breathMax;
        if (_breath < _breathMax) {
            int num = 50;
            for (int i = 1; i < _breathMax / num + 1; i++) {
                int num2 = 255;
                float num3 = 1f;
                if (_breath >= i * num) {
                    num2 = 255;
                }
                else {
                    float num4 = (float)(_breath - (i - 1) * num) / (float)num;
                    num2 = (int)(30f + 225f * num4);
                    if (num2 < 30)
                        num2 = 30;

                    num3 = num4 / 4f + 0.75f;
                    if ((double)num3 < 0.75)
                        num3 = 0.75f;
                }

                int num5 = 0;
                int num6 = 0;
                if (i > 10) {
                    num5 -= 260;
                    num6 += 26;
                }
                spriteBatch.Draw(TextureAssets.Bubble.Value, item.Center - Main.screenPosition + new Vector2((float)(26 * (i - 1) + num5) - 50f,
                    -50f + ((float)TextureAssets.Bubble.Height() - (float)TextureAssets.Bubble.Height() * num3) / 2f + (float)num6), new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Bubble.Width(), TextureAssets.Bubble.Height()), new Microsoft.Xna.Framework.Color(num2, num2, num2, num2), 0f, default(Vector2), num3, SpriteEffects.None, 0f);
            }
        }
    }
    #endregion

    #region PYRE
    private void UpdatePyre(Item item) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool pyreSphere = item.type == ModContent.ItemType<SphereOfPyre>();
        bool validToHandle = sphere && !pyreSphere;
        if (item.wet && item.lavaWet && !item.shimmerWet) {
            if (pyreSphere) {
                if (item.velocity.Y > 0.86f) {
                    item.velocity.Y *= 0.9f;
                }
                item.velocity.Y = item.velocity.Y - 0.6f;
                if (item.velocity.Y < -2f) {
                    item.velocity.Y = -2f;
                }
            }
            if (validToHandle) {
                item.velocity *= 0.9f;
                if (item.velocity.Length() < 1f) {
                    item.velocity *= 0.95f;

                    if (item.beingGrabbed) {
                        _breathCD2 = _breath2 = 0;
                    }

                    _breathCD2++;
                    if (_breathCD2 >= _breathCDMax) {
                        _breathCD2 = 0;
                        _breath2 -= 5;
                        if (_breath2 <= 0) {
                            _breath2 = 0;

                            item.ChangeItemType(ModContent.ItemType<SphereOfPyre>());
                            _cdToTransformation = 100;
                            for (int i = 0; i < 20; i++) {
                                int num = Dust.NewDust(item.Center, 1, 1, 309);
                                Main.dust[num].scale *= 1.5f;
                                Main.dust[num].color = new(249, 115, 43);
                            }
                        }
                    }
                }
            }
        }
        else if (!pyreSphere) {
            if (_breath2 != _breathMax2) {
                _breath2 = _breathMax2;
            }

            _breathCD2 = 0;
        }
    }

    private static void DrawPyre(Item item, SpriteBatch spriteBatch) {
        bool sphere = _spheresToHandle.Contains(item.type);
        if (!sphere) {
            return;
        }
        bool pyreSphere = item.type == ModContent.ItemType<SphereOfPyre>();
        bool validToHandle = sphere && !pyreSphere;
        if (!validToHandle) {
            return;
        }

        var handler = item.GetGlobalItem<SphereHandler>();
        int _breath2 = handler._breath2;
        int _breathMax2 = handler._breathMax2;
        if (_breath2 < _breathMax2) {
            int num = 50;
            for (int i = 1; i < _breathMax2 / num + 1; i++) {
                int num2 = 255;
                float num3 = 1f;
                if (_breath2 >= i * num) {
                    num2 = 255;
                }
                else {
                    float num4 = (float)(_breath2 - (i - 1) * num) / (float)num;
                    num2 = (int)(30f + 225f * num4);
                    if (num2 < 30)
                        num2 = 30;

                    num3 = num4 / 4f + 0.75f;
                    if ((double)num3 < 0.75)
                        num3 = 0.75f;
                }

                int num5 = 0;
                int num6 = 0;
                if (i > 10) {
                    num5 -= 260;
                    num6 += 26;
                }

                spriteBatch.Draw(TextureAssets.Flame.Value, item.Center - Main.screenPosition + new Vector2((float)(26 * (i - 1) + num5) - 50f,
                    -50f + ((float)TextureAssets.Flame.Height() - (float)TextureAssets.Flame.Height() * num3) / 2f + (float)num6), new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Flame.Width(), TextureAssets.Flame.Height()), new Microsoft.Xna.Framework.Color(num2, num2, num2, num2), 0f, default(Vector2), num3, SpriteEffects.None, 0f);
            }
        }
    }
    #endregion
}
