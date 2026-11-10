// Managed substitutes for the CSS boundary. Production callbacks are linked unchanged.
namespace CounterStrikeSharp.API
{
    public static class Server
    {
        public static readonly Queue<Action> Frames = new();
        public static void NextFrame(Action callback) => Frames.Enqueue(callback);
        public static void Pump() { while (Frames.TryDequeue(out var callback)) callback(); }
    }
}
namespace CounterStrikeSharp.API.Modules.Utils { public enum CsTeam { Spectator } }
namespace CounterStrikeSharp.API.Modules.Timers
{
    [Flags] public enum TimerFlags { REPEAT = 1, STOP_ON_MAPCHANGE = 2 }
    public sealed class Timer(Action callback)
    {
        public bool Killed;
        public void Kill() => Killed = true;
        public void Fire() { if (!Killed) callback(); }
    }
}
namespace CounterStrikeSharp.API.Core
{
    public class CBaseEntity { }
    public sealed class CCSPlayerController
    {
        public int Slot;
        public int? UserId = 1;
        public ulong SteamID = 100;
        public nint Handle = 123;
        public bool IsValid = true, IsBot;
        public string PlayerName = "initial";
        public int HtmlSends, TeamChanges;
        public string LastHtml = "";
        public void PrintToCenterHtml(string text) { HtmlSends++; LastHtml = text; }
        public void ChangeTeam(Modules.Utils.CsTeam team) => TeamChanges++;
    }
}
namespace Game_Manager_GoldKingZ
{
    using CounterStrikeSharp.API.Core;
    using CounterStrikeSharp.API.Modules.Timers;
    public partial class MainPlugin
    {
        public readonly CallbackLifetime Lifetime = new();
        public readonly Globals g_Main = new();
        public readonly List<Timer> Timers = new();
        public readonly FakeLocalizer Localizer = new();
        public Timer AddTimer(float interval, Action callback, TimerFlags flags)
        {
            var timer = new Timer(callback); Timers.Add(timer); return timer;
        }
    }
    public sealed class FakeLocalizer
    {
        public int Formats;
        public string this[string key, params object[] args]
        {
            get { Formats++; return key + ":" + string.Join(",", args); }
        }
    }
    public static class PlayerValidity
    {
        public static bool IsValid(this CCSPlayerController player, bool includeBots = false) => player.IsValid && (includeBots || !player.IsBot);
    }
    public partial class Helper
    {
        public static readonly List<string> Chat = new();
        public static int BlockCommands;
        public static void AdvancedPlayerPrintToChat(CCSPlayerController player, object? info, string text, params object[] args) => Chat.Add(text);
        private static void ExecuteBlockCommand(CCSPlayerController player) => BlockCommands++;
    }
}
