using HamstarHelpers.DebugHelpers;
using HamstarHelpers.NPCHelpers;
using HamstarHelpers.WorldHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.NetProtocol;

namespace TheLunatic.Logic {
	class MaskLogic {
		public static ISet<int> AllVanillaMasks { get; private set; }
		public static IDictionary<int, int> VanillaBossOfMask { get; private set; }
		public static int AvailableMaskCount { get; private set; }

		public ISet<int> GivenVanillaMasksByType { get; private set; }
		public ISet<string> GivenCustomMasksByBossUid { get; private set; }
		

		////////////////

		static MaskLogic() {
			MaskLogic.AllVanillaMasks = new HashSet<int> {
				ItemID.SkeletronMask,
				ItemID.BrainMask,
				ItemID.FleshMask,
				ItemID.TwinMask,
				ItemID.SkeletronPrimeMask,
				ItemID.BeeMask,
				ItemID.PlanteraMask,
				ItemID.GolemMask,
				ItemID.EaterMask,
				ItemID.EyeMask,
				ItemID.DestroyerMask,
				ItemID.KingSlimeMask,
				ItemID.DukeFishronMask,
				ItemID.BossMaskCultist,
				ItemID.BossMaskBetsy,
				ItemID.BossMaskMoonlord
			};
			MaskLogic.VanillaBossOfMask = new Dictionary<int, int> {
				{ NPCID.EyeofCthulhu, ItemID.EyeMask },
				{ NPCID.KingSlime, ItemID.KingSlimeMask },
				{ NPCID.QueenBee, ItemID.BeeMask },
				{ NPCID.EaterofWorldsBody, ItemID.EaterMask },
				{ NPCID.EaterofWorldsHead, ItemID.EaterMask },
				{ NPCID.EaterofWorldsTail, ItemID.EaterMask },
				{ NPCID.BrainofCthulhu, ItemID.BrainMask },
				{ NPCID.SkeletronHead, ItemID.SkeletronMask },
				{ NPCID.WallofFlesh, ItemID.FleshMask },
				{ NPCID.Retinazer, ItemID.TwinMask },
				{ NPCID.Spazmatism, ItemID.TwinMask },
				{ NPCID.SkeletronPrime, ItemID.SkeletronPrimeMask },
				{ NPCID.TheDestroyer, ItemID.DestroyerMask },
				{ NPCID.Golem, ItemID.GolemMask },
				{ NPCID.Plantera, ItemID.PlanteraMask },
				{ NPCID.DukeFishron, ItemID.DukeFishronMask },
				{ NPCID.CultistBoss, ItemID.BossMaskCultist },
				{ NPCID.DD2Betsy, ItemID.BossMaskBetsy },
				{ NPCID.MoonLordCore, ItemID.BossMaskMoonlord }
			};
			MaskLogic.AvailableMaskCount = MaskLogic.AllVanillaMasks.Count - 1;	// Either corruption or crimson; not both
		}

		public static int GetMaskTypeOfNpc( int npc_type ) {
			if( MaskLogic.VanillaBossOfMask.Keys.Contains(npc_type) ) {
				return MaskLogic.VanillaBossOfMask[ npc_type ];
			}

			NPC npc = new NPC();
			npc.SetDefaults( npc_type );
			if( npc.boss ) {
				return ModLoader.GetMod( "TheLunatic" ).ItemType<CustomBossMaskItem>();
			}
			return -1;
		}

		public static string GetMaskDisplayName( Item mask ) {
			if( MaskLogic.AllVanillaMasks.Contains(mask.type) ) {
				return mask.Name;
			}

			Mod mod = ModLoader.GetMod( "TheLunatic" );
			int custom_type = mod.ItemType<CustomBossMaskItem>();
			if( mask.type == custom_type ) {
				var mask_item_info = mask.GetGlobalItem<CustomBossMaskItemInfo>( mod );
				if( mask_item_info != null ) {
					return mask_item_info.BossDisplayName + " Mask";
				}
			}

			return null;
		}

