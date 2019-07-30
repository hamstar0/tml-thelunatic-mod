using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;


namespace TheLunatic {
	public class LunaticConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;


		////

		public bool DebugModeInfo = false;

		public bool DebugModeNetInfo = false;

		public bool DebugModeFastTime = false;

		public bool DebugModeReset = false;

		public bool DebugModeResetWin = false;

		public bool DebugModeSkipToSigns = false;


		[DefaultValue( true )]
		public bool Enabled = true;


		[DefaultValue( 9 )]
		public int DaysUntil = 9;  // Days until The End

		[DefaultValue( 4 )]
		public int HalfDaysRecoveredPerMask = 4;    // Half days recovered per mask

		[DefaultValue( 2.5f )]
		public float WallOfFleshMultiplier = 2.5f;    // Added time for WoF kill

		[DefaultValue( 1.5f )]
		public float HardModeMultiplier = 1.5f; // Added time for hard mode bosses


		[DefaultValue( true )]
		public bool LoonyEnforcesBossSequence = true;

		public bool LoonyAcceptsMasksWithoutBossKill = false;

		[DefaultValue( true )]
		public bool LoonySellsSummonItems = true;

		public bool LoonyShunsCheaters = false;

		[DefaultValue( true )]
		public bool LoonyGivesCompletionReward = true;

		[DefaultValue( true )]
		public bool LoonyIndicatesDaysRemaining = true;


		public bool OnlyVanillaBossesDropMasks = false;

		public bool MoonLordMaskWins = false;
	}
}
