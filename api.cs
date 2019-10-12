using System;
using Terraria.ModLoader;


namespace TheLunatic {
	public static class TheLunaticAPI {
		public static bool HasCurrentGameEnded() {
			var myworld = ModContent.GetInstance<TheLunaticWorld>();
			return myworld.GameLogic.HasGameEnded;
		}
	}
}
