using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using CommandSystem;
using CommandSystem.Commands;
using Exiled.Events.EventArgs.Warhead;

namespace Anti_nuke_camp
{
    public class AntiNukeCamp : Plugin<Config>
    {
        public static int activationCount = 0; // Make this public and static
        public static bool IsWarheadLocked = false;

        public override string Name => "AntiNukeCamp";
        public override string Author => "Joseph_fallen";
        public override Version RequiredExiledVersion => new Version(5, 0, 0);
        public override string Prefix => "antinukecamp";
        public override Version Version => new Version(2, 0, 2);

        public override void OnEnabled()
        {
            Log.Info("AntiNukeCamp has been loaded successfully!");
            Log.Info("This Plugin is under heavy development");

            Task.Run(async () => await CheckAndUpdatePlugin());

            Exiled.Events.Handlers.Warhead.Stopping += OnWarheadStopping;
            Exiled.Events.Handlers.Warhead.Starting += OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Detonating += OnWarheadDetonating;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus += OnWarheadChangingLeverStatus;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Warhead.Stopping -= OnWarheadStopping;
            Exiled.Events.Handlers.Warhead.Starting -= OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Detonating -= OnWarheadDetonating;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus -= OnWarheadChangingLeverStatus;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;

            base.OnDisabled();
        }

        private void OnWarheadStopping(StoppingEventArgs ev)
        {
            activationCount++;

            if (activationCount <= 3)
            {
                IsWarheadLocked = false;
                ev.IsAllowed = true;
            }
            else
            {
                IsWarheadLocked = true;
                ev.IsAllowed = false;

                Log.Info("Nuke has been disabled.");
                Log.Info("Playing CASSIE Announcement");

                SendCassieMessage("ALPHA WARHEAD EMERGENCY DETONATION SEQUENCE LOCKED");

                Log.Info("Announcement Played");
            }
        }

        private void OnWarheadStarting(StartingEventArgs ev)
        {
            Log.Info("Warhead is starting...");
            // Handle the warhead starting event if needed
        }

        private void OnWarheadDetonating(DetonatingEventArgs ev)
        {
            Log.Info("Warhead is detonating...");
            // Handle the warhead detonating event if needed
        }

        private void OnWarheadChangingLeverStatus(ChangingLeverStatusEventArgs ev)
        {
            Log.Info("Warhead lever status is changing...");
            // Handle the warhead lever status changing event if needed
        }

        private void OnRoundStart()
        {
            activationCount = 0;
            IsWarheadLocked = false;
            Log.Info("Round started, activation count reset.");
        }

        public static void SendCassieMessage(string message)
        {
            // Send the message using Cassie.Message without subtitles
            Cassie.Message(message, true, true, true);
        }

        private static async Task<string> CheckAndUpdatePlugin()
        {
            string pluginDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory", "PluginAPI", "plugins");
            string pluginPath = Path.Combine(pluginDirectory, "Anti-Camp.dll");

            string downloadUrl = "https://github.com/Josephfallen/Anti-Nuke-Camp/raw/main/Anti-Camp.dll";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Info("Checking for plugin updates...");
                    byte[] latestPluginData = await client.GetByteArrayAsync(downloadUrl);

                    if (latestPluginData != null && latestPluginData.Length > 0)
                    {
                        Log.Info("New plugin version found. Updating...");

                        if (File.Exists(pluginPath))
                        {
                            File.Copy(pluginPath, pluginPath + ".bak", true);
                        }

                        File.WriteAllBytes(pluginPath, latestPluginData);
                        Log.Info("Plugin updated successfully. Please restart the server to apply the update.");

                        return "Plugin updated successfully. Please restart the server to apply the update.";
                    }
                    else
                    {
                        Log.Info("No new updates found.");
                        return "No new updates found.";
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error updating plugin: {ex.Message}");
                    return $"Error updating plugin: {ex.Message}";
                }
            }
        }
    }

    // Command implementations
    public class ResetActivationCountCommand : ICommand
    {
        public string Command => "resetactivationcount";
        public string[] Aliases => new string[] { "rac" };
        public string Description => "Resets the activation count of the nuke";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Use the static fields directly
            if (AntiNukeCamp.IsWarheadLocked)
            {
                AntiNukeCamp.activationCount = 0;
                response = "Activation count has been reset.";
                return true;
            }

            response = "Plugin instance not found.";
            return false;
        }
    }

    public class CheckStatusCommand : ICommand
    {
        public string Command => "checkstatus";
        public string[] Aliases => new string[] { "status" };
        public string Description => "Checks the current status of the nuke";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Use the static fields directly
            response = AntiNukeCamp.IsWarheadLocked ? "The nuke is currently locked." : "The nuke is not locked.";
            return true;
        }
    }
}

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public string CommandPrefix { get; set; } = "!";

    // Implement the Debug property from IConfig
    public bool Debug { get; set; } = false;
}
