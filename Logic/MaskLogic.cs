using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.NPCHelpers;
using HamstarHelpers.Helpers.WorldHelpers;
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
			if( MaskLogic.VanillaBossOfMask.Keys.Contains(npcType) ) {
				return MaskLogic.VanillaBossOfMask[ npcType ];
			}

			var mymod = TheLunaticMod.Instance;
			NPC npc = new NPC();
			npc.SetDefaults( npcType );

			if( npc.boss ) {
				return mymod.ItemType<CustomBossMaskItem>();
			}
			return -1;
		}

		public static string GetMaskDisplayName( Item mask ) {
			if( MaskLogic.AllVanillaMasks.Contains(mask.type) ) {
				return mask.Name;
			}

			var mymod = TheLunaticMod.Instance;
			int customType = mymod.ItemType<CustomBossMaskItem>();
			if( mask.type == customType ) {
				var maskItemInfo = mask.GetGlobalItem<CustomBossMaskItemInfo>();
				if( maskItemInfo != null ) {
					return maskItemInfo.BossDisplayName + " Mask";
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

		public void RegisterReceiptOfMask( Player givingPlayer, int maskType, int bossType ) {
			var mymod = TheLunaticMod.Instance;

			if( maskType == mymod.ItemType<CustomBossMaskItem>() ) {
				NPC npc = new NPC();
				npc.SetDefaults( bossType );

				this.GivenCustomMasksByBossUid.Add( NPCIdentityHelpers.GetUniqueId(npc) );
			} else {
				this.GivenVanillaMasksByType.Add( maskType );
			}

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG Registering mask. " + givingPlayer.name + ", " + maskType );
			}

			// Buy time before the end comes
			if( this.GivenVanillaMasksByType.Count < (MaskLogic.AvailableMaskCount) ) {
				var modworld = mymod.GetModWorld<TheLunaticWorld>();
				int recovered = mymod.ConfigJson.Data.HalfDaysRecoveredPerMask;
				
				switch( maskType ) {
				case ItemID.FleshMask:
					recovered = (int)((float)recovered * mymod.ConfigJson.Data.WallOfFleshMultiplier);
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
					if( maskType == ItemID.BossMaskMoonlord && mymod.ConfigJson.Data.MoonLordMaskWins ) {
						this.GiveAllVanillaMasks();
					}
					recovered = (int)((float)recovered * mymod.ConfigJson.Data.HardModeMultiplier);
					break;
				}

				if( WorldStateHelpers.GetDayOrNightPercentDone() > 0.5f ) {
					recovered += 1;
				}
				
				modworld.GameLogic.SetTime( modworld.GameLogic.HalfDaysElapsed - recovered );
			}

			// Sky flash for all
			if( Main.netMode != 2 ) {  // Not server
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

		public bool DoesLoonyHaveThisMask( Item mask_item ) {
			if( this.GetRemainingVanillaMasks().Contains(mask_item.type) ) { return false; }

			var mask_item_info = mask_item.GetGlobalItem<CustomBossMaskItemInfo>();
			return this.GivenCustomMasksByBossUid.Contains( mask_item_info.BossUid );
		}

		public void GiveAllVanillaMasks() {
			foreach( int mask_type in this.GetRemainingVanillaMasks() ) {
				this.GivenVanillaMasksByType.Add( mask_type );
			}
		}


		////////////////

		public bool IsValidMask( Item mask ) {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( !myworld.GameLogic.HaveWeHopeToWin() ) { return false; }

			if( !mymod.ConfigJson.Data.LoonyAcceptsMasksWithoutBossKill ) {
				bool strict = mymod.ConfigJson.Data.LoonyEnforcesBossSequence;
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

		public void GiveMaskToLoony( Player player, Item mask ) {
			var mymod = TheLunaticMod.Instance;

			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendGivenMaskFromClient( mask );
			} else if( Main.netMode == 2 ) {    // Server
				throw new Exception( "Server should not be giving masks to loonys." );
			}

			int bossType = -1;
			if( mask.type == mymod.ItemType<CustomBossMaskItem>() ) {
				bossType = mask.GetGlobalItem<CustomBossMaskItemInfo>().BossNpcType;
			} else {
				var bossOfMask = MaskLogic.VanillaBossOfMask.Where( x => x.Value == mask.type ).First();
				bossType = bossOfMask.Value > 0 ? bossOfMask.Value : bossType;
			}
			this.RegisterReceiptOfMask( player, mask.type, bossType );

			mask.TurnToAir();

			Main.PlaySound( SoundID.Unlock, player.position );
		}
	}
}
