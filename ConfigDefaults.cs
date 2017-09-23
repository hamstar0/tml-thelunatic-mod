using HamstarHelpers.Utilities.Config;
using System;
using Terraria.ModLoader;


namespace TheLunatic {
	public class LunaticConfigData : ConfigurationDataBase {
		public readonly static Version CurrentVersion = new Version( 1, 2, 6 );


		public static LunaticConfigData GetCurrent() {
			var mymod = (TheLunaticMod)ModLoader.GetMod( "TheLunatic" );
			return mymod.Config.Data;
		}


		////////////////

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
			var new_config = new LunaticConfigData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= LunaticConfigData.CurrentVersion ) {
				return false;
			}

			if( vers_since < new Version( 1, 2, 2 ) ) {
				if( this.DaysUntil == LunaticConfigData._1_2_1_DaysUntil ) {
					this.DaysUntil = new_config.DaysUntil;
				}
				if( this.HardModeMultiplier == LunaticConfigData._1_2_1_HardModeMultiplier ) {
					this.HardModeMultiplier = new_config.HardModeMultiplier;
				}
				if( this.HalfDaysRecoveredPerMask == LunaticConfigData._1_2_1_HalfDaysRecoveredPerMask ) {
					this.HalfDaysRecoveredPerMask = new_config.HalfDaysRecoveredPerMask;
				}
			}

			this.VersionSinceUpdate = LunaticConfigData.CurrentVersion.ToString();

			return true;
		}


		////////////////

		public string _OLD_SETTINGS_BELOW = "";

		public readonly static int _1_2_1_DaysUntil = 9;
		public readonly static float _1_2_1_HardModeMultiplier = 1.5f;
		public readonly static int _1_2_1_HalfDaysRecoveredPerMask = 4;
	}
}
