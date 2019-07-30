using HamstarHelpers.Helpers.Debug;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Logic;


namespace TheLunatic {
	class TheLunaticWorld : ModWorld {
		public string ID { get; private set; }
		public bool HasCorrectID { get; private set; }	// Workaround for tml bug?

		public GameLogic GameLogic { get; private set; }
		public MaskLogic MaskLogic { get; private set; }
		


		////////////////
		
		public override void Initialize() {
			var mymod = (TheLunaticMod)this.mod;

			this.ID = Guid.NewGuid().ToString( "D" );
			this.HasCorrectID = false;  // 'Load()' decides if no pre-existing one is found

			this.GameLogic = new GameLogic();
			this.MaskLogic = new MaskLogic();

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG World created; logics (re)created." );
			}
		}

		////////////////

		public override void Load( TagCompound tag ) {
			var mymod = (TheLunaticMod)this.mod;
			bool hasArrived = false, hasQuit = false, hasEnd = false, hasWon = false, isSafe = false;
			int time = 0;
			int[] masks = new int[0];
			int customMaskCount = 0;
			string[] customMasks = new string[0];

			if( tag.ContainsKey("world_id") ) {
				this.ID = tag.GetString( "world_id" );

				hasArrived = tag.GetBool( "has_loony_arrived" );
				hasQuit = tag.GetBool( "has_loony_quit" );
				hasEnd = tag.GetBool( "has_game_ended" );
				hasWon = tag.GetBool( "has_won" );
				isSafe = tag.GetBool( "is_safe" );

				time = tag.GetInt( "half_days_elapsed_" + this.ID );

				masks = tag.GetIntArray( "masks_given_" + this.ID );

				customMaskCount = tag.GetInt( "custom_masks_given_" + this.ID );
				customMasks = new string[customMaskCount];
				for( int i = 0; i < customMaskCount; i++ ) {
					customMasks[i] = tag.GetString( "custom_mask_" + this.ID + "_" + i );
				}
			}

			this.HasCorrectID = true;

			this.GameLogic.LoadOnce( hasArrived, hasQuit, hasEnd, hasWon, isSafe, time );
			this.MaskLogic.LoadOnce( masks, customMasks );
		}

		public override TagCompound Save() {
			var mymod = (TheLunaticMod)this.mod;
			var tag = new TagCompound {
				{ "world_id", this.ID },
				{ "has_loony_arrived", (bool)this.GameLogic.HasLoonyArrived },
				{ "has_loony_quit", (bool)this.GameLogic.HasLoonyQuit },
				{ "has_game_ended", (bool)this.GameLogic.HasGameEnded },
				{ "has_won", (bool)this.GameLogic.HasWon },
				{ "is_safe", (bool)this.GameLogic.IsSafe },
				{ "half_days_elapsed_" + this.ID, (int)this.GameLogic.HalfDaysElapsed },
				{ "masks_given_" + this.ID, (int[])this.MaskLogic.GivenVanillaMasksByType.ToArray() },
				{ "custom_masks_given_" + this.ID, (int)this.MaskLogic.GivenCustomMasksByBossUid.Count }
			};

			int i = 0;
			foreach( string maskId in this.MaskLogic.GivenCustomMasksByBossUid ) {
				tag.Set( "custom_mask_" + this.ID + "_" + i, (string)maskId );
				i++;
			}

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG Saving world. " + this.ID + ", "
					+ this.GameLogic.HasLoonyArrived + ", "
					+ this.GameLogic.HasLoonyQuit + ", "
					+ this.GameLogic.HasGameEnded + ", "
					+ this.GameLogic.HasWon + ", "
					+ this.GameLogic.IsSafe + ", "
					+ this.GameLogic.HalfDaysElapsed + ", "
					+ "[" + String.Join( ",", this.MaskLogic.GivenVanillaMasksByType.ToArray() ) + "], " );
			}

			return tag;
		}

		////////////////

		public override void NetSend( BinaryWriter writer ) {
			writer.Write( this.HasCorrectID );
			writer.Write( this.ID );
		}

		public override void NetReceive( BinaryReader reader ) {
			bool hasCorrectId = reader.ReadBoolean();
			string id = reader.ReadString();

			if( hasCorrectId ) {
				this.ID = id;
				this.HasCorrectID = true;
			}
		}

		////////////////

		public override void PreUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			if( Main.netMode == 2 ) { // Server only
				if( this.HasCorrectID && this.GameLogic != null ) {
					this.GameLogic.Update();
				}
			}
		}
	}
}
