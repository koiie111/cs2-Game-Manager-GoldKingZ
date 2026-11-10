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
Console.WriteLine($"PASS {assertions} assertions. Pure managed behavior only; CSS integration pending.");
