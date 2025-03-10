using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.Achievements;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class TMLAchievement : Achievement {
	public ModAchievement ModAchievement;

	internal Asset<Texture2D> texture;

	internal Asset<Texture2D> borderTex;

	internal TMLAchievement(ModAchievement achievement, string name)
		: base(name)
	{
		ModAchievement = achievement;
		borderTex = null;
	}
}
