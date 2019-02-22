using HamstarHelpers.Components.Errors;
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
	partial class TheLunaticTownNPC : ModNPC {
		private bool FirstButtonIsShop = false;

		public override void SetChatButtons( ref string button1, ref string button2 ) {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new HamstarException( "Game logic not initialized." ); }

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


		public override void OnChatButtonClicked( bool firstButton, ref bool shop ) {
			if( firstButton ) {
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
			if( myworld.MaskLogic == null ) { throw new HamstarException( "Mask logic not initialized." ); }

			Player player = Main.player[Main.myPlayer];
			var remainingMasks = myworld.MaskLogic.GetRemainingVanillaMasks();
			if( remainingMasks.Count == 0 ) {
				return "I need no more masks.";
			}

			bool isCustom = false;
			bool isGiven = false;
			
			Item mask = PlayerItemFinderHelpers.FindFirstOfItemFor( player, remainingMasks );
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
				int remaining = remainingMasks.Count() - (isCustom?0:1);
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

				msg = this.GiveMaskReply( mask );

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


		////////////////

		public string GiveMaskReply( Item mask ) {
			string msg;

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
				string maskName = MaskLogic.GetMaskDisplayName( mask );
				msg = "A genuine " + maskName + ". Much thanks!\nA bit non-standard, but it should work...";
				break;
			}

			return msg;
		}

		////

		public string GetHint() {
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.MaskLogic == null ) { throw new HamstarException( "Mask logic not initialized." ); }
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