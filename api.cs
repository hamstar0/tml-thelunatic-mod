using System;


namespace TheLunatic {
	public static class TheLunaticAPI {
		public static LunaticConfigData GetModSettings() {
			return TheLunaticMod.Instance.ConfigJson.Data;
		}

		public static bool HasCurrentGameEnded() {
			var modworld = TheLunaticMod.Instance.GetModWorld<TheLunaticWorld>();
			return modworld.GameLogic.HasGameEnded;
		}
	}
}
