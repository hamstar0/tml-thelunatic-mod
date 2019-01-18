using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.PlayerHelpers;
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

		public static bool AlertedToImpendingDoom { get; private set; }
		public static bool AmHere { get; private set; }


		static TheLunaticTownNPC() {
			TheLunaticTownNPC.AlertedToImpendingDoom = false;
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

		public static bool WantsToSpawnAnew() {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			bool canSpawn = !myworld.GameLogic.HasLoonyQuit;

			if( canSpawn && mymod.ConfigJson.Data.LoonyEnforcesBossSequence ) {
				// Allow spawning if moon lord gone (shop use only)
				if( !NPC.downedMoonlord ) {
					//if( NPC.downedSlimeKing ) { can_spawn = false; }		// King Slime
					if( NPC.downedBoss1 ) { canSpawn = false; }            // Eye of Cthulhu
					if( NPC.downedQueenBee ) { canSpawn = false; }         // Queen Bee
					if( NPC.downedBoss2 ) { canSpawn = false; }            // Brain of Cthulhu && Eater of Worlds
					if( NPC.downedBoss3 ) { canSpawn = false; }            // Skeletron
					if( Main.hardMode ) { canSpawn = false; }              // Wall of Flesh
					if( NPC.downedMechBoss1 ) { canSpawn = false; }        // Destroyer
					if( NPC.downedMechBoss2 ) { canSpawn = false; }        // Twin
					if( NPC.downedMechBoss3 ) { canSpawn = false; }        // Skeletron Prime
					if( NPC.downedFishron ) { canSpawn = false; }          // Duke Fishron
					if( NPC.downedPlantBoss ) { canSpawn = false; }        // Plantera
					if( NPC.downedGolemBoss ) { canSpawn = false; }        // Golem
					if( NPC.downedAncientCultist ) { canSpawn = false; }   // Ancient Cultist
				}
			}
			
			return canSpawn;
		}

		public static bool WantsToSpawn() {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception("Game logic not initialized."); }

			bool canSpawn = !myworld.GameLogic.HasLoonyQuit;

			if( mymod.ConfigJson.Data.LoonyEnforcesBossSequence ) {
				if( canSpawn && !Main.hardMode ) {
					canSpawn = !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && !NPC.downedFishron
						&& !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( canSpawn && !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 ) {
					canSpawn = !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( canSpawn && !NPC.downedPlantBoss ) {
					canSpawn = !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( canSpawn && !NPC.downedGolemBoss ) {
					canSpawn = !NPC.downedAncientCultist && !NPC.downedMoonlord;
				}
				if( canSpawn && !NPC.downedAncientCultist ) {
					canSpawn = !NPC.downedMoonlord;
				}
			}
			
//if( _debug ) {
//Main.NewText( "WantsToSpawn:"+ can_spawn+" mech1:"+ NPC.downedMechBoss1 + " mech2:"+ NPC.downedMechBoss2+" mech3:"+ NPC.downedMechBoss3
//	+" fish:"+ NPC.downedFishron+" plant:"+ NPC.downedPlantBoss+" golem:"+ NPC.downedGolemBoss+" cult:"+ NPC.downedAncientCultist
//	+" moon:"+ NPC.downedMoonlord );
//}
			return canSpawn;
		}


		////////////////

		public override string Texture => "TheLunatic/NPCs/TheLunaticTownNPC";

		public override bool Autoload( ref string name ) {
			name = "The Lunatic";
			return this.mod.Properties.Autoload;
		}

		public override void SetStaticDefaults() {
			int npcType = this.npc.type;

			this.DisplayName.SetDefault( "The Lunatic" );

			Main.npcFrameCount[npcType] = 26;
			NPCID.Sets.AttackFrameCount[npcType] = 5;
			NPCID.Sets.DangerDetectRange[npcType] = 700;
			NPCID.Sets.AttackType[npcType] = 1;
			NPCID.Sets.AttackTime[npcType] = 30;
			NPCID.Sets.AttackAverageChance[npcType] = 30;
			NPCID.Sets.HatOffsetY[npcType] = 4;
		}

		public override void SetDefaults() {
			int npcType = this.npc.type;
			
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
			var myworld = mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception("Game logic not initialized."); }

			if( myworld.GameLogic.KillSurfaceTownNPCs ) { return false; }
			return TheLunaticTownNPC.WantsToSpawn();
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
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			TheLunaticTownNPC.AmHere = true;

			if( myworld.GameLogic.HaveWeEndSigns() ) {
				TheLunaticTownNPC.AlertedToImpendingDoom = true;
			}
		}

		////////////////

		public override void SetupShop( Chest shop, ref int nextSlot ) {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			bool strict = mymod.ConfigJson.Data.LoonyEnforcesBossSequence;
			bool downedMech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
			bool downedTowers = NPC.downedTowerSolar && NPC.downedTowerVortex && NPC.downedTowerNebula && NPC.downedTowerStardust;

			// Bake sale!
			Item sugarCookie = new Item();
			Item gingerbreadCookie = new Item();
			Item christmasPudding = new Item();
			Item pumpkinPie = new Item();
			Item cookedMarshmallow = new Item();

			sugarCookie.SetDefaults( 1919 );
			gingerbreadCookie.SetDefaults( 1920 );
			christmasPudding.SetDefaults( 1911 );
			pumpkinPie.SetDefaults( 1787 );
			cookedMarshmallow.SetDefaults( 969 );

			sugarCookie.value *= 8;
			gingerbreadCookie.value *= 8;
			christmasPudding.value *= 9;
			pumpkinPie.value *= 9;
			cookedMarshmallow.value *= 5;
			
			sugarCookie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			gingerbreadCookie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			christmasPudding.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			pumpkinPie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			cookedMarshmallow.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";

			shop.item[nextSlot++] = sugarCookie;
			shop.item[nextSlot++] = gingerbreadCookie;
			shop.item[nextSlot++] = christmasPudding;
			shop.item[nextSlot++] = pumpkinPie;
			shop.item[nextSlot++] = cookedMarshmallow;

			for( int i=0; i<15; i++ ) {
				shop.item[nextSlot++] = new Item();
			}

			// Boss summon items
			if( mymod.ConfigJson.Data.LoonySellsSummonItems ) {
				// Eye of Cthulhu
				if( /*NPC.downedBoss1*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EyeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 43 );		// Suspicious Looking Eye
					summonItem.value = 150000;
					shop.item[nextSlot++] = summonItem;
				}
				// King Slime
				if( /*NPC.downedSlimeKing*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.KingSlimeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 560 );    // Slime Crown
					summonItem.value = 180000;
					shop.item[nextSlot++] = summonItem;
				}
				// Queen Bee
				if( /*NPC.downedQueenBee &&*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BeeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1133 );    // Abeemination
					summonItem.value = 200000;
					shop.item[nextSlot++] = summonItem;
				}
				// Brain of Cthulhu
				if( /*NPC.downedBoss2 && WorldGen.crimson &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BrainMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1331 );	// Bloody Spine
					summonItem.value = 260000;
					shop.item[nextSlot++] = summonItem;
				}
				// Eater of Worlds
				if( /*NPC.downedBoss2 && !WorldGen.crimson &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EaterMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 70 );    // Worm Food
					summonItem.value = 250000;
					shop.item[nextSlot++] = summonItem;
				}
				// Skeletron
				if( /*NPC.downedBoss3*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1307 );    // Clothier Voodoo Doll
					summonItem.value = 300000;
					shop.item[nextSlot++] = summonItem;
				}
				// Wall of Flesh
				if( /*Main.hardMode*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.FleshMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 267 );    // Guide Voodoo Doll
					summonItem.value = 320000;
					shop.item[nextSlot++] = summonItem;
				}
				// Destroyer
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss1 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DestroyerMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 556 );    // Mechanical Worm
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Twins
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss2 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.TwinMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 544 );    // Mechanical Worm
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Skeletron Prime
				if( (!strict ||(Main.hardMode)) &&
						/*NPC.downedMechBoss3 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronPrimeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 557 );    // Mechanical Skull
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Golem
				if( (!strict || (Main.hardMode && downedMech && NPC.downedPlantBoss)) &&
						/*NPC.downedGolemBoss &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.GolemMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1293 );    // Lihzahrd Power Cell
					summonItem.value = 2000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Duke Fishron
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedFishron &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DukeFishronMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 2673 );    // Truffle Worm
					summonItem.value = 3500000;
					shop.item[nextSlot++] = summonItem;
				}
				// Moon Lord
				if( (!strict || (Main.hardMode && downedMech && NPC.downedPlantBoss && NPC.downedGolemBoss && NPC.downedAncientCultist)) &&
						/*NPC.downedMoonlord &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BossMaskMoonlord ) ) {    //&& downed_towers
					Item summonItem = new Item();
					summonItem.SetDefaults( 3601 );    // Celestial Sigil
					summonItem.value = 8000000;
					shop.item[nextSlot++] = summonItem;
				}
			}
		}

		////////////////
		
		public override string GetChat() {
			try {
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();
				if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

				Player player = Main.player[Main.myPlayer];
				var myplayer = player.GetModPlayer<TheLunaticPlayer>();

				if( myplayer.IsCheater() ) {
					return TheLunaticTownNPC.DismissalReplies[ Main.rand.Next(TheLunaticTownNPC.DismissalReplies.Length) ];
				}
				
				string msg;
				int daysLeft = mymod.ConfigJson.Data.DaysUntil - (myworld.GameLogic.HalfDaysElapsed / 2);
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

		////////////////

		private bool FirstButtonIsShop = false;

		public override void SetChatButtons( ref string button1, ref string button2 ) {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var myplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
			
			if( !myplayer.IsCheater() && myworld.GameLogic.HaveWeHopeToWin() ) {
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
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var remaining_masks = myworld.MaskLogic.GetRemainingVanillaMasks();
			if( remaining_masks.Count == 0 ) {
				return "I need no more masks.";
			}

			bool isCustom = false;
			bool isGiven = false;
			
			Item mask = PlayerItemFinderHelpers.FindFirstOfItemFor( player, remaining_masks );
			if( mask == null ) {
				mask = PlayerItemFinderHelpers.FindFirstOfItemFor( player, new HashSet<int> { mymod.ItemType<CustomBossMaskItem>() } );
				isCustom = mask != null;
				isGiven = isCustom && myworld.MaskLogic.DoesLoonyHaveThisMask( mask );
			}

			if( mask == null || isGiven ) {
				if( mask == null ) {
					mask = PlayerItemFinderHelpers.FindFirstOfItemFor( player, MaskLogic.AllVanillaMasks );
				}
				string msg, hint = this.GetHint();

				if( mask == null ) {
					msg = "Ehehe, good one! But seriously, you DO have a mask... don't you? ...please?\n" + hint;
				} else {
					msg = "Very nice, but I've already got a " + MaskLogic.GetMaskDisplayName( mask ) + ".\n" + hint;
				}

				if( !myworld.GameLogic.HasGameEnded && mymod.ConfigJson.Data.LoonyIndicatesDaysRemaining ) {
					int daysLeft = mymod.ConfigJson.Data.DaysUntil - (myworld.GameLogic.HalfDaysElapsed / 2);
					msg += "\n \nDays remaining: " + daysLeft;
				}
				return msg;
			}

			if( myworld.MaskLogic.IsValidMask( mask ) ) {
				int remaining = remaining_masks.Count() - (isCustom?0:1);
				string status, msg;

				if( mymod.Config.MoonLordMaskWins ) {
					if( mask.type == ItemID.BossMaskMoonlord ) {
						status = "At last! Seems this world gets to live a little bit longer. I won't need this anymore. Enjoy!";
						UmbralCowlItem.Give( player );
					} else {
						status = "I require more masks!";
					}
				} else {
					if( remaining >= 2 ) {
						status = "I require " + remaining + " more masks.";
					} else if( remaining == 1 ) {
						status = "I require one final mask.";
					} else {
						if( mymod.ConfigJson.Data.LoonyGivesCompletionReward ) {
							status = "At last! Seems this world gets to live a little bit longer. I won't need this anymore. Enjoy!";
							UmbralCowlItem.Give( player );
						} else {
							status = "At last! Seems this world gets to live a little bit longer. Be sure to celebrate by buying some of my delicious treats!";
						}
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

				myworld.MaskLogic.GiveMaskToLoony( player, mask );

				return msg + "\n\n" + status;
			}
			else {
				// 1: Display info, 2: Fast time, 4: Reset, 8: Reset win, 16: Skip to signs, 32: Display net info
				if( mymod.Config.DebugModeInfo ) {
					LogHelpers.Log("DEBUG cheater detected. "+ mask.Name);
				}

				if( mymod.ConfigJson.Data.LoonyShunsCheaters ) {
					player.GetModPlayer<TheLunaticPlayer>().SetCheater();
					return "You... you aren't supposed to even have this. Bye.";
				} else {
					return "I don't know how you got this, but I'm not ready for it yet.";
				}
			}
		}


		public string GetHint() {
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }
			var masks = myworld.MaskLogic.GetRemainingVanillaMasks();
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