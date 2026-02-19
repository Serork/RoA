using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Biomes.Backwoods;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Ambience;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Net;

namespace RoA.Common.CustomSkyAmbience;

sealed class CustomAmbienceServer : ILoadable {
    public void Load(Mod mod) {
        WorldGen.Hooks.OnWorldLoad += delegate {
            customAmbienceServer = new CustomAmbienceServer();
        };
        //On_AmbienceServer.Update += On_AmbienceServer_Update;
        On_BackgroundChangeFlashInfo.UpdateFlashValues += On_BackgroundChangeFlashInfo_UpdateFlashValues;
        On_AmbienceServer.IsSunnyDay += On_AmbienceServer_IsSunnyDay;
    }

    private void On_BackgroundChangeFlashInfo_UpdateFlashValues(On_BackgroundChangeFlashInfo.orig_UpdateFlashValues orig, BackgroundChangeFlashInfo self) {
        if (((Main.netMode != NetmodeID.Server && !Main.gameMenu && !Main.gamePaused)) && customAmbienceServer != null)
            customAmbienceServer.Update();

        orig(self);
    }

    private bool On_AmbienceServer_IsSunnyDay(On_AmbienceServer.orig_IsSunnyDay orig) {
        //foreach (Player player in Main.ActivePlayers) {
        //    if (player.InModBiome<BackwoodsBiome>()) {
        //        return false;
        //    }
        //}

        return orig();
    }

    public void Unload() {

    }

    internal static CustomAmbienceServer customAmbienceServer;

    public struct AmbienceSpawnInfo {
        public CustomSkyEntityType skyEntityType;
        public int targetPlayer;
    }

    private const int MINIMUM_SECONDS_BETWEEN_SPAWNS = 10;
    private const int MAXIMUM_SECONDS_BETWEEN_SPAWNS = 120;
    private readonly Dictionary<CustomSkyEntityType, Func<bool>> _spawnConditions = new Dictionary<CustomSkyEntityType, Func<bool>>();
    private readonly Dictionary<CustomSkyEntityType, Func<Player, bool>> _secondarySpawnConditionsPerPlayer = new Dictionary<CustomSkyEntityType, Func<Player, bool>>();
    private double _updatesUntilNextAttempt;
    private List<AmbienceSpawnInfo> _forcedSpawns = new List<AmbienceSpawnInfo>();

    private static bool IsInBackwoods() {
        if (!Main.IsItRaining && Main.dayTime)
            return !Main.eclipse;

        return false;
    }

    private static bool IsSunnyDay() {
        if (!Main.IsItRaining && Main.dayTime)
            return !Main.eclipse;

        return false;
    }

    private static bool IsSunset() {
        if (Main.dayTime)
            return Main.time > 40500.0;

        return false;
    }

    private static bool IsCalmNight() {
        if (!Main.IsItRaining && !Main.dayTime && !Main.bloodMoon && !Main.pumpkinMoon)
            return !Main.snowMoon;

        return false;
    }

    public CustomAmbienceServer() {
        ResetSpawnTime();
        _spawnConditions[CustomSkyEntityType.BackwoodsBirdsV] = () => IsSunnyDay();
        _secondarySpawnConditionsPerPlayer[CustomSkyEntityType.BackwoodsBirdsV] = (Player player) => player.InModBiome<BackwoodsBiome>();
    }

    private bool IsPlayerAtRightHeightForType(CustomSkyEntityType type, Player plr) {
        if (type == CustomSkyEntityType.BackwoodsBirdsV)
            return plr.InModBiome<BackwoodsBiome>();

        return IsPlayerInAPlaceWhereTheyCanSeeAmbienceSky(plr);
    }

