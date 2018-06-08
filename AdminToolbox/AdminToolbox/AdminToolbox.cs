﻿using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using Smod2.API;
using ServerMod2.API;
using System;
using System.IO;
using System.Collections.Generic;
using Unity;
using UnityEngine;

namespace AdminToolbox
{
    [PluginDetails(
        author = "Evan (AKA Rnen)",
        name = "Admin Toolbox",
        description = "Plugin for advanced admin tools",
        id = "rnen.admin.toolbox",
        version = "1.2",
        SmodMajor = 3,
        SmodMinor = 3,
        SmodRevision = 0
        )]
    class AdminToolbox : Plugin
    {
        public static bool isRoundFinished = false;
        public static bool evanSpectator_onRespawn = false;
        public static bool adminMode = false;
        public static bool lockRound = false;
        public static bool lockDown = false;

        public static int[] nineTailsTeam = { 1, 3 };
        public static int[] chaosTeam = { 2, 4 };

        public static Dictionary<string, List<bool>> playerdict = new Dictionary<string, List<bool>>();
        public static Dictionary<string, Vector> warpVectors = new Dictionary<string, Vector>();
        public static List<string> logText = new List<string>();
        public static int roundCount = 0;

        public override void OnDisable()
        {
        }
        public static void SetPlayerBools(Player player, bool keepSettings, bool godMode, bool dmgOff, bool destroyDoor)
        {
            playerdict[player.SteamId][0] = keepSettings;
            playerdict[player.SteamId][1] = godMode;
            playerdict[player.SteamId][2] = dmgOff;
            playerdict[player.SteamId][3] = destroyDoor;
        }

        public override void OnEnable()
        {
            this.Info(this.Details.name + " loaded sucessfully");
        }

        public override void Register()
        {
            // Register Events
            this.AddEventHandlers(new RoundEventHandler(this), Priority.High);
            this.AddEventHandler(typeof(IEventHandlerPlayerHurt), new DamageDetect(this), Priority.High);
            this.AddEventHandler(typeof(IEventHandlerPlayerDie), new DieDetect(this), Priority.High);
            this.AddEventHandler(typeof(IEventHandlerPlayerJoin), new PlayerJoinHandler(this), Priority.Highest);
            this.AddEventHandlers(new MyMiscEvents(this));

            //this.AddEventHandler(typeof(), new PlayerLeaveHandler(), Priority.Highest);

            // Register Commands
            this.AddCommand("spectator", new Command.SpectatorCommand(this));
            this.AddCommand("players", new Command.PlayerList(this));
            this.AddCommand("tpx", new Command.TeleportCommand(this));
            this.AddCommand("heal", new Command.HealCommand(this));
            this.AddCommand("god", new Command.GodModeCommand(this));
            this.AddCommand("godmode", new Command.GodModeCommand(this));
            this.AddCommand("nodmg", new Command.NoDmgCommand(this));
            this.AddCommand("tut", new Command.SetTutorial(this));
            this.AddCommand("tutorial", new Command.SetTutorial(this));
            this.AddCommand("role", new Command.SetPlayerRole(this));
            //this.AddCommand("keep", new Command.KeepSettings(this));
            //this.AddCommand("keepsettings", new Command.KeepSettings(this));
            this.AddCommand("hp", new Command.SetHpCommand(this));
            this.AddCommand("sethp", new Command.SetHpCommand(this));
            this.AddCommand("player", new Command.PlayerCommand(this));
            this.AddCommand("pos", new Command.PosCommand(this));
            this.AddCommand("warp", new Command.WarpCommmand(this));
            this.AddCommand("roundlock", new Command.RoundLock(this));
            this.AddCommand("rlock", new Command.RoundLock(this));
            this.AddCommand("lockdown", new Command.LockdownCommand(this));
            this.AddCommand("breakdoors", new Command.BreakDoorsCommand(this));
            this.AddCommand("bd", new Command.BreakDoorsCommand(this));
            //this.AddCommand("test", new Command.Test(this));
            // Register config settings
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_tutorial_dmg_allowed", new int[] { -1 }, Smod2.Config.SettingType.NUMERIC_LIST, true, "What (int)damagetypes TUTORIAL is allowed"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_damagetypes", new int[] { 5, 13, 14, 15, 16, 17 }, Smod2.Config.SettingType.NUMERIC_LIST, true, "What (int)damagetypes to debug"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_server", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_spectator", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_tutorial", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_player_damage", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_friendly_damage", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_player_kill", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_friendly_kill", true, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_scp_and_self_killed", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_endedRound_damageMultiplier", 1, Smod2.Config.SettingType.NUMERIC, true, "Damage multiplier after end of round"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_writeTkToFile", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_debug_player_joinANDleave", false, Smod2.Config.SettingType.BOOL, true, "true/false"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_intercom_extended_IDs_whitelist", new string[] {  }, Smod2.Config.SettingType.LIST, true, "What STEAMID's can use the Intercom freely"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_intercom_extended_duration", 1000f, Smod2.Config.SettingType.FLOAT, true, "How long people in the extended ID's list can talk"));
            this.AddConfig(new Smod2.Config.ConfigSetting("admintoolbox_intercom_extended_cooldown", 0f, Smod2.Config.SettingType.FLOAT, true, "How long cooldown after whitelisted people have used it"));
        }
    }

    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
    public class GetPlayerFromString
    {
        public static Player GetPlayer(string args, out Player playerOut)
        {
            int maxNameLength = 31;
            int LastnameDifference = 31;
            Player plyer = null;
            string str1 = args.ToLower();
            foreach (Player pl in PluginManager.Manager.Server.GetPlayers())
            {
                if (!pl.Name.ToLower().Contains(args.ToLower())) { goto NoPlayer; }
                if (str1.Length < maxNameLength)
                {
                    int x = maxNameLength - str1.Length;
                    int y = maxNameLength - pl.Name.Length;
                    string str2 = pl.Name;
                    for (int i = 0; i < x; i++)
                    {
                        str1 += "z";
                    }
                    for (int i = 0; i < y; i++)
                    {
                        str2 += "z";
                    }
                    int nameDifference = LevenshteinDistance.Compute(str1, str2);
                    if (nameDifference < LastnameDifference)
                    {
                        LastnameDifference = nameDifference;
                        plyer = pl;
                    }
                }
                NoPlayer:;
            }
            playerOut = plyer;
            return playerOut;
        }
    }
    public class LogHandler
    {
        public static void WriteToLog(string str)
        {
            if (!ConfigManager.Manager.Config.GetBoolValue("admintoolbox_writeTkToFile", false, false)) return;
            AdminToolbox.logText.Add(System.DateTime.Now.ToString() + ": " + str + "\n");
            string myLog = null;
            foreach (var item in AdminToolbox.logText)
            {
                myLog += item + Environment.NewLine;
            }
            Server server = PluginManager.Manager.Server;
            string fileName = server.Name.ToString() + "_AdminToolbox_TKLog.txt";
            File.WriteAllText(fileName, myLog);
        }
    }
}