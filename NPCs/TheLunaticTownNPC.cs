using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;
using Utils;


namespace TheLunatic.NPCs {
	public class TheLunaticTownNPC : ModNPC {
		public static string[] PossibleNames { get; private set; }
		public static string[] DismissalReplies { get; private set; }
		public static string[] NormalReplies { get; private set; }

		public static bool AlertedToImpendingDoom = false;
		public static bool AmHere { get; private set; }


		static TheLunaticTownNPC() {
			TheLunaticTownNPC.AmHere = false;

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
			TheLunaticTownNPC.PossibleNames = new string[] {
				"Whateley",
				"Dunwich",
				"Waite",
				"Olmstead",
				"Marsh",
				"Jenkin",
				"Jermyn",
				"Mason",
				"Alhazred",
				"Castro"

				/*"Bill",
				"Charlie",
				"Dennis",
				"Fred",
				"Greg",
				"Jerry",
				"Larry",
				"Phil",
				"Rob", 
				"Steve",
				"Tom",
				"Vexyloctimus the 42nd"*/

				/*"#24601",
				"#90210",
				"#12345",
				"#4815162342",
				"#32",
				"#4583.5",
				"#9944100",
				"#8675309",
				"#9001",
				"Bob"*/
			};
		}

		public static bool WantsToSpawn( TheLunaticMod mymod ) {
			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception("Game logic not initialized."); }

			bool can_spawn = !modworld.GameLogic.HasLoonyQuit;

			if( can_spawn && mymod.Config.Data.LoonyEnforcesBossSequence ) {
				// Allow spawning if moon lord gone (shop use only)
				if( !NPC.downedMoonlord ) {
					//if( NPC.downedSlimeKing ) { can_spawn = false; }		// King Slime
					if( NPC.downedQueenBee ) { can_spawn = false; }         // Queen Bee
					if( NPC.downedBoss1 ) { can_spawn = false; }			// Eye of Cthulhu
					if( NPC.downedBoss2 ) { can_spawn = false; }            // Brain of Cthulhu && Eater of Worlds
					if( NPC.downedBoss3 ) { can_spawn = false; }            // Skeletron
					if( Main.hardMode ) { can_spawn = false; }              // Wall of Flesh
					if( NPC.downedMechBoss1 ) { can_spawn = false; }        // Destroyer
					if( NPC.downedMechBoss2 ) { can_spawn = false; }        // Twin
					if( NPC.downedMechBoss3 ) { can_spawn = false; }        // Skeletron Prime
					if( NPC.downedFishron ) { can_spawn = false; }          // Duke Fishron
					if( NPC.downedPlantBoss ) { can_spawn = false; }        // Plantera
					if( NPC.downedGolemBoss ) { can_spawn = false; }        // Golem
					if( NPC.downedAncientCultist ) { can_spawn = false; }   // Ancient Cultist
				}
			}

			return can_spawn;
		}


		////////////////
		
		public override bool Autoload( ref string name, ref string texture, ref string[] altTextures ) {
			name = "The Lunatic";
			return mod.Properties.Autoload;
		}

		public override void SetDefaults() {
			int npc_type = this.npc.type;

			this.npc.name = "The Lunatic";
			this.npc.townNPC = true;
			this.npc.friendly = true;
			this.npc.width = 18;
			this.npc.height = 40;
			this.npc.aiStyle = 7;
			this.npc.damage = 10;
			this.npc.defense = 15;
			this.npc.lifeMax = 250;
			this.npc.HitSound = SoundID.NPCHit1;
			this.npc.DeathSound = SoundID.NPCDeath1;
			this.npc.knockBackResist = 0.5f;

			Main.npcFrameCount[npc_type] = 26;
			NPCID.Sets.AttackFrameCount[npc_type] = 5;
			NPCID.Sets.DangerDetectRange[npc_type] = 700;
			NPCID.Sets.AttackType[npc_type] = 1;
			NPCID.Sets.AttackTime[npc_type] = 30;
			NPCID.Sets.AttackAverageChance[npc_type] = 30;
			NPCID.Sets.HatOffsetY[npc_type] = 4;
			this.animationType = NPCID.Guide;
		}