    public void Update() {
        SpawnForcedEntities();
        //Main.NewText(_updatesUntilNextAttempt);
        if (_updatesUntilNextAttempt > 0) {
            _updatesUntilNextAttempt -= Main.dayRate * 10;
            return;
        }

        ResetSpawnTime();
        IEnumerable<CustomSkyEntityType> source = from pair in _spawnConditions
                                                  where pair.Value()
                                                  select pair.Key;

        if (source.Count((type) => true) == 0)
            return;

        FindPlayerThatCanSeeBackgroundAmbience(out var player);
        if (player == null)
            return;

        IEnumerable<CustomSkyEntityType> source2 = source.Where((type) => IsPlayerAtRightHeightForType(type, player) && _secondarySpawnConditionsPerPlayer.ContainsKey(type) && _secondarySpawnConditionsPerPlayer[type](player));
        int num = source2.Count((type) => true);
        if (num == 0 || Main.rand.Next(5) < 3) {
            source2 = source.Where((type) => IsPlayerAtRightHeightForType(type, player) && (!_secondarySpawnConditionsPerPlayer.ContainsKey(type) || _secondarySpawnConditionsPerPlayer[type](player)));
            num = source2.Count((type) => true);
        }
        if (num != 0) {
            CustomSkyEntityType type2 = source2.ElementAt(Main.rand.Next(num));
            SpawnForPlayer(player, type2);
        }
    }

    public void ResetSpawnTime() {
        _updatesUntilNextAttempt = Main.rand.Next(1200, 7200);
        if (Main.tenthAnniversaryWorld)
            _updatesUntilNextAttempt /= 2;
    }

    public void ForceEntitySpawn(AmbienceSpawnInfo info) {
        _forcedSpawns.Add(info);
    }

    private void SpawnForcedEntities() {
        if (_forcedSpawns.Count == 0)
            return;

        for (int num = _forcedSpawns.Count - 1; num >= 0; num--) {
            AmbienceSpawnInfo ambienceSpawnInfo = _forcedSpawns[num];
            Player player;
            if (ambienceSpawnInfo.targetPlayer == -1)
                FindPlayerThatCanSeeBackgroundAmbience(out player);
            else
                player = Main.player[ambienceSpawnInfo.targetPlayer];

            if (player != null && IsPlayerAtRightHeightForType(ambienceSpawnInfo.skyEntityType, player))
                SpawnForPlayer(player, ambienceSpawnInfo.skyEntityType);

            _forcedSpawns.RemoveAt(num);
        }
    }

    private static void FindPlayerThatCanSeeBackgroundAmbience(out Player player) {
        player = null;
        int num = Main.player.Count((plr) => plr.active && IsPlayerInAPlaceWhereTheyCanSeeAmbience(plr));
        if (num != 0)
            player = Main.player.Where((plr) => plr.active && IsPlayerInAPlaceWhereTheyCanSeeAmbience(plr)).ElementAt(Main.rand.Next(num));
    }

    private static bool IsPlayerInAPlaceWhereTheyCanSeeAmbience(Player plr) {
        if (!IsPlayerInAPlaceWhereTheyCanSeeAmbienceSky(plr))
            return IsPlayerInAPlaceWhereTheyCanSeeAmbienceHell(plr);

        return true;
    }

    private static bool IsPlayerInAPlaceWhereTheyCanSeeAmbienceSky(Player plr) => plr.position.Y <= Main.worldSurface * 16.0 + 1600.0;
    private static bool IsPlayerInAPlaceWhereTheyCanSeeAmbienceHell(Player plr) => plr.position.Y >= (Main.UnderworldLayer - 100) * 16;

    private void SpawnForPlayer(Player player, CustomSkyEntityType type) {
        if (Main.remixWorld)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new CustomAmbiencePacket(player, type, Main.rand.Next()));
            return;
        }

        if (Main.netMode != NetmodeID.SinglePlayer) {
            return;
        }
        int seed = Main.rand.Next();
        Main.QueueMainThreadAction(delegate {
            ((CustomAmbientSky)SkyManager.Instance["CustomAmbience"]).Spawn(player, type, seed);
        });
    }
}
