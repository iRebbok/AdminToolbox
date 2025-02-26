﻿using Smod2.Commands;
using Smod2;
using Smod2.API;
using System;
using System.IO;

namespace AdminToolbox.Command
{
	class RoundLockCommand : ICommandHandler
	{
		private readonly AdminToolbox plugin;
		static IConfigFile Config => ConfigManager.Manager.Config;
		Server Server => PluginManager.Manager.Server;

		public RoundLockCommand(AdminToolbox plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Locks the round so it cant end";
		}

		public string GetUsage()
		{
			return "ROUNDLOCK TRUE/FALSE";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			Player caller = (sender is Player _p) ? _p : null;
			if (args.Length > 0)
			{
				if (bool.TryParse(args[0], out bool k))
				{
					AdminToolbox.lockRound = k;
					if (caller != null) plugin.Info("Round lock: " + k);
					return new string[] { "Round lock: " + k };
				}
				else if (Int32.TryParse(args[0], out int l))
				{
					if (l < 1)
					{
						AdminToolbox.lockRound = false;
						if (caller != null) plugin.Info("Round lock: " + AdminToolbox.lockRound);
						return new string[] { "Round lock: " + AdminToolbox.lockRound };
					}
					else
					{
						AdminToolbox.lockRound = true;
						if (caller != null) plugin.Info("Round lock: " + AdminToolbox.lockRound);
						return new string[] { "Round lock: " + AdminToolbox.lockRound };
					}
				}
				else
					return new string[] { GetUsage() };
			}
			else
			{
				AdminToolbox.lockRound = !AdminToolbox.lockRound;
				if(caller!= null) plugin.Info("Round lock: " + AdminToolbox.lockRound);
				return new string[] { "Round lock: " + AdminToolbox.lockRound };
			}
		}
	}
}