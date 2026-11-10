// Exact OnTick from f7ff9e9; method name changed to run both paths in one managed fixture.
using System.Text;
using Game_Manager_GoldKingZ.Config;
namespace Game_Manager_GoldKingZ;
public partial class MainPlugin
{
    public void BaselineOnTick()
    {
        foreach(var getplayer in g_Main.Player_Data.Values)
        {
            if (getplayer == null) continue;

            var player = getplayer.Player;
            if (!player.IsValid() || !getplayer.PlayerName_Block) continue;

            var timeSinceLastChange = (DateTime.Now - getplayer.LastNameChangeTime).TotalSeconds;
            var totalBlock = Configs.Instance.BlockNameChanger_Block;
            var timeLeft = totalBlock - timeSinceLastChange;

            if (timeLeft < 0)
            {
                timeLeft = 0;
            }

            StringBuilder builder = new StringBuilder();
            var getLocalizer = Configs.Instance.BlockNameChanger == 1 ? "PrintToCenterToPlayer.NameChanging.Mode1.Blocked" : "PrintToCenterToPlayer.NameChanging.Mode2.Blocked";
            builder.AppendFormat(Localizer[getLocalizer, (int)Math.Ceiling(timeLeft)]);
            var centerhtml = builder.ToString();
            player.PrintToCenterHtml(centerhtml);
        }
    }
    
}
