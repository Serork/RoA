using System.IO;
using System.Text;

using Terraria;
using Terraria.Social;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class VanillaAchievements {
    internal static void LoadVanillaAchievements() {
        try {
            bool _isCloudSave;
            string _savePath;
            byte[] _cryptoKey;
            switch (SocialAPI.Mode) {
                case SocialMode.Steam: {
                    _isCloudSave = true;
                    Terraria.Social.Steam.AchievementsSocialModule achievementsSocialModule2 = new Terraria.Social.Steam.AchievementsSocialModule();
                    _savePath = achievementsSocialModule2.GetSavePath();
                    _cryptoKey = achievementsSocialModule2.GetEncryptionKey();
                    break;
                }
                case SocialMode.WeGame: {
                    _isCloudSave = true;
                    Terraria.Social.WeGame.AchievementsSocialModule achievementsSocialModule = new Terraria.Social.WeGame.AchievementsSocialModule();
                    _savePath = achievementsSocialModule.GetSavePath();
                    _cryptoKey = achievementsSocialModule.GetEncryptionKey();
                    break;
                }
                default:
                    _isCloudSave = false;
                    _savePath = Main.SavePath + Path.DirectorySeparatorChar + "achievements.dat";
                    _cryptoKey = Encoding.ASCII.GetBytes("RELOGIC-TERRARIA");
                    break;
            }
            TMLAchievements.Log.Debug(_isCloudSave);
            TMLAchievements.Log.Debug(_savePath);
            TMLAchievements.Log.Debug(_cryptoKey);
        }
        catch {
        }
    }
}
