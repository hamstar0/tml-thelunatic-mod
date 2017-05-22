using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheLunatic.Items;
using Utils;

namespace TheLunatic.Logic {
	public class MaskLogic {
		public static ISet<int> AllVanillaMasks { get; private set; }
		public static IDictionary<int, int> VanillaBossOfMask { get; private set; }
		public static int AvailableMaskCount { get; private set; }

		private TheLunaticMod Mod;

		public ISet<int> GivenVanillaMasksByType { get; private set; }
		public ISet<string> GivenCustomMasksByBossUid { get; private set; }
		

		////////////////

		static MaskLogic() {
			MaskLogic.AllVanillaMasks = new HashSet<int> {
				1281, // Skeletron Mask
				2104, // Brain of Cthulhu Mask
				2105, // Wall of Flesh Mask
				2106, // Twin Mask
				2107, // Skeletron Prime Mask
				2108, // Queen Bee Mask
				2109, // Plantera Mask
				2110, // Golem Mask
				2111, // Eater of Worlds Mask
				2112, // Eye of Cthulhu Mask
				2113, // Destroyer Mask
				2493, // King Slime Mask
				2588, // Duke Fishron Mask 	
				3372, // Ancient Cultist Mask
				3863, // Betsy Mask
				3373 // Moon Lord Mask
			};
			MaskLogic.VanillaBossOfMask = new Dictionary<int, int> {
				{ 4, 2112 },	// Eye of Cthulhu
				{ 50, 2493 },	// King Slime
				{ 222, 2108 },	// Queen Bee
				{ 13, 2111 },	// Eater of Worlds
				{ 14, 2111 },	// Eater of Worlds
				{ 15, 2111 },	// Eater of Worlds
				{ 266, 2104 },	// Brain of Cthulhu
				{ 35, 1281 },	// Skeletron
				{ 113, 2105 },	// Wall of Flesh
				{ 125, 2106 },	// The Twins
				{ 126, 2106 },	// The Twins
				{ 127, 2107 },	// Skeletron Prime
				{ 134, 2113 },	// The Destroyer (head)
				{ 245, 2110 },	// Golem (body)
				{ 262, 2109 },	// Plantera
				{ 370, 2588 },	// Duke Fishron
				{ 439, 3372 },	// Lunatic Cultist
				{ 551, 3863 },	// Betsy
				{ 398, 3373 }	// Moon Lord (core)
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
				return mask.name;
			}

			Mod mod = ModLoader.GetMod( "TheLunatic" );
			int custom_type = mod.ItemType<CustomBossMaskItem>();
			if( mask.type == custom_type ) {
				var info = mask.GetModInfo<CustomBossMaskItemInfo>( mod );
				if( info != null ) {
					return info.BossDisplayName + " Mask";
				}
			}

			return null;
		}

		////////////////

		public MaskLogic( TheLunaticMod mod ) {
			this.Mod = mod;
			this.GivenVanillaMasksByType = new HashSet<int>();
			this.GivenCustomMasksByBossUid = new HashSet<string>();
		}
		

		public void LoadOnce( int[] masks, string[] custom_masks ) {
			if( this.IsLoaded ) {
				DebugHelper.Log( "Redundant Mask Logic load. "+String.Join(",", masks)+" ("+String.Join(",", this.GivenVanillaMasksByType)+")" );
				return;
			}
			
			this.GivenVanillaMasksByType = new HashSet<int>( masks );
			this.GivenCustomMasksByBossUid = new HashSet<string>( custom_masks );

			if( (DebugHelper.DEBUGMODE & 4) > 0 ) {
				this.GivenVanillaMasksByType.Clear();
				this.GivenCustomMasksByBossUid.Clear();
			}
			if( (DebugHelper.DEBUGMODE & 8) > 0 ) {
				this.GivenVanillaMasksByType.Remove( 3373 );
			}
			this.IsLoaded = true;
		}
		private bool IsLoaded = false;


		////////////////

		public void RegisterReceiptOfMask( Player giving_player, int mask_type, int boss_type ) {
			if( mask_type == this.Mod.ItemType<CustomBossMaskItem>() ) {
				NPC npc = new NPC();
				npc.SetDefaults( boss_type );
				this.GivenCustomMasksByBossUid.Add( NPCHelper.GetUniqueId(npc) );
			} else {
				this.GivenVanillaMasksByType.Add( mask_type );
			}

			if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				DebugHelper.Log( "DEBUG Registering mask. "+giving_player.name+", "+mask_type );
			}

			// Buy time before the end comes
			if( this.GivenVanillaMasksByType.Count < (MaskLogic.AvailableMaskCount) ) {
				var modworld = this.Mod.GetModWorld<TheLunaticWorld>();
				int recovered = this.Mod.Config.Data.HalfDaysRecoveredPerMask;
				
				switch( mask_type ) {
				case 2105: // Wall of Flesh Mask
					recovered = (int)((float)recovered * this.Mod.Config.Data.WallOfFleshMultiplier);
					break;
				case 2113: // Destroyer Mask
				case 2106: // Twin Mask
				case 2107: // Skeletron Prime Mask
				case 2109: // Plantera Mask
				case 2110: // Golem Mask
				case 2588: // Duke Fishron Mask
				case 3372: // Ancient Cultist Mask
				case 3863: // Betsy Mask
				case 3373: // Moon Lord Mask
					if( mask_type == 3373 && this.Mod.Config.Data.MoonLordMaskWins ) {
						this.GiveAllVanillaMasks();
					}
					recovered = (int)((float)recovered * this.Mod.Config.Data.HardModeMultiplier);
					break;
				}

				if( MiscHelper.GetDayOrNightPercentDone() > 0.5f ) {
					recovered += 1;
				}
				
				modworld.GameLogic.SetTime( modworld.GameLogic.HalfDaysElapsed - recovered );
			}