		////////////////

		public MaskLogic() {
			this.GivenVanillaMasksByType = new HashSet<int>();
			this.GivenCustomMasksByBossUid = new HashSet<string>();
		}
		

		public void LoadOnce( TheLunatic mymod, int[] masks, string[] custom_masks ) {
			if( this.IsLoaded ) {
				DebugHelpers.Log( "Redundant Mask Logic load. " + String.Join( ",", masks ) + " (" + String.Join( ",", this.GivenVanillaMasksByType ) + ")" );
				return;
			}

			this.GivenVanillaMasksByType = new HashSet<int>( masks );
			this.GivenCustomMasksByBossUid = new HashSet<string>( custom_masks );

			if( mymod.IsResetDebugMode() ) {
				this.GivenVanillaMasksByType.Clear();
				this.GivenCustomMasksByBossUid.Clear();
			}
			if( mymod.IsResetWinDebugMode() ) {
				this.GivenVanillaMasksByType.Remove( ItemID.BossMaskMoonlord );
			}
			this.IsLoaded = true;
		}
		private bool IsLoaded = false;


		////////////////

		public void RegisterReceiptOfMask( TheLunatic mymod, Player giving_player, int mask_type, int boss_type ) {
			if( mask_type == mymod.ItemType<CustomBossMaskItem>() ) {
				NPC npc = new NPC();
				npc.SetDefaults( boss_type );
				this.GivenCustomMasksByBossUid.Add( NPCIdentityHelpers.GetUniqueId(npc) );
			} else {
				this.GivenVanillaMasksByType.Add( mask_type );
			}
			
			if( mymod.IsDisplayInfoDebugMode() ) {
				DebugHelpers.Log( "DEBUG Registering mask. " + giving_player.name + ", " + mask_type );
			}

			// Buy time before the end comes
			if( this.GivenVanillaMasksByType.Count < (MaskLogic.AvailableMaskCount) ) {
				var modworld = mymod.GetModWorld<MyModWorld>();
				int recovered = mymod.Config.Data.HalfDaysRecoveredPerMask;
				
				switch( mask_type ) {
				case ItemID.FleshMask:
					recovered = (int)((float)recovered * mymod.Config.Data.WallOfFleshMultiplier);
					break;
				case ItemID.DestroyerMask:
				case ItemID.TwinMask:
				case ItemID.SkeletronPrimeMask:
				case ItemID.PlanteraMask:
				case ItemID.GolemMask:
				case ItemID.DukeFishronMask:
				case ItemID.BossMaskBetsy:
				case ItemID.BossMaskCultist:
				case ItemID.BossMaskMoonlord:
					if( mask_type == ItemID.BossMaskMoonlord && mymod.Config.Data.MoonLordMaskWins ) {
						this.GiveAllVanillaMasks();
					}
					recovered = (int)((float)recovered * mymod.Config.Data.HardModeMultiplier);
					break;
				}

				if( WorldHelpers.GetDayOrNightPercentDone() > 0.5f ) {
					recovered += 1;
				}
				
				modworld.GameLogic.SetTime( modworld.GameLogic.HalfDaysElapsed - recovered );
			}

			// Sky flash for all
			if( Main.netMode != 2 ) {  // Not server
				Player current_player = Main.player[Main.myPlayer];
				var modplayer = current_player.GetModPlayer<MyModPlayer>( mymod );
				modplayer.FlashMe();
			}
		}

		public ISet<int> GetRemainingVanillaMasks() {
			ISet<int> masks = new HashSet<int>( MaskLogic.AllVanillaMasks.Where(x => !this.GivenVanillaMasksByType.Contains(x)) );

			if( WorldGen.crimson ) {
				masks.Remove( ItemID.EaterMask );
			} else {
				masks.Remove( ItemID.BrainMask );
			}

			return masks;
		}

