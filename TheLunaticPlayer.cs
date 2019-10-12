using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Players;
using Microsoft.Xna.Framework.Graphics;
using PlayerExtend;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Buffs;
using TheLunatic.Items;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		public IDictionary<string, bool> Bye { get; private set; }
		public PlayerNoclip Noclip { get; private set; }
		
		public bool HasVerifiedGameData { get; private set; }
		public bool IsInDangerZone { get; private set; }

		private float QuakeScale = 1f;
		private int QuakeDuration = 0;
		private int QuakeStartDuration = 0;

		public Texture2D MaskTex = null;


		////////////////

		public override bool CloneNewInstances => false;

		public override void Initialize() {
			this.Noclip = new PlayerNoclip();
			this.Bye = new Dictionary<string, bool>();
			this.HasVerifiedGameData = false;
			this.IsInDangerZone = false;
		}

		public override void clientClone( ModPlayer clone ) {
			var myclone = (TheLunaticPlayer)clone;
			myclone.Bye = this.Bye;
			myclone.Noclip = this.Noclip;
			myclone.HasVerifiedGameData = this.HasVerifiedGameData;
			myclone.QuakeScale = this.QuakeScale;
			myclone.QuakeDuration = this.QuakeDuration;
			myclone.QuakeStartDuration = this.QuakeStartDuration;
			myclone.MaskTex = this.MaskTex;
		}


		////////////////

		public override void SyncPlayer( int toWho, int fromWho, bool newPlayer ) {
			var mymod = (TheLunaticMod)this.mod;

			if( Main.netMode == 2 ) {
				if( toWho == -1 && fromWho == this.player.whoAmI ) {
					this.OnServerConnect( this.player );
				}
			}
		}

		public override void OnEnterWorld( Player enteringPlayer ) {
			if( enteringPlayer.whoAmI != Main.myPlayer ) { return; }
			if( this.player.whoAmI != Main.myPlayer ) { return; }

			var mymod = (TheLunaticMod)this.mod;

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Alert( enteringPlayer.name + " joined (" + PlayerIdentityHelpers.GetUniqueId( enteringPlayer ) + ")" );
			}

			if( Main.netMode == 0 ) {
				this.OnSingleConnect();
			}
			if( Main.netMode == 1 ) {
				this.OnClientConnect( enteringPlayer );
			}
		}

		////////////////

		public override void Load( TagCompound tags ) {
			var mymod = (TheLunaticMod)this.mod;

			try {
				int worlds = tags.GetInt( "world_count" );
				this.Bye = new Dictionary<string, bool>();

				for( int i = 0; i < worlds; i++ ) {
					string worldId = tags.GetString( "world_id_" + i );
					this.Bye[worldId] = tags.GetBool( "bye_" + i );
				}

				if( mymod.Config.DebugModeInfo ) {
					LogHelpers.Log( "DEBUG Load player. {" +
						string.Join( ";", this.Bye.Select( x => x.Key + "=" + x.Value ).ToArray() ) + "}" );
				}
			} catch( Exception e ) {
				LogHelpers.Log( e.ToString() );
			}
		}

		public override TagCompound Save() {
			var mymod = (TheLunaticMod)this.mod;
			var tags = new TagCompound { { "world_count", this.Bye.Count } };
			int i = 0;

			foreach( var kv in this.Bye ) {
				string worldId = kv.Key;
				tags.Set( "world_id_" + i, worldId );
				tags.Set( "bye_" + i, this.Bye[worldId] );
				i++;
			}

			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG Save player. {" +
					string.Join( ";", this.Bye.Select( x => x.Key + "=" + x.Value ).ToArray() ) + "}" );
			}

			return tags;
		}


		////////////////

		public override bool PreItemCheck() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Enabled ) { base.PreItemCheck(); }

try {
			// Force de-select of items while shadow walking
			if( this.player.HeldItem != null && this.player.HeldItem.type > 0 ) {
				var buff = (ShadowWalkerBuff)mymod.GetBuff( "ShadowWalkerBuff" );
				buff.PlayerPreItemCheck( this.player );
			}

			UmbralCowlItem.CheckEquipState( this.player );
} catch( Exception e ) {
	throw new ModHelpersException( "", e );
}

			return base.PreItemCheck();
		}


		////////////////

		public bool IsCheater() {
			var myworld = ModContent.GetInstance<TheLunaticWorld>();
			if( !this.Bye.Keys.Contains( myworld.ID ) ) { return false; }
			return this.Bye[myworld.ID];
		}

		////////////////

		public void SetCheater() {
			var myworld = ModContent.GetInstance<TheLunaticWorld>();
			this.Bye[myworld.ID] = true;
		}
	}
}
