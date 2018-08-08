using HamstarHelpers.Components.Config;
using System;


namespace TheLunatic {
	public class LunaticConfigData : ConfigurationDataBase {
		public readonly static Version ConfigVersion = new Version( 1, 3, 1 );
		public readonly static string ConfigFileName = "The Lunatic Config.json";
		
		
		////////////////

		public string VersionSinceUpdate = "";

		public bool DebugModeInfo = false;
		public bool DebugModeNetInfo = false;
		public bool DebugModeFastTime = false;
		public bool DebugModeReset = false;
		public bool DebugModeResetWin = false;
		public bool DebugModeSkipToSigns = false;

		public bool Enabled = true;

		public int DaysUntil = 9;  // Days until The End
		public int HalfDaysRecoveredPerMask = 4;    // Half days recovered per mask
		public float WallOfFleshMultiplier = 2.5f;    // Added time for WoF kill
		public float HardModeMultiplier = 1.5f; // Added time for hard mode bosses

		public bool LoonyEnforcesBossSequence = true;
		public bool LoonyAcceptsMasksWithoutBossKill = false;
		public bool LoonySellsSummonItems = true;
		public bool LoonyShunsCheaters = false;
		public bool LoonyGivesCompletionReward = true;
		public bool LoonyIndicatesDaysRemaining = true;

		public bool OnlyVanillaBossesDropMasks = false;
		public bool MoonLordMaskWins = false;



		////////////////

		public bool UpdateToLatestVersion() {
			var new_config = new LunaticConfigData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= LunaticConfigData.ConfigVersion ) {
				return false;
			}

			this.VersionSinceUpdate = LunaticConfigData.ConfigVersion.ToString();

			return true;
		}
	}
}