		public bool DoesLoonyHaveThisMask( TheLunatic mymod, Item mask_item ) {
			if( this.GetRemainingVanillaMasks().Contains(mask_item.type) ) { return false; }

			var mask_item_info = mask_item.GetGlobalItem<CustomBossMaskItemInfo>( mymod );
			return this.GivenCustomMasksByBossUid.Contains( mask_item_info.BossUid );
		}

		public void GiveAllVanillaMasks() {
			foreach( int mask_type in this.GetRemainingVanillaMasks() ) {
				this.GivenVanillaMasksByType.Add( mask_type );
			}
		}


		////////////////

		public bool IsValidMask( TheLunatic mymod, Item mask ) {
			var modworld = mymod.GetModWorld<MyModWorld>();
			if( !modworld.GameLogic.HaveWeHopeToWin(mymod) ) { return false; }

			if( !mymod.Config.Data.LoonyAcceptsMasksWithoutBossKill ) {
				bool strict = mymod.Config.Data.LoonyEnforcesBossSequence;
				bool downed_mech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
				//bool downed_towers = NPC.downedTowerSolar && NPC.downedTowerVortex && NPC.downedTowerNebula && NPC.downedTowerStardust;

				switch( mask.type ) {
				case ItemID.EyeMask:
					if( !NPC.downedBoss1 ) { return false; }
					break;
				case ItemID.BrainMask:
					if( !NPC.downedBoss2 ) { return false; }
					break;
				case ItemID.EaterMask:
					if( !NPC.downedBoss2 ) { return false; }
					break;
				case ItemID.SkeletronMask:
					if( !NPC.downedBoss3 ) { return false; }
					break;
				case ItemID.KingSlimeMask:
					if( !NPC.downedSlimeKing ) { return false; }
					break;
				case ItemID.BeeMask:
					if( !NPC.downedQueenBee ) { return false; }
					break;
				case ItemID.FleshMask:
					if( !Main.hardMode ) { return false; }
					break;
				case ItemID.DestroyerMask:
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss1 ) { return false; }
					break;
				case ItemID.TwinMask:
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss2 ) { return false; }
					break;
				case ItemID.SkeletronPrimeMask:
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss3 ) { return false; }
					break;
				case ItemID.DukeFishronMask:
					if( (strict && (!Main.hardMode)) || !NPC.downedFishron ) { return false; }
					break;
				case ItemID.PlanteraMask:
					if( (strict && (!Main.hardMode || !downed_mech)) || !NPC.downedPlantBoss ) { return false; }
					break;
				case ItemID.GolemMask:
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss)) || !NPC.downedGolemBoss ) { return false; }
					break;
				case ItemID.BossMaskBetsy:
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ) { return false; }
					break;
				case ItemID.BossMaskCultist:
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ||
						!NPC.downedAncientCultist ) { return false; }
					break;
				case ItemID.BossMaskMoonlord:
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss || !NPC.downedAncientCultist)) ||
						!NPC.downedMoonlord ) { return false; }  //|| !downed_towers
					break;
				}
			}

			if( (WorldGen.crimson && mask.type == ItemID.EaterMask) || (!WorldGen.crimson && mask.type == ItemID.BrainMask) ) {
				return false;
			}

			return true;
		}

		public void GiveMaskToLoony( TheLunatic mymod, Player player, Item mask ) {
			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendGivenMaskFromClient( mymod, mask );
			} else if( Main.netMode == 2 ) {    // Server
				throw new Exception( "Server should not be giving masks to loonys." );
			}

			int boss_type = -1;
			if( mask.type == mymod.ItemType<CustomBossMaskItem>() ) {
				boss_type = mask.GetGlobalItem<CustomBossMaskItemInfo>( mymod ).BossNpcType;
			} else {
				var boss_of_mask = MaskLogic.VanillaBossOfMask.Where( x => x.Value == mask.type ).First();
				boss_type = boss_of_mask.Value > 0 ? boss_of_mask.Value : boss_type;
			}
			this.RegisterReceiptOfMask( mymod, player, mask.type, boss_type );

			mask.TurnToAir();

			Main.PlaySound( SoundID.Unlock, player.position );
		}
	}
}
