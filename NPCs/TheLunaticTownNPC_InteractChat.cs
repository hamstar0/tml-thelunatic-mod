using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
using System;
using Terraria;
using Terraria.ModLoader;


namespace TheLunatic.NPCs {
	partial class TheLunaticTownNPC : ModNPC {
		private static void InitializeReplies() {
			TheLunaticTownNPC.NormalReplies = new string[] {
				"THE END IS NIGH! For a small tax-exempt donation, I can tell of your fortune during your last days on this planet!",
				"Have you by chance given thought to accepting into your heart our savior, Lord Cthulhu? It's a relatively painless surgical procedure!",
				"Find any interesting masks you'd be willing to let go? Not like it's important, or anything...",
				"My boss has gone nuts. All we do is rituals to summon elder gods, lately. Why couldn't we just go with my bake sale fund raiser idea?!",
				"The masks? They are the captured essense of powerful beings. Great collector's items. Also good for saving the world. Or destroying it. Same difference?",
				//"I used to be the gang's critter wrangler, until... let's not talk about that. Also, disregard any rumors of a world-ending cleanup operation. Crazy talk, I says! Heh...",
				"Heard they called in some part time fashion clerk to fill my vacancy. Now that the dungeon's main occupants have esc...relocated, thought it time to move on...",
				"Ph'nglui mglw'nafh ... I forget the rest. Sorry, my R'leyhian is a bit rusty. Just brushing up. No reason..."
			};
			TheLunaticTownNPC.DismissalReplies = new string[] {
				//"You'll pay!",
				//"*gargling noises*",
				//"Infidel!",
				"..."
			};
		}



		////////////////
		
		public override string GetChat() {
			try {
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();
				if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }

				Player player = Main.player[Main.myPlayer];
				var myplayer = player.GetModPlayer<TheLunaticPlayer>();

				if( myplayer.IsCheater() ) {
					return TheLunaticTownNPC.DismissalReplies[ Main.rand.Next(TheLunaticTownNPC.DismissalReplies.Length) ];
				}
				
				string msg;
				int daysLeft = mymod.Config.DaysUntil - (myworld.GameLogic.HalfDaysElapsed / 2);
				int rand = Main.rand.Next( Math.Max((daysLeft / 2), 2) );	// Closer to apocalypose, less chit chat

				if( myworld.GameLogic.HasWon || rand != 0 ) {
					msg = TheLunaticTownNPC.NormalReplies[ Main.rand.Next(TheLunaticTownNPC.NormalReplies.Length) ];
				} else {
					if( myworld.GameLogic.IsApocalypse ) {
						msg = "Party time! Masks won't do much good, now. Alas, they were our only hope...";
					} else if( TheLunaticTownNPC.AlertedToImpendingDoom ) {
						if( daysLeft <= 3 && Main.rand.Next(3) == 0 ) {
							msg = "I enjoy a good party and all, but this one'll be killer if we don't get underground soon, at this rate. Literally.";
						} else {
							if( daysLeft > 1 ) {
								msg = "So it's begun. I estimate we've got only a few days until party time.\n...on a totally unrelated note, got any masks?";
							} else {
								msg = "Those tremors... the ceremony must be near completion. That makes this the final day.\nIf you have any masks for me, it's now or never!";
							}
						}
					} else {
						msg = "There ought to be signs. There's always signs within the last few days. Seismic activity, unusual celestial phenomena... Noticed any, yet?";
					}
				}
				
				return msg;
			} catch( Exception e ) {
				LogHelpers.Log( e.ToString() );
				throw e;
			}
		}
	}
}