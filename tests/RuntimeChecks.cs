using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Game_Manager_GoldKingZ;
using Game_Manager_GoldKingZ.Config;

static class RuntimeChecks
{
    private static Globals.PlayerDataClass Add(MainPlugin plugin, int slot)
    {
        var player = new CCSPlayerController { Slot = slot, SteamID = (ulong)(100 + slot) };
        var data = new Globals.PlayerDataClass(player, null!, null!, "initial", 0, false, false,
            player.SteamID, "", 255, 0, false, null!, DateTime.MinValue, DateTime.MinValue,
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        plugin.g_Main.Player_Data.Add(slot, data);
        return data;
    }

    public static void Run(Action<bool> check)
    {
        foreach (int recipients in new[] { 0, 1, 8, 32 })
        {
            var plugin = MainPlugin.Instance = new MainPlugin();
            Configs.Instance.BlockNameChanger = 1;
            Configs.Instance.BlockNameChanger_Block = 10;
            var all = Enumerable.Range(0, 32).Select(i => Add(plugin, i)).ToArray();
            for (int i = 0; i < recipients; i++) { all[i].PlayerName_Block = true; all[i].LastNameChangeTime = DateTime.Now.AddSeconds(10); }
            for (int tick = 0; tick < 64; tick++) plugin.OnTick();
            check(all.Sum(d => d.Player.HtmlSends) == recipients * 64);
            check(plugin.Localizer.Formats == recipients);
            for (int i = 0; i < recipients; i++)
            {
                all[i].LastNameChangeTime = DateTime.Now.AddSeconds(-20);
                // Expiry is owned by the existing 2-second checker, not by OnTick.
                check(all[i].PlayerName_Block);
            }
            plugin.OnTick();
            check(all.Take(recipients).All(d => d.Player.LastHtml.EndsWith(":0")));
            Helper.Timer_Checker_Callback();
            check(plugin.g_Main.ActiveNameBlocks.Count == 0);
            int sends = all.Sum(d => d.Player.HtmlSends); plugin.OnTick(); check(sends == all.Sum(d => d.Player.HtmlSends));
        }

        foreach (int mode in new[] { 1, 2 })
        {
            var plugin = MainPlugin.Instance = new MainPlugin();
            var data = Add(plugin, 0);
            Configs.Instance.BlockNameChanger = mode; Configs.Instance.BlockNameChanger_Changes = 2;
            data.Player.PlayerName = "first"; Helper.Timer_Checker_Callback(); check(!data.PlayerName_Block);
            data.Player.PlayerName = "second"; Helper.Timer_Checker_Callback();
            check(data.PlayerName_Block && data.Player.TeamChanges == 1 && plugin.g_Main.ActiveNameBlocks.Contains(data));
            var deadline = data.LastNameChangeTime = DateTime.Now.AddSeconds(-5);
            data.Player.PlayerName = "third"; Helper.Timer_Checker_Callback();
            check(mode == 1 ? data.LastNameChangeTime > deadline : data.LastNameChangeTime == deadline);
            data.LastNameChangeTime = DateTime.Now.AddSeconds(-20);
            int commands = Helper.BlockCommands; Helper.Timer_Checker_Callback();
            check(!data.PlayerName_Block && Helper.BlockCommands == commands + (mode == 2 ? 1 : 0));
        }

        {
            var plugin = MainPlugin.Instance = new MainPlugin();
            var data = Add(plugin, 5); data.PlayerName_Block = true;
            int ran = 0;
            plugin.QueuePlayerFrame(data.Player, () => ran++);
            var timer = plugin.AddPlayerTimer(10, data.Player, () => ran++, TimerFlags.REPEAT);
            plugin.g_Main.ActiveReloadId[5] = 1;
            plugin.OnClientDisconnect(5);
            // Even identical native IDs/handle after reconnect must reject the queued callback.
            Add(plugin, 5); Server.Pump(); timer.Fire();
            check(ran == 0 && timer.Killed && plugin.Timers.Count == 0);
            check(plugin.g_Main.ActiveNameBlocks.Count == 0 && !plugin.g_Main.ActiveReloadId.ContainsKey(5));
            plugin.OnClientDisconnect(5); check(plugin.g_Main.Player_Data.Count == 0);
        }

        {
            var plugin = MainPlugin.Instance = new MainPlugin();
            var data = Add(plugin, 0); int ran = 0;
            plugin.QueueFrame(() => ran++); plugin.QueuePlayerFrame(data.Player, () => ran++);
            var timer = plugin.AddPlayerTimer(10, data.Player, () => ran++, TimerFlags.STOP_ON_MAPCHANGE);
            data.PlayerName_Block = true;
            plugin.Lifetime.Invalidate(); plugin.CancelPlayerTimers(); plugin.g_Main.Clear();
            Server.Pump(); timer.Fire();
            check(ran == 0 && timer.Killed && plugin.Timers.Count == 0 && plugin.g_Main.ActiveNameBlocks.Count == 0);
            data = Add(plugin, 0); plugin.QueuePlayerFrame(data.Player, () => ran++); Server.Pump(); check(ran == 1);
            for (int i = 0; i < 1000; i++) plugin.AddPlayerTimer(1, data.Player, () => ran++, TimerFlags.STOP_ON_MAPCHANGE).Fire();
            check(ran == 1001 && plugin.Timers.Count == 0);
            timer = plugin.AddPlayerTimer(1, data.Player, () => ran++, TimerFlags.STOP_ON_MAPCHANGE);
            timer.Fire(); plugin.CancelPlayerTimer(timer); check(!timer.Killed);
            int globalRan = 0;
            var globalTimer = plugin.AddLifetimeTimer(1, () => globalRan++, TimerFlags.STOP_ON_MAPCHANGE);
            plugin.OnClientDisconnect(data.Player.Slot); globalTimer.Fire(); check(globalRan == 1 && plugin.Timers.Count == 0);
            data = Add(plugin, 0);
            timer = plugin.AddPlayerTimer(1, data.Player, () => ran++, TimerFlags.REPEAT);
            data.Player.IsValid = false; timer.Fire(); check(timer.Killed && plugin.Timers.Count == 0);
        }
    }
    public static void Benchmark()
    {
        foreach (int recipients in new[] { 0, 1, 8, 32 })
        foreach (bool optimized in new[] { false, true })
        {
            var plugin = MainPlugin.Instance = new MainPlugin();
            Configs.Instance.BlockNameChanger = 1; Configs.Instance.BlockNameChanger_Block = 10;
            var all = Enumerable.Range(0, 32).Select(i => Add(plugin, i)).ToArray();
            for (int i = 0; i < recipients; i++) { all[i].PlayerName_Block = true; all[i].LastNameChangeTime = DateTime.Now; }
            Action tick = optimized ? plugin.OnTick : plugin.BaselineOnTick;
            for (int i = 0; i < 20000; i++) tick();
            for (int window = 0; window < 3; window++)
            {
                int sends = all.Sum(d => d.Player.HtmlSends);
                var watch = new System.Diagnostics.Stopwatch();
                long before = GC.GetAllocatedBytesForCurrentThread();
                watch.Start();
                for (int i = 0; i < 20000; i++) tick();
                watch.Stop(); long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                int delivered = all.Sum(d => d.Player.HtmlSends) - sends;
                if (delivered != 20000 * recipients) throw new Exception("Benchmark changed delivery");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { synthetic = true, recipients, connected = 32, optimized, window, frames = 20000, delivered, milliseconds = watch.Elapsed.TotalMilliseconds, allocatedBytes = allocated }));
            }
        }
    }

}
