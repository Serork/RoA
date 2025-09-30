//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using ReLogic.Content;

//using RoA.Core;

//using Terraria;
//using Terraria.GameContent;
//using Terraria.GameInput;
//using Terraria.Localization;
//using Terraria.ModLoader;
//using Terraria.ModLoader.IO;
//using Terraria.UI;

//namespace RoA.Common;

//sealed class RoAAchievementInGameNotification : IInGameNotification {
//    internal class RoAAchievementStorage_Player : ModPlayer {
//        public bool DefeatLothor = false;
//        public bool MineMercuriumNugget = false;
//        public bool OpenRootboundChest = false;
//        public bool SurviveBackwoodsFog = false;
//        public bool CraftDruidWreath = false;
//        public bool DefeatLothorEnraged = false;

//        public override void SaveData(TagCompound tag) {
//            if (DefeatLothor) {
//                tag["DefeatLothor"] = true;
//            }
//            if (MineMercuriumNugget) {
//                tag["MineMercuriumNugget"] = true;
//            }
//            if (OpenRootboundChest) {
//                tag["OpenRootboundChest"] = true;
//            }
//            if (SurviveBackwoodsFog) {
//                tag["SurviveBackwoodsFog"] = true;
//            }
//            if (CraftDruidWreath) {
//                tag["CraftDruidWreath"] = true;
//            }
//            if (DefeatLothorEnraged) {
//                tag["DefeatLothorEnraged"] = true;
//            }
//        }

//        public override void LoadData(TagCompound tag) {
//            DefeatLothor = tag.ContainsKey("DefeatLothor");
//            MineMercuriumNugget = tag.ContainsKey("MineMercuriumNugget");
//            OpenRootboundChest = tag.ContainsKey("OpenRootboundChest");
//            SurviveBackwoodsFog = tag.ContainsKey("SurviveBackwoodsFog");
//            CraftDruidWreath = tag.ContainsKey("CraftDruidWreath");
//            DefeatLothorEnraged = tag.ContainsKey("DefeatLothorEnraged");
//        }
//    }

//    //internal class RoAAchievementStorage_World : ModSystem {
//    //    public static bool DefeatLothor = false;
//    //    public static bool MineMercuriumNugget = false;
//    //    public static bool OpenRootboundChest = false;
//    //    public static bool SurviveBackwoodsFog = false;
//    //    public static bool CraftDruidWreath = false;
//    //    public static bool DefeatLothorEnraged = false;

//    //    public override void ClearWorld() {
//    //        DefeatLothor = false;
//    //        MineMercuriumNugget = false;
//    //        OpenRootboundChest = false;
//    //        SurviveBackwoodsFog = false;
//    //        CraftDruidWreath = false;
//    //        DefeatLothorEnraged = false;
//    //    }

//    //    public override void SaveWorldData(TagCompound tag) {
//    //        if (DefeatLothor) {
//    //            tag["DefeatLothor"] = true;
//    //        }
//    //        if (MineMercuriumNugget) {
//    //            tag["MineMercuriumNugget"] = true;
//    //        }
//    //        if (OpenRootboundChest) {
//    //            tag["OpenRootboundChest"] = true;
//    //        }
//    //        if (SurviveBackwoodsFog) {
//    //            tag["SurviveBackwoodsFog"] = true;
//    //        }
//    //        if (CraftDruidWreath) {
//    //            tag["CraftDruidWreath"] = true;
//    //        }
//    //        if (DefeatLothorEnraged) {
//    //            tag["DefeatLothorEnraged"] = true;
//    //        }
//    //    }

//    //    public override void LoadWorldData(TagCompound tag) {
//    //        DefeatLothor = tag.ContainsKey("DefeatLothor");
//    //        MineMercuriumNugget = tag.ContainsKey("MineMercuriumNugget");
//    //        OpenRootboundChest = tag.ContainsKey("OpenRootboundChest");
//    //        SurviveBackwoodsFog = tag.ContainsKey("SurviveBackwoodsFog");
//    //        CraftDruidWreath = tag.ContainsKey("CraftDruidWreath");
//    //        DefeatLothorEnraged = tag.ContainsKey("DefeatLothorEnraged");
//    //    }
//    //}

//    private Asset<Texture2D> _achievementTexture;
//    private Asset<Texture2D> _achievementBorderTexture;
//    private const int _iconSize = 64;
//    private const int _iconSizeWithSpace = 66;
//    private const int _iconsPerRow = 8;
//    private int _iconIndex;
//    private Rectangle _achievementIconFrame;
//    private string _title;
//    private int _ingameDisplayTimeLeft;

