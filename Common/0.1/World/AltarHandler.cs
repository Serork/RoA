using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.World;

sealed class AltarHandler : ModSystem {
    private static Point _altarPosition;
    private static float _altarStrength, _altarFactor;
    private static bool _shouldUpdateAudio;
    private static float _musicFade;

    public static float Counting { get; private set; }
    public static float MiracleMintCounting { get; private set; }
    public static float Counting2 { get; private set; }

    internal static void SetPosition(Point altarPosition) {
        //if (!WorldGen.gen) {
        //    return;
        //}

        _altarPosition = altarPosition;
    }

    private sealed class LothorSummoningHandler2 : ModSceneEffect {
        public override int Music => MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "Lothor");

        public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override bool IsSceneEffectActive(Player player) {
            bool result = LothorSummoningHandler.ActiveMessages.Item1 && !NPC.AnyDanger(true);
            return result;
        }
    }

    public static Point GetAltarPosition() => _altarPosition;
    public static float GetAltarStrength() => _altarStrength;
    public static float GetAltarFactor() => _altarFactor;

    public override void Load() {
        On_Main.UpdateAudio += On_Main_UpdateAudio;
        On_LegacyAudioSystem.UpdateCommonTrack += On_LegacyAudioSystem_UpdateCommonTrack;
    }

    private void On_LegacyAudioSystem_UpdateCommonTrack(On_LegacyAudioSystem.orig_UpdateCommonTrack orig, LegacyAudioSystem self, bool active, int i, float totalVolume, ref float tempFade) {
        //if (LothorSummoningHandler.ActiveMessages.Item1) {
        //    i = MusicLoader.GetMusicSlot(ResourceManager.Music + "Lothor");
        //}

        ////if (_shouldUpdateAudio) {
        ////    if (Main.netMode != NetmodeID.Server) {
        ////        totalVolume = Math.Min(totalVolume, MathHelper.Clamp(1f - _altarStrength * 2f, 0f, 1f));
        ////    }
        ////}

        orig(self, active, i, totalVolume, ref tempFade);
    }

    private void On_Main_UpdateAudio(On_Main.orig_UpdateAudio orig, Main self) {
        if (_shouldUpdateAudio) {
            if (Main.netMode != NetmodeID.Server) {
                Main.musicFade[Main.curMusic] = MathHelper.Clamp(1f - _altarStrength * 2f, 0f, 1f);
                _musicFade = Main.musicFade[Main.curMusic];
            }
        }

        orig(self);
    }

    public override void PostUpdatePlayers() {
        MiracleMintCounting += (float)Math.Round(TimeSystem.LogicDeltaTime / 3f + Main.rand.NextFloatRange(0.015f), 2);

        if (MiracleMintCounting >= 1f) {
            MiracleMintCounting = 0f;
        }

        float counting = MathHelper.Clamp(1f - Counting, 0f, 1f);
        float factor = AltarHandler.GetAltarFactor();
        bool flag6 = LothorSummoningHandler.PreArrivedLothorBoss.Item1 || LothorSummoningHandler.PreArrivedLothorBoss.Item2;
        bool flag7 = NPC.AnyNPCs(ModContent.NPCType<Lothor>());
        if (flag6 || flag7) {
            factor = 1f;
        }
        Counting2 = MathHelper.Lerp(Counting2, counting, factor > 0.5f ? Math.Max(0.1f, counting * 0.1f) : counting < 0.5f ? 0.075f : Math.Max(0.05f, counting * 0.025f));
        Counting += TimeSystem.LogicDeltaTime / (3f - MathHelper.Min(0.9f, factor) * 2.5f) * Math.Max(0.05f, Counting) * 7f;
        Point Position = GetAltarPosition();
        if (Counting > 0.8f) {
            if (factor > 0f && Main.rand.NextChance(1f - (double)Math.Min(0.25f, factor - 0.5f))) {
                float volume = 2.5f * Math.Max(0.3f, factor + 0.1f);
                float dist = Vector2.Distance(Main.LocalPlayer.Center, Position.ToWorldCoordinates());
                float dist2 = MathHelper.Clamp(1f - dist / 1400f, 0f, 1f);
                volume *= dist2;
                if (flag7) {
                    volume *= 0.25f;
                }
                var style = new SoundStyle(ResourceManager.AmbientSounds + "Heartbeat") { Volume = volume };
                var sound = SoundEngine.FindActiveSound(in style);
                if (Main.netMode == NetmodeID.Server) {
                    MultiplayerSystem.SendPacket(new PlayHeartbeatSoundPacket(Main.LocalPlayer, Position.X, Position.Y, volume), ignoreClient: -1);
                }
                else {
                    SoundEngine.PlaySound(style, new Microsoft.Xna.Framework.Vector2(Position.X, Position.Y) * 16f);
                }
            }
        }
        if (Counting >= 1.25f) {
            Counting = 0f;
        }
    }

    public override void PostUpdateNPCs() {
        if (!NPC.downedBoss2) {
            return;
        }

        Vector2 altarCoords = GetAltarPosition().ToWorldCoordinates();
        int type = ModContent.NPCType<DruidSoul>();
        NPC druidSoul = null;
        bool flag = NPC.AnyNPCs(type);
        bool flag6 = LothorSummoningHandler.PreArrivedLothorBoss.Item1 || LothorSummoningHandler.PreArrivedLothorBoss.Item2;
        if (flag6) {
            bool check = LothorSummoningHandler.ActiveMessages.Item1 && !NPC.AnyDanger(true);
            if (check) {
                if (_musicFade < 1f) {
                    _musicFade += 0.005f;
                }
                if (_musicFade < 0.2f) {
                    Main.musicFade[Main.curMusic] *= MathHelper.Clamp(1f - _altarStrength * 2f, 0f, 1f);
                }
                else {
                    Main.musicFade[Main.curMusic] *= Utils.Remap(_musicFade, 0f, 1f, 0.2f, 1f, true);
                }
            }
            else {
                if (LothorSummoningHandler.PreArrivedLothorBoss.Item1 && !NPC.AnyDanger(true)) {
                    Main.musicFade[Main.curMusic] *= MathHelper.Clamp(1f - _altarStrength * 2f, 0f, 1f);
                }
                if (_musicFade > 0f) {
                    _musicFade -= 0.005f;
                }
            }
        }
        _shouldUpdateAudio = false;
        if (!flag6 && !flag) {
            if (_altarStrength > 0f) {
                _altarStrength -= 0.01f;
            }
            if (_altarFactor > 0f) {
                _altarFactor -= 0.01f;
            }
            return;
        }
        if (!flag) {
            if (_altarStrength >= 0.35f) {
                _altarStrength -= 0.00075f;
                _altarStrength *= 0.995f;
            }
            else if (flag6) {
                _altarStrength += 0.015f;
            }
            if (flag6) {
                _altarFactor = 1f;
            }
            if (flag6 && LothorSummoningHandler.IsActive) {
                float value = MathHelper.Clamp((float)Math.Pow(LothorSummoningHandler._alpha * 5f, 5.0), 0f, 1f);
                _altarStrength *= value;
                _altarFactor *= value;
            }
            return;
        }
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.active && npc.type == type) {
                druidSoul = npc;
                break;
            }
        }
        if (druidSoul is null) {
            return;
        }
        _shouldUpdateAudio = true;
        Vector2 npcCenter = druidSoul.Center;
        float radius = 100f;
        float x = npcCenter.X - altarCoords.X;
        float y = npcCenter.Y - altarCoords.Y;
        float distance = (float)Math.Sqrt(x * x + y * y);
        float maxDist = 1000f;
        float distance2 = druidSoul.Distance(altarCoords);
        bool flag2 = distance2 < maxDist;
        bool flag3 = distance2 < radius;
        _altarFactor = MathHelper.SmoothStep(_altarFactor, flag3 ? 1f : flag2 ? (float)Math.Round(1f - (distance / maxDist) * 1.25f, 2) : 0f, Math.Max(0.1f, _altarFactor * 0.1f));
        _altarFactor = MathHelper.Clamp(_altarFactor, 0.01f, 1f);
        bool flag7 = druidSoul.As<DruidSoul>().ShouldConsumeItsEnergy && !flag6 && _altarFactor >= 0.85f;
        _altarStrength += (float)Math.Round(TimeSystem.LogicDeltaTime / 5f * (flag7 ? 0.5f : (flag6 ? -1.65f : -2f)), 3);
        _altarStrength = MathHelper.Clamp(_altarStrength, 0f, 1f);
        if (_altarStrength >= 0.05f && _altarStrength <= 0.95f) {
            PunchCameraModifier punchCameraModifier = new(altarCoords, (Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(), _altarStrength * 1.25f * Main.rand.NextFloat(5f, 10f), _altarStrength * 1.25f * Main.rand.NextFloat(3f, 4.5f), 20, 2000f, "Lothor");
            Main.instance.CameraModifiers.Add(punchCameraModifier);
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.WriteVector2(_altarPosition.ToVector2());
        writer.Write(_altarStrength);
        writer.Write(_altarFactor);
    }

    public override void NetReceive(BinaryReader reader) {
        _altarPosition = reader.ReadVector2().ToPoint();
        _altarStrength = reader.ReadSingle();
        _altarFactor = reader.ReadSingle();
    }

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();
    private static void Reset() {
        _altarPosition = Point.Zero;
        _altarStrength = _altarFactor = 0f;
    }

    public override void SaveWorldData(TagCompound tag) {
        tag.Add(RoA.ModName + nameof(_altarPosition) + "x", _altarPosition.X);
        tag.Add(RoA.ModName + nameof(_altarPosition) + "y", _altarPosition.Y);
        tag.Add(RoA.ModName + nameof(_altarStrength), _altarStrength);
    }

    public override void LoadWorldData(TagCompound tag) {
        _altarPosition = new Point(tag.GetInt(RoA.ModName + nameof(_altarPosition) + "x"), tag.GetInt(RoA.ModName + nameof(_altarPosition) + "y"));
        _altarStrength = tag.GetFloat(RoA.ModName + nameof(_altarStrength));
    }
}
