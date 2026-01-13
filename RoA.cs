using Microsoft.Xna.Framework.Graphics;

using MonoMod.RuntimeDetour;

using ReLogic.Content.Sources;

using RoA.Common.Crossmod;
using RoA.Common.CustomSkyAmbience;
using RoA.Common.Networking;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Weapons.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System.IO;
using System.Reflection;

using Terraria;
using Terraria.Initializers;
using Terraria.ModLoader;
using Terraria.Net;

namespace RoA;

sealed partial class RoA : Mod {
    public static readonly string ModSourcePath = Path.Combine(Program.SavePathShared, "ModSources");

    private static RoA? _instance;

    public RoA() {
        _instance = this;
    }

    public static RoA Instance => _instance ??= ModContent.GetInstance<RoA>();

    public static string ModName => Instance.Name;

    public override IContentSource CreateDefaultContentSource() => new CustomContentSource(base.CreateDefaultContentSource());

    public override void HandlePacket(BinaryReader reader, int sender) {
        MultiplayerSystem.HandlePacket(reader, sender);
    }

    public override void Load() {
        if (!Main.dedServ) {
            Main.RunOnMainThread(() => {
                _brilliantBouquetTextureForRecipeBrowser = Helper.ResizeImage(ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<BrilliantBouquet>()).Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad), 24, 24);
                _fenethsWreathTextureForRecipeBrowser = Helper.ResizeImage(ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<FenethsBlazingWreath>()).Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad), 24, 24);
            });
        }

        On_NetworkInitializer.Load += On_NetworkInitializer_Load;
    }

    private void On_NetworkInitializer_Load(On_NetworkInitializer.orig_Load orig) {
        orig();
        //NetManager.Instance.Register<CustomNetAmbienceModule>();
    }

    public override void PostSetupContent() {
        foreach (IPostSetupContent type in GetContent<IPostSetupContent>()) {
            type.PostSetupContent();
        }

        //LoadAchievements();

        DoBossChecklistIntegration();
        DoMusicDisplayIntegration();
        DoRecipeBrowserIntergration();

        NetManager.Instance.Register<CustomNetAmbienceModule>();
    }

    public override object Call(params object[] args) => DruidModCalls.Call(args);

    public static Hook Detour(MethodInfo source, MethodInfo target) {
        Hook hook = new(source, target);
        hook.Apply();

        return hook;
    }
}