//    public bool ShouldBeRemoved { get; private set; }

//    public object CreationObject { get; private set; }

//    private float Scale {
//        get {
//            if (_ingameDisplayTimeLeft < 30)
//                return MathHelper.Lerp(0f, 1f, (float)_ingameDisplayTimeLeft / 30f);

//            if (_ingameDisplayTimeLeft > 285)
//                return MathHelper.Lerp(1f, 0f, ((float)_ingameDisplayTimeLeft - 285f) / 15f);

//            return 1f;
//        }
//    }

//    private float Opacity {
//        get {
//            float scale = Scale;
//            if (scale <= 0.5f)
//                return 0f;

//            return (scale - 0.5f) / 0.5f;
//        }
//    }

//    private string _achievementName;

//    public RoAAchievementInGameNotification(string achievementName) {
//        _achievementName = achievementName;

//        string title = Language.GetTextValue($"Mods.RoA.Achievements.{_achievementName}.Name");
//        Asset<Texture2D> iconTexture = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + $"Achievement_{_achievementName}", AssetRequestMode.AsyncLoad);

//        _ingameDisplayTimeLeft = 300;
//        _title = title;
//        int num = (_iconIndex = 0);
//        _achievementIconFrame = new Rectangle(num % 8 * 66, num / 8 * 66, 64, 64);
//        _achievementTexture = iconTexture;
//        _achievementBorderTexture = Main.Assets.Request<Texture2D>("Images/UI/Achievement_Borders", AssetRequestMode.AsyncLoad);
//    }

//    public void Update() {
//        _ingameDisplayTimeLeft--;
//        if (_ingameDisplayTimeLeft < 0)
//            _ingameDisplayTimeLeft = 0;
//    }

//    public void PushAnchor(ref Vector2 anchorPosition) {
//        float num = 50f * Opacity;
//        anchorPosition.Y -= num;
//    }

//    public void DrawInGame(SpriteBatch sb, Vector2 bottomAnchorPosition) {
//        float opacity = Opacity;
//        if (opacity > 0f) {
//            float num = Scale * 1.1f;
//            Vector2 size = (FontAssets.ItemStack.Value.MeasureString(_title) + new Vector2(58f, 10f)) * num;
//            Rectangle r = Utils.CenteredRectangle(bottomAnchorPosition + new Vector2(0f, (0f - size.Y) * 0.5f), size);
//            Vector2 mouseScreen = Main.MouseScreen;
//            bool num2 = r.Contains(mouseScreen.ToPoint());
//            Utils.DrawInvBG(c: num2 ? (new Color(64, 109, 164) * 0.75f) : (new Color(64, 109, 164) * 0.5f), sb: sb, R: r);
//            float num3 = num * 0.3f;
//            Vector2 vector = r.Right() - Vector2.UnitX * num * (12f + num3 * (float)_achievementIconFrame.Width);
//            sb.Draw(_achievementTexture.Value, vector, _achievementIconFrame, Color.White * opacity, 0f, new Vector2(0f, _achievementIconFrame.Height / 2), num3, SpriteEffects.None, 0f);
//            sb.Draw(_achievementBorderTexture.Value, vector, null, Color.White * opacity, 0f, new Vector2(0f, _achievementIconFrame.Height / 2), num3, SpriteEffects.None, 0f);
//            Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, (int)Main.mouseTextColor / 5, Main.mouseTextColor) * opacity, sb: sb, text: _title, pos: vector - Vector2.UnitX * 10f, scale: num * 0.9f, anchorx: 1f, anchory: 0.4f);
//            if (num2)
//                OnMouseOver();
//        }
//    }

//    private void OnMouseOver() {
//        if (!PlayerInput.IgnoreMouseInterface) {
//            Main.player[Main.myPlayer].mouseInterface = true;
//            if (Main.mouseLeft && Main.mouseLeftRelease) {
//                Main.mouseLeftRelease = false;

//                Main.NewText(Language.GetTextValue("Mods.RoA.AchievementModNotification"));

//                //IngameFancyUI.OpenAchievementsAndGoto(_theAchievement);
//                _ingameDisplayTimeLeft = 0;
//                ShouldBeRemoved = true;
//            }
//        }
//    }

//    public void DrawInNotificationsArea(SpriteBatch spriteBatch, Rectangle area, ref int gamepadPointLocalIndexTouse) {
//        Utils.DrawInvBG(spriteBatch, area, Color.Red);
//    }
//}
