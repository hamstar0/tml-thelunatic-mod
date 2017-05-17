using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Logic;
using Utils;


namespace TheLunatic {
	class TheLunaticWorld : ModWorld {
		public string ID { get; private set; }
		public GameLogic GameLogic { get; private set; }
		public MaskLogic MaskLogic { get; private set; }



		////////////////

		public TheLunaticWorld() : base() {
			this.ID = Guid.NewGuid().ToString( "D" );
			this.GameLogic = null;
			this.MaskLogic = null;
		}

		public override void Initialize() {
			this.GameLogic = new GameLogic( (TheLunaticMod)this.mod );
			this.MaskLogic = new MaskLogic( (TheLunaticMod)this.mod );
			this.IsIDLoaded = false;

			if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				DebugHelper.Log( "DEBUG World created; logics (re)created." );
			}
		}

		public void LoadOnce( string id ) {
			if( this.IsIDLoaded ) {
				DebugHelper.Log( "Redundant world ID load. "+id+" ("+ this.ID+")" );
				return;
			}
			this.IsIDLoaded = true;

			this.ID = id;
		}
		private bool IsIDLoaded = false;

		////////////////

		public override void Load( TagCompound tag ) {
			try {
				var mymod = (TheLunaticMod)this.mod;

				string world_id =		tag.GetString(		"world_id" );
				bool has_arrived =		tag.GetBool(		"has_loony_arrived" );
				bool has_quit =			tag.GetBool(		"has_loony_quit" );
				bool has_end =			tag.GetBool(		"has_game_ended" );
				bool has_won =			tag.GetBool(		"has_won" );
				bool is_safe =			tag.GetBool(		"is_safe" );
				int time =				tag.GetInt(			"half_days_elapsed_" + world_id );
				int[] masks =			tag.GetIntArray(	"masks_given_" + world_id );
				int custom_mask_count = tag.GetInt(			"custom_masks_given_" + world_id );
				string[] custom_masks = new string[ custom_mask_count ];
				for( int i = 0; i < custom_mask_count; i++ ) {
					custom_masks[i] = tag.GetString( "custom_mask_" + world_id + "_" + i );
				}

				this.LoadOnce( world_id );
				this.GameLogic.LoadOnce( has_arrived, has_quit, has_end, has_won, is_safe, time );
				this.MaskLogic.LoadOnce( masks, custom_masks );
			} catch( Exception e ) {
				DebugHelper.Log( "Load data out of sync. "+e.ToString() );
			}
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

			if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				DebugHelper.Log( "DEBUG Saving world. "+ this.ID + ", "
					+ this.GameLogic.HasLoonyArrived + ", "
					+ this.GameLogic.HasLoonyQuit + ", "
					+ this.GameLogic.HasGameEnded + ", "
					+ this.GameLogic.HasWon + ", "
					+ this.GameLogic.IsSafe + ", "
					+ this.GameLogic.HalfDaysElapsed + ", "
					+ "["+String.Join(",", this.MaskLogic.GivenVanillaMasksByType.ToArray()) + "], " );
			}

			return tag;
		}

		////////////////

		public override void PreUpdate() {
			if( Main.netMode != 2 ) { return; } // Server only

			if( this.GameLogic != null ) {
				this.GameLogic.Update();
			}
		}
	}
}
