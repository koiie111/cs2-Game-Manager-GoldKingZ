namespace Game_Manager_GoldKingZ;
// Config file tests substitute only logging/resource creation; production parsing is linked unchanged.
public partial class MainPlugin { public static MainPlugin Instance = new(); public string ModuleVersion => "test"; }
public partial class Helper { public static void DebugMessage(string message, int config=0) {} }
public static class JsonCreator { public static void StartCreate() {} }
