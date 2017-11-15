using System;


namespace TheLunatic {
	public static class TheLunaticAPI {
		public static LunaticConfigData GetModSettings() {
			return TheLunaticMod.Instance.Config.Data;
		}

		public static bool HasCurrentGameEnded() {
			var modworld = TheLunaticMod.Instance.GetModWorld<MyWorld>();
			return modworld.GameLogic.HasGameEnded;
		}
	}
}