		public override bool CanTownNPCSpawn( int numTownNPCs, int money ) {
			var modworld = mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception("Game logic not initialized."); }

			if( modworld.GameLogic.KillSurfaceTownNPCs ) { return false; }
			return TheLunaticTownNPC.WantsToSpawn( (TheLunaticMod)this.mod );
		}

		public override void TownNPCAttackStrength( ref int damage, ref float knockback ) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown( ref int cooldown, ref int randExtraCooldown ) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj( ref int projType, ref int attackDelay ) {
			projType = 1;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed( ref float multiplier, ref float gravityCorrection, ref float randomOffset ) {
			multiplier = 12f;
			randomOffset = 2f;
		}

		public override string TownNPCName() {
			return TheLunaticTownNPC.PossibleNames[ Main.rand.Next(TheLunaticTownNPC.PossibleNames.Length) ];
			//return "The Lunatic";
		}

		public override bool UsesPartyHat() {
			return false;	// :(
		}

		public override void AI() {
			var modworld = mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			TheLunaticTownNPC.AmHere = true;

			if( modworld.GameLogic.HaveWeEndSigns() ) {
				TheLunaticTownNPC.AlertedToImpendingDoom = true;
			}
		}

		////////////////

		public override void SetupShop( Chest shop, ref int nextSlot ) {
			var mymod = (TheLunaticMod)this.mod;
			bool strict = mymod.Config.Data.LoonyEnforcesBossSequence;
			bool downed_mech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
			bool downed_towers = NPC.downedTowerSolar && NPC.downedTowerVortex && NPC.downedTowerNebula && NPC.downedTowerStardust;

			// Bake sale!
			Item sugar_cookie = new Item();
			Item gingerbread_cookie = new Item();
			Item christmas_pudding = new Item();
			Item pumpkin_pie = new Item();
			Item cooked_marshmallow = new Item();

			sugar_cookie.SetDefaults( 1919 );
			gingerbread_cookie.SetDefaults( 1920 );
			christmas_pudding.SetDefaults( 1911 );
			pumpkin_pie.SetDefaults( 1787 );
			cooked_marshmallow.SetDefaults( 969 );

			sugar_cookie.value *= 8;
			gingerbread_cookie.value *= 8;
			christmas_pudding.value *= 9;
			pumpkin_pie.value *= 9;
			cooked_marshmallow.value *= 5;

			shop.item[nextSlot++] = sugar_cookie;
			shop.item[nextSlot++] = gingerbread_cookie;
			shop.item[nextSlot++] = christmas_pudding;
			shop.item[nextSlot++] = pumpkin_pie;
			shop.item[nextSlot++] = cooked_marshmallow;

			for( int i=0; i<15; i++ ) {
				shop.item[nextSlot++] = new Item();
			}

			// Boss summon items
			if( mymod.Config.Data.LoonySellsSummonItems ) {
				// Eye of Cthulhu
				if( NPC.downedBoss1 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 43 );		// Suspicious Looking Eye
					summon_item.value = 150000;
					shop.item[nextSlot++] = summon_item;
				}
				// King Slime
				if( NPC.downedSlimeKing ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 560 );    // Slime Crown
					summon_item.value = 180000;
					shop.item[nextSlot++] = summon_item;
				}
				// Queen Bee
				if( NPC.downedQueenBee ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1133 );    // Abeemination
					summon_item.value = 200000;
					shop.item[nextSlot++] = summon_item;
				}
				// Brain of Cthulhu
				if( NPC.downedBoss2 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1331 );	// Bloody Spine
					summon_item.value = 260000;
					shop.item[nextSlot++] = summon_item;
				}
				// Eater of Worlds
				if( NPC.downedBoss2 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 70 );    // Worm Food
					summon_item.value = 250000;
					shop.item[nextSlot++] = summon_item;
				}
				// Skeletron
				if( NPC.downedBoss3 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1307 );    // Clothier Voodoo Doll
					summon_item.value = 300000;
					shop.item[nextSlot++] = summon_item;
				}
				// Wall of Flesh
				if( Main.hardMode ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 267 );    // Guide Voodoo Doll
					summon_item.value = 320000;
					shop.item[nextSlot++] = summon_item;
				}
				// Destroyer
				if( (!strict || (Main.hardMode)) && NPC.downedMechBoss1 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 556 );    // Mechanical Worm
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Twins
				if( (!strict || (Main.hardMode)) && NPC.downedMechBoss2 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 544 );    // Mechanical Worm
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Skeletron Prime
				if( (!strict ||(Main.hardMode)) && NPC.downedMechBoss3 ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 557 );    // Mechanical Skull
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Golem
				if( (!strict || (Main.hardMode && downed_mech && NPC.downedPlantBoss)) && NPC.downedGolemBoss ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1293 );    // Lihzahrd Power Cell
					summon_item.value = 2000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Duke Fishron
				if( (!strict || (Main.hardMode)) && NPC.downedFishron ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 2673 );    // Truffle Worm
					summon_item.value = 3500000;
					shop.item[nextSlot++] = summon_item;
				}
				// Moon Lord
				if( (!strict || (Main.hardMode && downed_mech && NPC.downedPlantBoss && NPC.downedGolemBoss &&
						NPC.downedAncientCultist)) && NPC.downedMoonlord ) {    //&& downed_towers
					Item summon_item = new Item();
					summon_item.SetDefaults( 3601 );    // Celestial Sigil
					summon_item.value = 8000000;
					shop.item[nextSlot++] = summon_item;
				}
			}
		}

		////////////////


		public override string GetChat() {
			try {
				var mymod = (TheLunaticMod)this.mod;
				var modworld = mymod.GetModWorld<TheLunaticWorld>();
				if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

				Player player = Main.player[Main.myPlayer];
				var modplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );

				if( modplayer.IsCheater() ) {
					return TheLunaticTownNPC.DismissalReplies[ Main.rand.Next(TheLunaticTownNPC.DismissalReplies.Length) ];
				}
				
				string msg;
				int days_left = mymod.Config.Data.DaysUntil - (modworld.GameLogic.HalfDaysElapsed / 2);
				int rand = Main.rand.Next( Math.Max((days_left / 2), 2) );	// Closer to apocalypose, less chit chat

				if( modworld.GameLogic.HasWon || rand != 0 ) {
					msg = TheLunaticTownNPC.NormalReplies[ Main.rand.Next(TheLunaticTownNPC.NormalReplies.Length) ];
				} else {
					if( modworld.GameLogic.IsApocalypse ) {
						msg = "Party time! Masks won't do much good, now. Alas, they were our only hope...";
					} else if( TheLunaticTownNPC.AlertedToImpendingDoom ) {
						if( days_left <= 3 && Main.rand.Next(3) == 0 ) {
							msg = "I enjoy a good party and all, but this one'll be killer if we don't get underground soon, at this rate. Literally.";
						} else {
							if( days_left > 1 ) {
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
				DebugHelper.Log( e.ToString() );
				throw e;
			}
		}

		////////////////

		private bool FirstButtonIsShop = false;

		public override void SetChatButtons( ref string button1, ref string button2 ) {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var modplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
			
			if( !modplayer.IsCheater() && modworld.GameLogic.HaveWeHopeToWin() ) {
				this.FirstButtonIsShop = false;
				button1 = "Give boss mask";
				button2 = "Shop";
			} else {
				this.FirstButtonIsShop = true;
				button1 = "Shop";
			}
		}


		public override void OnChatButtonClicked( bool first_button, ref bool shop ) {
			if( first_button ) {
				if( this.FirstButtonIsShop ) {
					shop = true;
				} else {
					Main.npcChatText = this.onGiveMaskButtonClick();
				}
			} else {
				shop = true;
			}
		}

		////////////////

		private string onGiveMaskButtonClick() {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var remaining_masks = modworld.MaskLogic.GetRemainingVanillaMasks();
			if( remaining_masks.Count == 0 ) {
				return "I need no more masks.";
			}

			bool is_custom = false;
			bool is_given = false;
			Item mask = PlayerHelper.FindFirstOfItemFor( player, remaining_masks );
			if( mask == null ) {
				mask = PlayerHelper.FindFirstOfItemFor( player, new HashSet<int> { mymod.ItemType<CustomBossMaskItem>() } );
				is_custom = mask != null;
				is_given = is_custom && modworld.MaskLogic.DoesLoonyHaveThisMask( mask );
			}

			if( mask == null || is_given ) {
				if( mask == null ) {
					mask = PlayerHelper.FindFirstOfItemFor( player, MaskLogic.AllVanillaMasks );
				}
				string msg, hint = this.GetHint();

				if( mask == null ) {
					msg = "Ehehe, good one! But seriously, you DO have a mask... don't you? ...please?\n" + hint;
				} else {
					msg = "Very nice, but I've already got a " + MaskLogic.GetMaskDisplayName( mask ) + ".\n" + hint;
				}

				if( !modworld.GameLogic.HasGameEnded && mymod.Config.Data.LoonyIndicatesDaysRemaining ) {
					int days_left = mymod.Config.Data.DaysUntil - (modworld.GameLogic.HalfDaysElapsed / 2);
					msg += "\n \nDays remaining: " + days_left;
				}
				return msg;
			}

			if( modworld.MaskLogic.IsValidMask( mask ) ) {
				int remaining = remaining_masks.Count() - (is_custom?0:1);
				string status, msg;

				if( remaining >= 2 ) {
					status = "I require " + remaining + " more masks.";
				} else if( remaining == 1 ) {
					status = "I require one final mask.";
				} else {
					if( mymod.Config.Data.LoonyGivesCompletionReward ) {
						status = "At last! Seems this world gets to live a little bit longer. I won't need this anymore. Enjoy!";
						UmbralCowlItem.Give( this.mod, player );
					} else {
						status = "At last! Seems this world gets to live a little bit longer. Be sure to celebrate by buying some of my delicious treats!";
					}
				}

				switch( mask.type ) {
				case 2112: // Eye of Cthulhu Mask
					msg = "A genuine Eye of Cthulhu mask. Much thanks!\nThat wasn't really Cthulhu's eye, by the way. We tried getting a real one, but it saw right through us. Right THROUGH us.";
					break;
				case 2493: // King Slime Mask
					msg = "A genuine King Slime mask. Much thanks!\nA bit sticky to the touch, though.";
					break;
				case 2108: // Queen Bee Mask
					msg = "A genuine Queen Bee mask. Much thanks!\nI'll take it. Not one of ours, but having it gone helps us cut back on the bug spray.";
					break;
				case 2104: // Brain of Cthulhu Mask
					msg = "A genuine Brain of Cthulhu mask. Much thanks!\nDon't be confused: It's a brain OF Cthulhu. Not Cthulhu's brain. Dang newbies.";
					break;
				case 2111: // Eater of Worlds Mask
					msg = "A genuine Eater of Worlds mask. Much thanks!\nSuch an ominous name. It really only nibbles them a bit...";
					break;
				case 1281: // Skeletron Mask
					msg = "A genuine Skeletron mask. Much thanks!\nYou don't even want to know what it took to stuff that thing inside the old geezer. Took it like a champ, he did.";
					break;
				case 2105: // Wall of Flesh Mask
					msg = "A genuine Wall of Flesh mask. Much thanks!\nAh, you cleared up that fleshy blockage yourself. I wonder what happened to the plumber we called...";
					break;
				case 2113: // Destroyer Mask
					msg = "A genuine Destroyer Mask mask. Much thanks!\nDo you have any idea how much assembly these req... er, thanks for the mask!";
					break;
				case 2106: // Twin Mask
					msg = "A genuine Twins mask. Much thanks!\nI wonder what became of the larger thing these were supposed to be part of...";
					break;
				case 2107: // Skeletron Prime Mask
					msg = "A genuine Skeletron Prime mask. Much thanks!\nA favorite for terrorizing small children! Uh, I mean the mask, of course! That's right!";
					break;
				case 2109: // Plantera Mask
					msg = "A genuine Plantera mask. Much thanks!\nI have a strange urge to listen to heavy metal for some reason...";
					break;
				case 2110: // Golem Mask
					msg = "A genuine Golem mask. Much thanks!\nServes those damn reptiles right for going rogue.";
					break;
				case 2588: // Duke Fishron Mask
					//msg = "A genuine Duke Fishron mask. Much thanks!\nThese are quite the delicacy. A heck of a fishing trip to catch, though.";
					msg = "A genuine Duke Fishron mask. Much thanks!\nHere's what you get for breeding a shark, a pig, and a dragon. Just a little leftover from someone's previous party.";
					break;
				case 3372: // Ancient Cultist Mask
					msg = "A genuine Ancient Cultist mask. Much thanks!\nFairwell, boss. The summoning ritual is now halted. Seems we're still getting a party of sorts, though...";
					break;
				case 3373: // Moon Lord Mask
					msg = "PRAISE BE TO YOG-er, thank you very much!";
					break;
				default:
					string mask_name = MaskLogic.GetMaskDisplayName( mask );
					msg = "A genuine "+mask_name+ ". Much thanks!\nA bit non-standard, but it should work...";
					break;
				}

				modworld.MaskLogic.GiveMaskToLoony( player, mask );

				return msg + "\n\n" + status;
			}
			else {
				if( (DebugHelper.DEBUGMODE & 1) > 0 ) { DebugHelper.Log("DEBUG cheater detected. "+ mask.name); }

				if( mymod.Config.Data.LoonyShunsCheaters ) {
					player.GetModPlayer<TheLunaticPlayer>( this.mod ).SetCheater();
					return "You... you aren't supposed to even have this. Bye.";
				} else {
					return "I don't know how you got this, but I'm not ready for it yet.";
				}
			}
		}


		public string GetHint() {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }
			var masks = modworld.MaskLogic.GetRemainingVanillaMasks();
			string msg;

			if( masks.Contains( 2112 ) ) { // Eye of Cthulhu Mask
				msg = "Where to find a mask? Keep an eye out for a big eye. No really!";
			} else if( masks.Contains( 2493 ) ) { // King Slime Mask
				msg = "You could try persuading a giant slime to give you one.";
			} else if( masks.Contains( 2104 ) ) { // Brain of Cthulhu Mask
				msg = "I'll bet a certain big-brained abomination has one.";
			} else if( masks.Contains( 2111 ) ) { // Eater of Worlds Mask
				msg = "I'll bet a certain giant worm's got the goods.";
			} else if( masks.Contains( 2108 ) ) { // Queen Bee Mask
				msg = "Try asking bee momma politely.";
			} else if( masks.Contains( 1281 ) ) { // Skeletron Mask
				msg = "Might have to take one from our dungeon guard dog.";
			} else if( masks.Contains( 2105 ) ) { // Wall of Flesh Mask
				msg = "That eldritch horror of the underworld's likely your next stop.";
			} else if( masks.Contains( 2113 ) ) { // Destroyer Mask
				msg = "I expect the giant toy worm's got one.";
			} else if( masks.Contains( 2106 ) ) { // Twin Mask
				msg = "I reckon the eyeball bros got one.";
			} else if( masks.Contains( 2107 ) ) { // Skeletron Prime Mask
				msg = "That machine playing copycat to our guard dog must have one.";
			} else if( masks.Contains( 2109 ) ) { // Plantera Mask
				msg = "It seems some horticulturalist's pet might give you your next mask.";
			} else if( masks.Contains( 2110 ) ) { // Golem Mask
				msg = "Dang lizards. Check with their new punching toy.";
			} else if( masks.Contains( 2588 ) ) { // Duke Fishron Mask
				msg = "The fish lord for sure has the next.";
			} else if( masks.Contains( 3863 ) ) { // Betsy Mask
				msg = "Buncha foreigners and their pet flying lizard brought one.";
			} else if( masks.Contains( 3372 ) ) { // Ancient Cultist Mask
				msg = "*sigh* Gonna have to get the next one from my boss. You didn't hear this from me.";
			} else if( masks.Contains( 3373 ) ) { // Moon Lord Mask
				msg = "You'll have to wrest the last one from Cthulhu's kin. Good luck!";
			} else {
				msg = "No additional masks required.";
			}

			return msg;
		}
	}
}