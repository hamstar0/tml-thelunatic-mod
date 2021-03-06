﻿using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.World;
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

		public static int GetMaskTypeOfNpc( int npcType ) {
			if( npcType == 0 || npcType == -1 ) {	// -1 for legacy support
				return -1;
			}
			if( MaskLogic.VanillaBossOfMask.Keys.Contains(npcType) ) {
				return MaskLogic.VanillaBossOfMask[ npcType ];
			}

			var mymod = TheLunaticMod.Instance;
			NPC npc = new NPC();
			npc.SetDefaults( npcType );

			if( npc.boss ) {
				return ModContent.ItemType<CustomBossMaskItem>();
			}
			return -1;
		}

		public static string GetMaskDisplayName( Item maskItem ) {
			if( MaskLogic.AllVanillaMasks.Contains(maskItem.type) ) {
				return maskItem.Name;
			}

			var mymod = TheLunaticMod.Instance;
			int customType = ModContent.ItemType<CustomBossMaskItem>();
			if( maskItem.type == customType && maskItem.modItem != null ) {
				var myMaskItem = (CustomBossMaskItem)maskItem.modItem;
				if( myMaskItem != null ) {
					return myMaskItem.BossDisplayName + " Mask";
				}
			}

			return null;
		}

		////////////////

		public MaskLogic() {
			this.GivenVanillaMasksByType = new HashSet<int>();
			this.GivenCustomMasksByBossUid = new HashSet<string>();
		}
		

		public void LoadOnce( int[] masks, string[] customMasks ) {
			var mymod = TheLunaticMod.Instance;

			if( this.IsLoaded ) {
				LogHelpers.Log( "Redundant Mask Logic load. " + String.Join( ",", masks ) + " (" + String.Join( ",", this.GivenVanillaMasksByType ) + ")" );
				return;
			}

			this.GivenVanillaMasksByType = new HashSet<int>( masks );
			this.GivenCustomMasksByBossUid = new HashSet<string>( customMasks );

			if( mymod.Config.DebugModeReset ) {
				this.GivenVanillaMasksByType.Clear();
				this.GivenCustomMasksByBossUid.Clear();
			}
			if( mymod.Config.DebugModeResetWin ) {
				this.GivenVanillaMasksByType.Remove( ItemID.BossMaskMoonlord );
			}
			this.IsLoaded = true;
		}
		private bool IsLoaded = false;


		////////////////

		public void RegisterReceiptOfMask( Player givingPlayer, int maskType, int bossNpcType ) {
			var mymod = TheLunaticMod.Instance;

			if( maskType == ModContent.ItemType<CustomBossMaskItem>() && bossNpcType != 0 && bossNpcType != -1 ) {	// -1 for legacy support
				NPC npc = new NPC();
				npc.SetDefaults( bossNpcType );

				this.GivenCustomMasksByBossUid.Add( NPCID.GetUniqueKey(npc) );
			} else {
				this.GivenVanillaMasksByType.Add( maskType );
			}

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG Registering mask. " + givingPlayer.name + ", " + maskType );
			}

			// Buy time before the end comes
			if( this.GivenVanillaMasksByType.Count < (MaskLogic.AvailableMaskCount) ) {
				var modworld = ModContent.GetInstance<TheLunaticWorld>();
				int recovered = mymod.Config.HalfDaysRecoveredPerMask;
				
				switch( maskType ) {
				case ItemID.FleshMask:
					recovered = (int)((float)recovered * mymod.Config.WallOfFleshMultiplier);
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
					if( maskType == ItemID.BossMaskMoonlord && mymod.Config.MoonLordMaskWins ) {
						this.GiveAllVanillaMasks();
					}
					recovered = (int)((float)recovered * mymod.Config.HardModeMultiplier);
					break;
				}

				if( WorldStateHelpers.GetDayOrNightPercentDone() > 0.5f ) {
					recovered += 1;
				}
				
				modworld.GameLogic.SetTime( modworld.GameLogic.HalfDaysElapsed - recovered );
			}

			// Sky flash for all
			if( !Main.dedServ && Main.netMode != 2 ) {  // Not server
				Player currentPlayer = Main.player[Main.myPlayer];
				var modplayer = currentPlayer.GetModPlayer<TheLunaticPlayer>();
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

		public bool DoesLoonyHaveThisMask( Item maskItem ) {
			if( this.GetRemainingVanillaMasks().Contains(maskItem.type) ) {
				return false;
			}

			string bossUid = "";
			if( maskItem.modItem != null && maskItem.modItem is CustomBossMaskItem ) {
				bossUid = ((CustomBossMaskItem)maskItem.modItem).BossUid;
			}

			return this.GivenCustomMasksByBossUid?.Contains( bossUid ) ?? false;
		}

		public void GiveAllVanillaMasks() {
			foreach( int maskType in this.GetRemainingVanillaMasks() ) {
				this.GivenVanillaMasksByType.Add( maskType );
			}
		}


		////////////////

		public bool IsValidMask( Item mask ) {
			var mymod = TheLunaticMod.Instance;
			var myworld = ModContent.GetInstance<TheLunaticWorld>();
			if( !myworld.GameLogic.HaveWeHopeToWin() ) { return false; }

			if( !mymod.Config.LoonyAcceptsMasksWithoutBossKill ) {
				bool strict = mymod.Config.LoonyEnforcesBossSequence;
				bool downedMech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
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
					if( (strict && (!Main.hardMode || !downedMech)) || !NPC.downedPlantBoss ) { return false; }
					break;
				case ItemID.GolemMask:
					if( (strict && (!Main.hardMode || !downedMech || !NPC.downedPlantBoss)) || !NPC.downedGolemBoss ) { return false; }
					break;
				case ItemID.BossMaskBetsy:
					if( (strict && (!Main.hardMode || !downedMech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ) { return false; }
					break;
				case ItemID.BossMaskCultist:
					if( (strict && (!Main.hardMode || !downedMech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ||
						!NPC.downedAncientCultist ) { return false; }
					break;
				case ItemID.BossMaskMoonlord:
					if( (strict && (!Main.hardMode || !downedMech || !NPC.downedPlantBoss || !NPC.downedGolemBoss || !NPC.downedAncientCultist)) ||
						!NPC.downedMoonlord ) { return false; }  //|| !downed_towers
					break;
				}
			}

			if( (WorldGen.crimson && mask.type == ItemID.EaterMask) || (!WorldGen.crimson && mask.type == ItemID.BrainMask) ) {
				return false;
			}

			return true;
		}

		public void GiveMaskToLoony( Player player, Item maskItem ) {
			var mymod = TheLunaticMod.Instance;

			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendGivenMaskFromClient( maskItem );
			} else if( Main.netMode == 2 ) {    // Server
				throw new ModHelpersException( "Server should not be giving masks to loonys." );
			}

			int bossType = -1;
			if( maskItem.type == ModContent.ItemType<CustomBossMaskItem>() && maskItem.modItem != null ) {
				bossType = ( (CustomBossMaskItem)maskItem.modItem ).BossNpcType;
			} else {
				var bossOfMask = MaskLogic.VanillaBossOfMask
					.Where( x => x.Value == maskItem.type )
					.First();
				bossType = bossOfMask.Value > 0 ? bossOfMask.Value : bossType;
			}
			this.RegisterReceiptOfMask( player, maskItem.type, bossType );

			maskItem.TurnToAir();

			Main.PlaySound( SoundID.Unlock, player.position );
		}
	}
}
