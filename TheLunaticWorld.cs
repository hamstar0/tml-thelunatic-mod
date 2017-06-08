using HamstarHelpers.MiscHelpers;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Logic;


namespace TheLunatic {
	public class TheLunaticWorld : ModWorld {
		public string ID { get; private set; }
		public bool HasCorrectID { get; private set; }	// Workaround for tml bug?

		public GameLogic GameLogic { get; private set; }
		public MaskLogic MaskLogic { get; private set; }
		


		////////////////
		
		public override void Initialize() {
			var mymod = (TheLunaticMod)this.mod;

			this.ID = Guid.NewGuid().ToString( "D" );
			this.HasCorrectID = false;  // 'Load()' decides if no pre-existing one is found

			this.GameLogic = new GameLogic( mymod );
			this.MaskLogic = new MaskLogic( mymod );

			if( (TheLunaticMod.DEBUGMODE & 1) > 0 ) {
				DebugHelpers.Log( "DEBUG World created; logics (re)created." );
			}
		}

		////////////////

		public override void Load( TagCompound tag ) {
			var mymod = (TheLunaticMod)this.mod;
			bool has_arrived = false, has_quit = false, has_end = false, has_won = false, is_safe = false;
			int time = 0;
			int[] masks = new int[0];
			int custom_mask_count = 0;
			string[] custom_masks = new string[0];

			if( tag.ContainsKey("world_id") ) {
				this.ID = tag.GetString( "world_id" );

				has_arrived = tag.GetBool( "has_loony_arrived" );
				has_quit = tag.GetBool( "has_loony_quit" );
				has_end = tag.GetBool( "has_game_ended" );
				has_won = tag.GetBool( "has_won" );
				is_safe = tag.GetBool( "is_safe" );

				time = tag.GetInt( "half_days_elapsed_" + this.ID );

				masks = tag.GetIntArray( "masks_given_" + this.ID );

				custom_mask_count = tag.GetInt( "custom_masks_given_" + this.ID );
				custom_masks = new string[custom_mask_count];
				for( int i = 0; i < custom_mask_count; i++ ) {
					custom_masks[i] = tag.GetString( "custom_mask_" + this.ID + "_" + i );
				}
			}

			this.HasCorrectID = true;

			this.GameLogic.LoadOnce( has_arrived, has_quit, has_end, has_won, is_safe, time );
			this.MaskLogic.LoadOnce( masks, custom_masks );
		}

		public override TagCompound Save() {
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
			foreach( string mask_id in this.MaskLogic.GivenCustomMasksByBossUid ) {
				tag.Set( "custom_mask_" + this.ID + "_" + i, (string)mask_id );
				i++;
			}

			if( (TheLunaticMod.DEBUGMODE & 1) > 0 ) {
				DebugHelpers.Log( "DEBUG Saving world. " + this.ID + ", "
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
			bool has_correct_id = reader.ReadBoolean();
			string id = reader.ReadString();

			if( has_correct_id ) {
				this.ID = id;
				this.HasCorrectID = true;
			}
		}

		////////////////

		public override void PreUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			if( Main.netMode == 2 ) { // Server only
				if( this.HasCorrectID && this.GameLogic != null ) {
					this.GameLogic.Update();
				}
			}
		}
	}
}
