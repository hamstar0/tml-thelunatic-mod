using Terraria;

namespace Utils {
	public static class NPCHelper {
		public static string GetUniqueId( NPC npc ) {
			if( npc.modNPC != null ) {
				return npc.modNPC.mod.Name + " " + Main.npcName[npc.type];
			}
			return ""+npc.type;
		}

		public static int FindNpcTypeByUniqueId( string uid ) {
			NPC npc = new NPC();
			for( int i = Main.npcName.Length - 1; i >= 0; i-- ) {
				npc.SetDefaults( i );
				if( NPCHelper.GetUniqueId( npc ) == uid ) {
					return i;
				}
			}
			return -1;
		}


		public static bool IsNPCDead( NPC check_npc ) {
			return check_npc.life <= 0 || !check_npc.active;
		}


		public static void Kill( NPC npc ) {
			npc.life = 0;
			npc.checkDead();
			npc.active = false;
			NetMessage.SendData( 28, -1, -1, "", npc.whoAmI, -1f, 0f, 0f, 0, 0, 0 );
		}

		public static void Leave( NPC npc, bool announce=true ) {
			int whoami = npc.whoAmI;
			if( announce ) {
				string str = Main.npc[whoami].name + " the " + Main.npc[whoami].name;
				
				if( Main.netMode == 0 ) {
					Main.NewText( str + " " + Lang.misc[35], 50, 125, 255, false );
				} else if( Main.netMode == 2 ) {
					NetMessage.SendData( 25, -1, -1, str + " " + Lang.misc[35], 255, 50f, 125f, 255f, 0, 0, 0 );
				}
			}
			Main.npc[whoami].active = false;
			Main.npc[whoami].netSkip = -1;
			Main.npc[whoami].life = 0;
			NetMessage.SendData( 23, -1, -1, "", whoami, 0f, 0f, 0f, 0, 0, 0 );
		}
	}
}