			// Sky flash for all
			if( Main.netMode != 2 ) {  // Not server
				Player current_player = Main.player[Main.myPlayer];
				var modplayer = current_player.GetModPlayer<TheLunaticPlayer>( this.Mod );
				modplayer.FlashMe();
			}
		}

		public ISet<int> GetRemainingVanillaMasks() {
			ISet<int> masks = new HashSet<int>( MaskLogic.AllVanillaMasks.Where( x => !this.GivenVanillaMasksByType.Contains(x) ) );

			if( WorldGen.crimson ) {
				masks.Remove( 2111 );
			} else {
				masks.Remove( 2104 );
			}

			return masks;
		}

		public bool DoesLoonyHaveThisMask( Item mask_item ) {
			if( this.GetRemainingVanillaMasks().Contains(mask_item.type) ) { return true; }

			var mask_item_info = mask_item.GetModInfo<CustomBossMaskItemInfo>( this.Mod );
			return this.GivenCustomMasksByBossUid.Contains( mask_item_info.BossUid );
		}

		public void GiveAllVanillaMasks() {
			foreach( int mask_type in this.GetRemainingVanillaMasks() ) {
				this.GivenVanillaMasksByType.Add( mask_type );
			}
		}


		////////////////

		public bool IsValidMask( Item mask ) {
			var modworld = this.Mod.GetModWorld<TheLunaticWorld>();
			if( !modworld.GameLogic.HaveWeHopeToWin() ) { return false; }

			if( !this.Mod.Config.Data.LoonyAcceptsMasksWithoutBossKill ) {
				bool strict = this.Mod.Config.Data.LoonyEnforcesBossSequence;
				bool downed_mech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
				//bool downed_towers = NPC.downedTowerSolar && NPC.downedTowerVortex && NPC.downedTowerNebula && NPC.downedTowerStardust;

				switch( mask.type ) {
				case 2112: // Eye of Cthulhu Mask
					if( !NPC.downedBoss1 ) { return false; }
					break;
				case 2104: // Brain of Cthulhu Mask
					if( !NPC.downedBoss2 ) { return false; }
					break;
				case 2111: // Eater of Worlds Mask
					if( !NPC.downedBoss2 ) { return false; }
					break;
				case 1281: // Skeletron Mask
					if( !NPC.downedBoss3 ) { return false; }
					break;
				case 2493: // King Slime Mask
					if( !NPC.downedSlimeKing ) { return false; }
					break;
				case 2108: // Queen Bee Mask
					if( !NPC.downedQueenBee ) { return false; }
					break;
				case 2105: // Wall of Flesh Mask
					if( !Main.hardMode ) { return false; }
					break;
				case 2113: // Destroyer Mask
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss1 ) { return false; }
					break;
				case 2106: // Twin Mask
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss2 ) { return false; }
					break;
				case 2107: // Skeletron Prime Mask
					if( (strict && (!Main.hardMode)) || !NPC.downedMechBoss3 ) { return false; }
					break;
				case 2588: // Duke Fishron Mask
					if( (strict && (!Main.hardMode)) || !NPC.downedFishron ) { return false; }
					break;
				case 2109: // Plantera Mask
					if( (strict && (!Main.hardMode || !downed_mech)) || !NPC.downedPlantBoss ) { return false; }
					break;
				case 2110: // Golem Mask
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss)) || !NPC.downedGolemBoss ) { return false; }
					break;
				case 3863: // Betsy Mask
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ) { return false; }
					break;
				case 3372: // Ancient Cultist Mask
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss)) ||
						!NPC.downedAncientCultist ) { return false; }
					break;
				case 3373: // Moon Lord Mask
					if( (strict && (!Main.hardMode || !downed_mech || !NPC.downedPlantBoss || !NPC.downedGolemBoss || !NPC.downedAncientCultist)) ||
						!NPC.downedMoonlord ) { return false; }  //|| !downed_towers
					break;
				}
			}

			if( (WorldGen.crimson && mask.type == 2111) || (!WorldGen.crimson && mask.type == 2104) ) {
				return false;
			}

			return true;
		}

		public void GiveMaskToLoony( Player player, Item mask ) {
			if( Main.netMode == 1 ) {   // Client
				TheLunaticNetProtocol.SendGivenMaskFromClient( this.Mod, mask );
			} else if( Main.netMode == 0 ) {    // Single
				int boss_type = -1;
				if( mask.type == this.Mod.ItemType<CustomBossMaskItem>() ) {
					boss_type = mask.GetModInfo<CustomBossMaskItemInfo>( this.Mod ).BossNpcType;
				}
				this.RegisterReceiptOfMask( player, mask.type, boss_type );
			} else {
				throw new Exception( "Server should not be giving masks to loonys." );
			}

			mask.TurnToAir();

			Main.PlaySound( SoundID.Unlock, player.position );
		}
	}
}
