﻿using Terraria;


namespace Utils {
	public static class MiscHelper {
		public static bool IsBeingInvaded() {
			return Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0;
		}


		public static double GetDayFractional() {
			if( Main.dayTime ) {
				return Main.time / 54000.0;
			} else {
				return Main.time / 32400.0;
			}
		}


		public static string RenderMoney( int money ) {
			string render = "";
			string label_copper = Lang.inter[18];
			string label_silver = Lang.inter[17];
			string label_gold = Lang.inter[16];
			string label_plat = Lang.inter[15];

			int plat = 0;
			int gold = 0;
			int silver = 0;
			int copper = 0;

			if( money < 0 ) { money = 0; }

			if( money >= 1000000 ) {
				plat = money / 1000000;
				money -= plat * 1000000;
			}
			if( money >= 10000 ) {
				gold = money / 10000;
				money -= gold * 10000;
			}
			if( money >= 100 ) {
				silver = money / 100;
				money -= silver * 100;
			}
			if( money >= 1 ) {
				copper = money;
			}

			if( plat > 0 ) { render += plat + " " + label_plat; }
			if( render.Length > 0 ) { render += " "; }
			if( gold > 0 ) { render += gold + " " + label_gold; }
			if( render.Length > 0 ) { render += " "; }
			if( silver > 0 ) { render += silver + " " + label_silver; }
			if( render.Length > 0 ) { render += " "; }
			if( copper > 0 ) { render += copper + " " + label_copper; }

			return render;
		}
	}
}
