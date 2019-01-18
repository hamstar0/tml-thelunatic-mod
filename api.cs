using System;


namespace TheLunatic {
	public static class TheLunaticAPI {
		public static LunaticConfigData GetModSettings() {
			return TheLunaticMod.Instance.ConfigJson.Data;
		}

		public static bool HasCurrentGameEnded() {
			var myworld = TheLunaticMod.Instance.GetModWorld<TheLunaticWorld>();
			return myworld.GameLogic.HasGameEnded;
		}
	}
}
