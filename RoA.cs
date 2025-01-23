using Microsoft.Xna.Framework;

using MonoMod.RuntimeDetour;

using ReLogic.Content.Sources;

using RoA.Common.Networking;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.ModLoader;

namespace RoA;

sealed class RoA : Mod {
    public enum NetMessagePacket : byte {
        EvilLeafPacket,
    }

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

        NetMessagePacket msgType = (NetMessagePacket)reader.ReadByte();
        switch (msgType) {
            case NetMessagePacket.EvilLeafPacket:
                int identity = reader.ReadInt32();
                Vector2 twigPosition = reader.ReadVector2();
                Projectile projectile = Main.projectile.FirstOrDefault(x => x.identity == identity);
                projectile.As<EvilLeaf>().
                    SetUpTwigPosition(twigPosition);
                break;
        }
    }

    public override void PostSetupContent() {
        foreach (IPostSetupContent type in GetContent<IPostSetupContent>()) {
            type.PostSetupContent();
        }
    }

    public static Hook Detour(MethodInfo source, MethodInfo target) {
        Hook hook = new(source, target);
        hook.Apply();

        return hook;
    }

}
