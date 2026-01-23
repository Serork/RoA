using Microsoft.Xna.Framework;

using RoA.Content.Items.Pets;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class SmallMoon : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Small Moon");
        // Description.SetDefault("A small moone is following you");

        Main.buffNoTimeDisplay[Type] = true;
        Main.lightPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.buffTime[buffIndex] = 18000;
        player.GetModPlayer<SmallMoonPlayer>().smallMoon = true;

        ushort _type = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Pets.SmallMoon>();
        if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[_type] <= 0)
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0.0f, 0.0f, _type, 0, 0.0f, player.whoAmI, 0.0f, 0.0f);
    }
}

sealed class SmallMoonPlayer : ModPlayer {
    public Color smallMoonColor, smallMoonColor2;
    public bool smallMoon;

    public bool HasContributor;

    private float _lerpColorProgress, _lerpColorProgress2;
    private Color _lerpColor, _lerpColor2;
    private Color? _currentColor = null, _nextColor = null;

    public override void ResetEffects() {
        smallMoon = false;
    }

    public override void OnEnterWorld() => SetInfo();

    public override void PostUpdate() {
        int type = ModContent.ItemType<MoonFlower>();
        //if (!Player.HasItem(type) && Player.whoAmI == Main.myPlayer && Main.mouseItem.type != type && !Player.miscEquips.Any(x => x.type == type)) {
        //    return;
        //}

        SetInfo();
    }

    private bool ShouldBeRandom() {
        string[] keywords = ["roa", "rise", "ages"];
        bool flag = false;
        foreach (string kw in keywords) {
            if (Player.name.ToLower().Contains(kw)) {
                flag = true;
                break;
            }
        }
        return Player.name.Equals("Serork", StringComparison.CurrentCultureIgnoreCase) || flag;
    }

    private void SetInfo() {
        if (Main.bloodMoon) smallMoonColor = Color.Red;
        else if (Main.pumpkinMoon) smallMoonColor = Color.Orange;
        else if (Main.snowMoon) smallMoonColor = Color.DeepSkyBlue;
        else {
            smallMoonColor = new Color(190, 190, 140);
        }
        HasContributor = false;

        if (Player.name.Equals("peege.on", StringComparison.CurrentCultureIgnoreCase)) {
            smallMoonColor = Player.underShirtColor;
            HasContributor = true;
        }
        if (Player.name.Equals("has2r", StringComparison.CurrentCultureIgnoreCase)) {
            smallMoonColor = Color.Indigo;
            HasContributor = true;        
        }
        if (Player.name.Equals("HalfbornFan", StringComparison.CurrentCultureIgnoreCase)) {
            smallMoonColor = Main.DiscoColor;
            HasContributor = true;
        }
        if (Player.name == string.Empty) {
            smallMoonColor = Color.Transparent;
            HasContributor = true;
        }
        smallMoonColor2 = smallMoonColor.ModifyRGB(2f);

        if (ShouldBeRandom()) {
            if (_currentColor == null) {
                _currentColor = Color.Yellow;
            }
            if (_nextColor == null) {
                _nextColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
            }
            smallMoonColor = GetLerpColor([_currentColor.Value, _nextColor.Value]);
            HasContributor = true;
        }

        SetContributorColor("cleo.", [Color.Black, Color.White]);
        SetContributorColor("NotFurryAlex", [new Color(255, 33, 140), new Color(255, 216, 0), new Color(33, 177, 255)]);
        SetContributorColor("N.F.A.", [new Color(255, 33, 140), new Color(255, 216, 0), new Color(33, 177, 255)]);
        SetContributorColor("Heretic", [new Color(88, 1, 1), new Color(232, 140, 3)]);
        SetContributorColor("BRIPE", [new Color(71, 98, 255), Color.Blue]);
        SetContributorColor("Chaos Sage", [Color.White, Color.Red]);
        SetContributorColor("KondiU", [new Color(50, 255, 0), new Color(255, 255, 255)]);
        SetContributorColor("Lion8cake", [Color.Orange, Color.White]);
        SetContributorColor("KaffaZ", [new Color(185, 0, 64), new Color(255, 195, 0), new Color(94, 0, 50)]);
        SetContributorColor("KaffaZ_Himself", [new Color(185, 0, 64), new Color(255, 195, 0), new Color(94, 0, 50)]);
        SetContributorColor("gbf", [new Color(206, 68, 178), new Color(208, 33, 33)]);

        SetContributorColor("Croul", 
            [Helper.FromHexRgb(0x2B2D42),
             Helper.FromHexRgb(0x8D99AE),
             Helper.FromHexRgb(0xEDF2F4),
             Helper.FromHexRgb(0xEF233C),
             Helper.FromHexRgb(0xD90429)]);
    }

    private void SetContributorColor(string contributor, List<Color> from) {
        if (Player.name.Equals(contributor, StringComparison.CurrentCultureIgnoreCase)) {
            smallMoonColor = GetLerpColor(from);
            HasContributor = true;
        }
    }

    private Color GetLerpColor(List<Color> from) {
        _lerpColorProgress += 0.005f;
        _lerpColorProgress2 += 0.005f;
        int colorCount = from.Count;
        for (int i = 0; i < colorCount; i++) {
            float part = 1f / colorCount;
            float min = part * i;
            float max = part * (i + 1);
            if (_lerpColorProgress >= min && _lerpColorProgress <= max) {
                _lerpColor = Color.Lerp(from[i], from[i == colorCount - 1 ? 0 : (i + 1)], Utils.Remap(_lerpColorProgress, min, max, 0f, 1f, true));
            }
            float progress2 = (_lerpColorProgress2 + 0.5f) % 1f;
            if (progress2 >= min && progress2 <= max) {
                _lerpColor2 = Color.Lerp(from[i], from[i == colorCount - 1 ? 0 : (i + 1)], Utils.Remap(progress2, min, max, 0f, 1f, true));
            }
        }
        if (ShouldBeRandom() && Math.Round(_lerpColorProgress, 2) == 0.5f) {
            _currentColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
        }
        if (_lerpColorProgress > 1f) {
            _lerpColorProgress = 0f;
            if (ShouldBeRandom()) {
                _nextColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
            }
        }
        if (_lerpColorProgress2 > 1f) {
            _lerpColorProgress2 = 0f;
        }
        smallMoonColor2 = Color.Lerp(smallMoonColor2, _lerpColor2, 0.25f);
        return _lerpColor;
    }
}