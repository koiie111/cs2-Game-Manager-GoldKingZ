using Game_Manager_GoldKingZ;
using Game_Manager_GoldKingZ.Config;
using System.Text.Json.Nodes;
int assertions = 0;
void Check(bool condition) { assertions++; if (!condition) throw new Exception($"Assertion {assertions}"); }
foreach (var mode in new[]{"mode1", "mode2"})
{
    var cache = new CountdownText(); int formats = 0;
    string Format(string key, int seconds) { formats++; return $"{key}:{seconds}"; }
    for (int i=0; i<64; i++) Check(cache.Get(9.99-i/128.0, mode, "en", Format)==mode+":10");
    Check(formats==1);
    Check(cache.Get(9,mode,"en",Format)==mode+":9"); Check(formats==2);
    cache.Get(9,mode,"ru",Format); Check(formats==3);
    cache.Get(9,"other","ru",Format); Check(formats==4);
    Check(cache.Get(-1,mode,"en",Format)==mode+":0");
    Check(cache.Get(10,mode,"en",Format)==mode+":10"); // mode1 reblock / new deadline
}
var lifetime = new CallbackLifetime(); var old = lifetime.Capture(); Check(lifetime.IsCurrent(old)); lifetime.Invalidate(); Check(!lifetime.IsCurrent(old)); Check(lifetime.IsCurrent(lifetime.Capture()));
var root = JsonNode.Parse("""{"BlockPing":true,"HideBloodAndHsSpark":true,"HideBloodDecals":false,"Ignore_BombPlantedHUDMessages":true,"Custom_MuteSounds_1":{"Custom_MuteSounds1_SoundeventHash_Global_Side":[123,456]}}""")!.AsObject();
LegacyConfig.Upgrade(root); Check(root["BlockPing"]!.GetValue<int>()==1); Check(!root["HideBloodDecals"]!.GetValue<bool>()); Check(root["HideHeadShotSpark"]!.GetValue<bool>()); Check(root["Ignore_BombPlantedHUDMessagesAndSound"]!.GetValue<bool>()); Check(root["Custom_MuteSounds_1"]!["Custom_MuteSounds1_SoundeventHashAndString_Global_Side"]![0]!.GetValue<string>()=="123");
var once=root.ToJsonString(); LegacyConfig.Upgrade(root); Check(root.ToJsonString()==once);
var bad=JsonNode.Parse("""{"AutoSetPlayerLanguage":true}""")!.AsObject(); bool rejected=false; try { LegacyConfig.Upgrade(bad); } catch(InvalidDataException) { rejected=true; } Check(rejected);
var rules=new RuleIndex(Newtonsoft.Json.Linq.JObject.Parse("""{"Flags: @first":{"format":"one"},"any":{"format":"fallback"},"Flags: @second":{"format":"two"}}"""));
Check(rules.Groups.Length==2); Check(rules.Groups[0].Name=="Flags: @first"); Check(rules.Groups[1].Name=="Flags: @second"); Check((string?)rules.Any?["format"]=="fallback");
Check(SoundRules.Matches(new[]{"123"},"9912399")); Check(SoundRules.Matches(new[]{"WEAPON"},"weapon.fire")); Check(SoundRules.Matches(new[]{""},"anything")); Check(!SoundRules.Matches(Array.Empty<string>(),"anything")); Check(!SoundRules.Matches(new[]{"abc"},"def"));
var temp=Path.Combine(Path.GetTempPath(),"gm-config-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(temp,"config"));
try
{
    var configPath=Path.Combine(temp,"config/config.json");
    File.WriteAllText(configPath,"""{"BlockPing":true,"BlockNameChanger":1,"HideBloodAndHsSpark":true,"Ignore_BombPlantedHUDMessages":true}""");
    Configs.Load(temp); Check(Configs.Instance.BlockPing==1);Check(Configs.Instance.BlockNameChanger==1);Check(Configs.Instance.HideBloodDecals);Check(Configs.Instance.HideHeadShotSpark);Check(Configs.Instance.Ignore_BombPlantedHUDMessagesAndSound);Check(File.Exists(configPath+".pre-upgrade"));
    foreach(var invalid in new[]{"null","[]","{bad json"})
    {
        var prior = Configs.Instance; bool detached = false;
        File.WriteAllText(configPath,invalid);bool failed=false;try { Configs.Load(temp, () => detached = true); } catch(InvalidDataException) { failed=true; }Check(failed);Check(File.ReadAllText(configPath)==invalid);
        Check(!detached && ReferenceEquals(Configs.Instance, prior));
    }
    File.WriteAllText(configPath, "{\"BlockNameChanger\":2}");
    var beforeReload = Configs.Instance; bool applied = false;
    Configs.Load(temp, () => { Check(ReferenceEquals(Configs.Instance, beforeReload)); applied = true; });
    Check(applied && Configs.Instance.BlockNameChanger == 2);
}
finally { Directory.Delete(temp,true); }
var ruleDir = Path.Combine(Path.GetTempPath(), "gm-rules-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(ruleDir);
try
{
    var path = Path.Combine(ruleDir, "rules.json");
    File.WriteAllText(path, "// comment\n{\"ANY\":{\"text\":\"kept\"}}");
    var retained = RuleDocuments.Read(path); Check((string?)retained["ANY"]?["text"] == "kept");
    File.WriteAllText(path, ""); Check(RuleDocuments.Read(path, true).Count == 0);
    foreach (var invalid in new[] { "", "[]", "null", "{bad" })
    {
        File.WriteAllText(path, invalid); bool failed = false;
        try { RuleDocuments.Read(path); } catch (Newtonsoft.Json.JsonException) { failed = true; }
        Check(failed); Check(File.ReadAllText(path) == invalid); Check((string?)retained["ANY"]?["text"] == "kept");
    }
}
finally { Directory.Delete(ruleDir, true); }
Check(CommandRules.Matches("css_test arg", new[] { "css_" }, true, Array.Empty<string>(), true));
Check(CommandRules.Matches("say forbidden", Array.Empty<string>(), true, new[] { "FORBIDDEN" }, true));
Check(!CommandRules.Matches("say forbidden", Array.Empty<string>(), true, new[] { "FORBIDDEN" }, false));
Check(!CommandRules.Matches("CSS_test", new[] { "css_" }, false, Array.Empty<string>(), true));
Check(!CommandRules.Matches("anything", Array.Empty<string>(), true, Array.Empty<string>(), true));
RuntimeChecks.Run(Check);
Console.WriteLine($"PASS {assertions} assertions. Pure managed behavior only; CSS integration pending.");

if (args.Contains("--benchmark"))
{
    foreach (int recipients in new[]{0,1,8,32})
    {
        var caches = Enumerable.Range(0,recipients).Select(_=>new CountdownText()).ToArray();
        string Format(string key,int second)=>string.Format("Blocked {0}",second);
        Func<string,int,string> formatter=Format;
        foreach (bool optimized in new[]{false,true})
        {
            void Run(int frames)
            {
                for(int tick=0;tick<frames;tick++)
                    for(int slot=0;slot<recipients;slot++)
                    {
                        double left=10-(tick%640)/64.0;
                        string result=optimized ? caches[slot].Get(left,"mode1","en",formatter) : new System.Text.StringBuilder().AppendFormat(Format("mode1",(int)Math.Ceiling(left))).ToString();
                        if (result.Length==0) throw new Exception();
                    }
            }
            Run(100000);var watch=new System.Diagnostics.Stopwatch();long allocated=GC.GetAllocatedBytesForCurrentThread();watch.Start();Run(20000);watch.Stop();allocated=GC.GetAllocatedBytesForCurrentThread()-allocated;
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {synthetic=true,recipients,optimized,frames=20000,milliseconds=watch.Elapsed.TotalMilliseconds,allocatedBytes=allocated}));
        }
    }
}

if (args.Contains("--runtime-benchmark")) RuntimeChecks.Benchmark();
