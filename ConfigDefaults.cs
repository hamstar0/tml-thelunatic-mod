using System;


namespace TheLunatic {
	public class ConfigurationData {
		public readonly static Version CurrentVersion = new Version( 1, 2, 6 );


		public string VersionSinceUpdate = "";

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

		public int DEBUGFLAGS = 0;  // 1: Display info, 2: Fast time, 4: Reset, 8: Reset win, 16: Skip to signs, 32: Display net info



		////////////////

		public bool UpdateToLatestVersion() {
			var new_config = new ConfigurationData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= ConfigurationData.CurrentVersion ) {
				return false;
			}

			if( vers_since < new Version( 1, 2, 2 ) ) {
				if( this.DaysUntil == ConfigurationData._1_2_1_DaysUntil ) {
					this.DaysUntil = new_config.DaysUntil;
				}
				if( this.HardModeMultiplier == ConfigurationData._1_2_1_HardModeMultiplier ) {
					this.HardModeMultiplier = new_config.HardModeMultiplier;
				}
				if( this.HalfDaysRecoveredPerMask == ConfigurationData._1_2_1_HalfDaysRecoveredPerMask ) {
					this.HalfDaysRecoveredPerMask = new_config.HalfDaysRecoveredPerMask;
				}
			}

			this.VersionSinceUpdate = ConfigurationData.CurrentVersion.ToString();

			return true;
		}


		////////////////

		public string _OLD_SETTINGS_BELOW = "";

		public readonly static int _1_2_1_DaysUntil = 9;
		public readonly static float _1_2_1_HardModeMultiplier = 1.5f;
		public readonly static int _1_2_1_HalfDaysRecoveredPerMask = 4;
	}
}
