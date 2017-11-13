using System;


namespace TheLunatic {
	public static class TheLunaticApi {
		public static LunaticConfigData GetModSettings() {
			if( TheLunaticMod.Instance == null ) {
				throw new Exception( "Lunatic mod not loaded." );
			}
			return TheLunaticMod.Instance.Config.Data;
		}

		public static bool HasCurrentGameEnded() {
			if( TheLunaticMod.Instance == null ) {
				throw new Exception("Lunatic mod not loaded.");
			}
			var modworld = TheLunaticMod.Instance.GetModWorld<MyWorld>();
			return modworld.GameLogic.HasGameEnded;
		}
	}
}
