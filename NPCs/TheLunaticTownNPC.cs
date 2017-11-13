using HamstarHelpers.DebugHelpers;
using HamstarHelpers.PlayerHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;


namespace TheLunatic.NPCs {
	[AutoloadHead]
	class TheLunaticTownNPC : ModNPC {
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
			};
		}

		public static bool WantsToSpawnAnew( TheLunaticMod mymod ) {
			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			bool can_spawn = !modworld.GameLogic.HasLoonyQuit;

			if( can_spawn && mymod.Config.Data.LoonyEnforcesBossSequence ) {
				// Allow spawning if moon lord gone (shop use only)
				if( !NPC.downedMoonlord ) {
					//if( NPC.downedSlimeKing ) { can_spawn = false; }		// King Slime
					if( NPC.downedBoss1 ) { can_spawn = false; }            // Eye of Cthulhu
					if( NPC.downedQueenBee ) { can_spawn = false; }         // Queen Bee
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

		public static bool WantsToSpawn( TheLunaticMod mymod ) {
			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception("Game logic not initialized."); }

			bool can_spawn = !modworld.GameLogic.HasLoonyQuit;

			if( mymod.Config.Data.LoonyEnforcesBossSequence ) {
				if( can_spawn && !Main.hardMode ) {
					can_spawn = !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && !NPC.downedFishron
						&& !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( can_spawn && !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 ) {
					can_spawn = !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( can_spawn && !NPC.downedPlantBoss ) {
					can_spawn = !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( can_spawn && !NPC.downedGolemBoss ) {
					can_spawn = !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( can_spawn && !NPC.downedAncientCultist ) {
					can_spawn = !NPC.downedMoonlord;
				}
			}
			
//if( _debug ) {
//Main.NewText( "WantsToSpawn:"+ can_spawn+" mech1:"+ NPC.downedMechBoss1 + " mech2:"+ NPC.downedMechBoss2+" mech3:"+ NPC.downedMechBoss3
//	+" fish:"+ NPC.downedFishron+" plant:"+ NPC.downedPlantBoss+" golem:"+ NPC.downedGolemBoss+" cult:"+ NPC.downedAncientCultist
//	+" moon:"+ NPC.downedMoonlord );
//}
			return can_spawn;
		}


		////////////////

		public override string Texture { get { return "TheLunatic/NPCs/TheLunaticTownNPC"; } }

		public override bool Autoload( ref string name ) {
			name = "The Lunatic";
			return mod.Properties.Autoload;
		}

		public override void SetStaticDefaults() {
			int npc_type = this.npc.type;

			this.DisplayName.SetDefault( "The Lunatic" );

			Main.npcFrameCount[npc_type] = 26;
			NPCID.Sets.AttackFrameCount[npc_type] = 5;
			NPCID.Sets.DangerDetectRange[npc_type] = 700;
			NPCID.Sets.AttackType[npc_type] = 1;
			NPCID.Sets.AttackTime[npc_type] = 30;
			NPCID.Sets.AttackAverageChance[npc_type] = 30;
			NPCID.Sets.HatOffsetY[npc_type] = 4;
		}

		public override void SetDefaults() {
			int npc_type = this.npc.type;
			
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
			this.animationType = NPCID.Guide;
		}

		public override bool CanTownNPCSpawn( int numTownNPCs, int money ) {
			var modworld = mod.GetModWorld<MyWorld>();
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
			return false;   // :(
		}
		
		public override void AI() {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = this.mod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			TheLunaticTownNPC.AmHere = true;

			if( modworld.GameLogic.HaveWeEndSigns( mymod ) ) {
				TheLunaticTownNPC.AlertedToImpendingDoom = true;
			}
		}

		////////////////

		public override void SetupShop( Chest shop, ref int nextSlot ) {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = mymod.GetModWorld<MyWorld>();
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
			
			sugar_cookie.GetGlobalItem<MyGlobalItem>(mymod).AddedTooltip = "Bake sale!";
			gingerbread_cookie.GetGlobalItem<MyGlobalItem>( mymod ).AddedTooltip = "Bake sale!";
			christmas_pudding.GetGlobalItem<MyGlobalItem>( mymod ).AddedTooltip = "Bake sale!";
			pumpkin_pie.GetGlobalItem<MyGlobalItem>( mymod ).AddedTooltip = "Bake sale!";
			cooked_marshmallow.GetGlobalItem<MyGlobalItem>( mymod ).AddedTooltip = "Bake sale!";

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
				if( /*NPC.downedBoss1*/ modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EyeMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 43 );		// Suspicious Looking Eye
					summon_item.value = 150000;
					shop.item[nextSlot++] = summon_item;
				}
				// King Slime
				if( /*NPC.downedSlimeKing*/ modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.KingSlimeMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 560 );    // Slime Crown
					summon_item.value = 180000;
					shop.item[nextSlot++] = summon_item;
				}
				// Queen Bee
				if( /*NPC.downedQueenBee &&*/ modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BeeMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1133 );    // Abeemination
					summon_item.value = 200000;
					shop.item[nextSlot++] = summon_item;
				}
				// Brain of Cthulhu
				if( /*NPC.downedBoss2 && WorldGen.crimson &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BrainMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1331 );	// Bloody Spine
					summon_item.value = 260000;
					shop.item[nextSlot++] = summon_item;
				}
				// Eater of Worlds
				if( /*NPC.downedBoss2 && !WorldGen.crimson &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EaterMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 70 );    // Worm Food
					summon_item.value = 250000;
					shop.item[nextSlot++] = summon_item;
				}
				// Skeletron
				if( /*NPC.downedBoss3*/ modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1307 );    // Clothier Voodoo Doll
					summon_item.value = 300000;
					shop.item[nextSlot++] = summon_item;
				}
				// Wall of Flesh
				if( /*Main.hardMode*/ modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.FleshMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 267 );    // Guide Voodoo Doll
					summon_item.value = 320000;
					shop.item[nextSlot++] = summon_item;
				}
				// Destroyer
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss1 &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DestroyerMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 556 );    // Mechanical Worm
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Twins
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss2 &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.TwinMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 544 );    // Mechanical Worm
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Skeletron Prime
				if( (!strict ||(Main.hardMode)) &&
						/*NPC.downedMechBoss3 &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronPrimeMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 557 );    // Mechanical Skull
					summon_item.value = 1000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Golem
				if( (!strict || (Main.hardMode && downed_mech && NPC.downedPlantBoss)) &&
						/*NPC.downedGolemBoss &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.GolemMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 1293 );    // Lihzahrd Power Cell
					summon_item.value = 2000000;
					shop.item[nextSlot++] = summon_item;
				}
				// Duke Fishron
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedFishron &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DukeFishronMask ) ) {
					Item summon_item = new Item();
					summon_item.SetDefaults( 2673 );    // Truffle Worm
					summon_item.value = 3500000;
					shop.item[nextSlot++] = summon_item;
				}
				// Moon Lord
				if( (!strict || (Main.hardMode && downed_mech && NPC.downedPlantBoss && NPC.downedGolemBoss && NPC.downedAncientCultist)) &&
						/*NPC.downedMoonlord &&*/
						modworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BossMaskMoonlord ) ) {    //&& downed_towers
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
				var modworld = mymod.GetModWorld<MyWorld>();
				if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

				Player player = Main.player[Main.myPlayer];
				var modplayer = player.GetModPlayer<MyPlayer>( this.mod );

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
				DebugHelpers.Log( e.ToString() );
				throw e;
			}
		}

		////////////////

		private bool FirstButtonIsShop = false;

		public override void SetChatButtons( ref string button1, ref string button2 ) {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = this.mod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var modplayer = player.GetModPlayer<MyPlayer>( this.mod );
			
			if( !modplayer.IsCheater() && modworld.GameLogic.HaveWeHopeToWin( mymod ) ) {
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
			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var remaining_masks = modworld.MaskLogic.GetRemainingVanillaMasks();
			if( remaining_masks.Count == 0 ) {
				return "I need no more masks.";
			}

			bool is_custom = false;
			bool is_given = false;
			Item mask = PlayerItemHelpers.FindFirstOfItemFor( player, remaining_masks );
			if( mask == null ) {
				mask = PlayerItemHelpers.FindFirstOfItemFor( player, new HashSet<int> { mymod.ItemType<CustomBossMaskItem>() } );
				is_custom = mask != null;
				is_given = is_custom && modworld.MaskLogic.DoesLoonyHaveThisMask( mymod, mask );
			}

			if( mask == null || is_given ) {
				if( mask == null ) {
					mask = PlayerItemHelpers.FindFirstOfItemFor( player, MaskLogic.AllVanillaMasks );
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

			if( modworld.MaskLogic.IsValidMask( mymod, mask ) ) {
				int remaining = remaining_masks.Count() - (is_custom?0:1);
				string status, msg;

				if( remaining >= 2 ) {
					status = "I require " + remaining + " more masks.";
				} else if( remaining == 1 ) {
					status = "I require one final mask.";
				} else {
					if( mymod.Config.Data.LoonyGivesCompletionReward ) {
						status = "At last! Seems this world gets to live a little bit longer. I won't need this anymore. Enjoy!";
						UmbralCowlItem.Give( mymod, player );
					} else {
						status = "At last! Seems this world gets to live a little bit longer. Be sure to celebrate by buying some of my delicious treats!";
					}
				}

				switch( mask.type ) {
				case ItemID.EyeMask: // Eye of Cthulhu Mask
					msg = "A genuine Eye of Cthulhu mask. Much thanks!\nThat wasn't really Cthulhu's eye, by the way. We tried getting a real one, but it saw right through us. Right THROUGH us.";
					break;
				case ItemID.KingSlimeMask: // King Slime Mask
					msg = "A genuine King Slime mask. Much thanks!\nA bit sticky to the touch, though.";
					break;
				case ItemID.BeeMask: // Queen Bee Mask
					msg = "A genuine Queen Bee mask. Much thanks!\nI'll take it. Not one of ours, but having it gone helps us cut back on the bug spray.";
					break;
				case ItemID.BrainMask: // Brain of Cthulhu Mask
					msg = "A genuine Brain of Cthulhu mask. Much thanks!\nDon't be confused: It's a brain OF Cthulhu. Not Cthulhu's brain. Dang newbies.";
					break;
				case ItemID.EaterMask: // Eater of Worlds Mask
					msg = "A genuine Eater of Worlds mask. Much thanks!\nSuch an ominous name. It really only nibbles them a bit...";
					break;
				case ItemID.SkeletronMask: // Skeletron Mask
					msg = "A genuine Skeletron mask. Much thanks!\nYou don't even want to know what it took to stuff that thing inside the old geezer. Took it like a champ, he did.";
					break;
				case ItemID.FleshMask: // Wall of Flesh Mask
					msg = "A genuine Wall of Flesh mask. Much thanks!\nAh, you cleared up that fleshy blockage yourself. I wonder what happened to the plumber we called...";
					break;
				case ItemID.DestroyerMask: // Destroyer Mask
					msg = "A genuine Destroyer Mask mask. Much thanks!\nDo you have any idea how much assembly these req... er, thanks for the mask!";
					break;
				case ItemID.TwinMask: // Twin Mask
					msg = "A genuine Twins mask. Much thanks!\nI wonder what became of the larger thing these were supposed to be part of...";
					break;
				case ItemID.SkeletronPrimeMask: // Skeletron Prime Mask
					msg = "A genuine Skeletron Prime mask. Much thanks!\nA favorite for terrorizing small children! Uh, I mean the mask, of course! That's right!";
					break;
				case ItemID.PlanteraMask: // Plantera Mask
					msg = "A genuine Plantera mask. Much thanks!\nI have a strange urge to listen to heavy metal for some reason...";
					break;
				case ItemID.GolemMask: // Golem Mask
					msg = "A genuine Golem mask. Much thanks!\nServes those damn reptiles right for going rogue.";
					break;
				case ItemID.DukeFishronMask: // Duke Fishron Mask
					//msg = "A genuine Duke Fishron mask. Much thanks!\nThese are quite the delicacy. A heck of a fishing trip to catch, though.";
					msg = "A genuine Duke Fishron mask. Much thanks!\nHere's what you get for breeding a shark, a pig, and a dragon. Just a little leftover from someone's previous party.";
					break;
				case ItemID.BossMaskCultist: // Ancient Cultist Mask
					msg = "A genuine Ancient Cultist mask. Much thanks!\nFairwell, boss. The summoning ritual is now halted. Seems we're still getting a party of sorts, though...";
					break;
				case ItemID.BossMaskMoonlord: // Moon Lord Mask
					msg = "PRAISE BE TO YOG-er, thank you very much!";
					break;
				default:
					string mask_name = MaskLogic.GetMaskDisplayName( mask );
					msg = "A genuine "+mask_name+ ". Much thanks!\nA bit non-standard, but it should work...";
					break;
				}

				modworld.MaskLogic.GiveMaskToLoony( mymod, player, mask );

				return msg + "\n\n" + status;
			}
			else {
				// 1: Display info, 2: Fast time, 4: Reset, 8: Reset win, 16: Skip to signs, 32: Display net info
				if( mymod.IsDisplayInfoDebugMode() ) {
					DebugHelpers.Log("DEBUG cheater detected. "+ mask.Name);
				}

				if( mymod.Config.Data.LoonyShunsCheaters ) {
					player.GetModPlayer<MyPlayer>( this.mod ).SetCheater();
					return "You... you aren't supposed to even have this. Bye.";
				} else {
					return "I don't know how you got this, but I'm not ready for it yet.";
				}
			}
		}


		public string GetHint() {
			var modworld = this.mod.GetModWorld<MyWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }
			var masks = modworld.MaskLogic.GetRemainingVanillaMasks();
			string msg;
			
			if( masks.Contains( ItemID.EyeMask ) ) {
				msg = "Where to find a mask? Keep an eye out for a big eye. No really!";
			} else if( masks.Contains( ItemID.KingSlimeMask ) ) {
				msg = "You could try persuading a giant slime to give you one.";
			} else if( masks.Contains( ItemID.BrainMask ) ) {
				msg = "I'll bet a certain big-brained abomination has one.";
			} else if( masks.Contains( ItemID.EaterMask ) ) {
				msg = "I'll bet a certain giant worm's got the goods.";
			} else if( masks.Contains( ItemID.BeeMask ) ) {
				msg = "Try asking bee momma politely.";
			} else if( masks.Contains( ItemID.SkeletronMask ) ) {
				msg = "Might have to take one from our dungeon guard dog.";
			} else if( masks.Contains( ItemID.FleshMask ) ) { 
				msg = "That eldritch horror of the underworld's likely your next stop.";
			} else if( masks.Contains( ItemID.DestroyerMask ) ) {
				msg = "I expect the giant toy worm's got one.";
			} else if( masks.Contains( ItemID.TwinMask ) ) {
				msg = "I reckon the eyeball bros got one.";
			} else if( masks.Contains( ItemID.SkeletronPrimeMask ) ) {
				msg = "That machine playing copycat to our guard dog must have one.";
			} else if( masks.Contains( ItemID.PlanteraMask ) ) {
				msg = "It seems some horticulturalist's pet might give you your next mask.";
			} else if( masks.Contains( ItemID.GolemMask ) ) {
				msg = "Dang lizards. Check with their new punching toy.";
			} else if( masks.Contains( ItemID.DukeFishronMask ) ) {
				msg = "The fish lord for sure has the next.";
			} else if( masks.Contains( ItemID.BossMaskBetsy ) ) {
				msg = "Buncha foreigners and their pet flying lizard brought one.";
			} else if( masks.Contains( ItemID.BossMaskCultist ) ) {
				msg = "*sigh* Gonna have to get the next one from my boss. You didn't hear this from me.";
			} else if( masks.Contains( ItemID.BossMaskMoonlord ) ) {
				msg = "You'll have to wrest the last one from Cthulhu's kin. Good luck!";
			} else {
				msg = "No additional masks required.";
			}

			return msg;
		}
	}
}