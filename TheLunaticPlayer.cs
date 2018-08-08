using HamstarHelpers.Helpers.DebugHelpers;
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
using TheLunatic.NetProtocol;


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

		public override bool CloneNewInstances { get { return false; } }

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

		public override void OnEnterWorld( Player player ) {    // This seems to be as close to a constructor as we're gonna get!
			if( player.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var modworld = mymod.GetModWorld<TheLunaticWorld>();

				if( Main.netMode != 2 ) {   // Not server
					if( !mymod.ConfigJson.LoadFile() ) {
						mymod.ConfigJson.SaveFile();
					}
				}
				modworld.GameLogic.ApplyDebugOverrides( mymod );

				if( Main.netMode == 1 ) {	// Client
					ClientPacketHandlers.SendRequestModSettingsFromClient( mymod );
					ClientPacketHandlers.SendRequestModDataFromClient( mymod );
				} else if( Main.netMode == 0 ) {	// Single
					this.PostEnterWorld();
				}
			}
		}

		public void PostEnterWorld() {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = mymod.GetModWorld<TheLunaticWorld>();

			if( modworld.GameLogic.HasGameEnded && !modworld.GameLogic.HasWon ) {
				Main.NewText( "You inexplicably feel like this will now be a boring adventure.", 64, 64, 96, false );
			}

			this.HasVerifiedGameData = true;
		}

		////////////////

		public override void Load( TagCompound tags ) {
			var mymod = (TheLunaticMod)this.mod;

			try {
				int worlds = tags.GetInt( "world_count" );
				this.Bye = new Dictionary<string, bool>();

				for( int i = 0; i < worlds; i++ ) {
					string world_id = tags.GetString( "world_id_" + i );
					this.Bye[world_id] = tags.GetBool( "bye_" + i );
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
				string world_id = kv.Key;
				tags.Set( "world_id_" + i, world_id );
				tags.Set( "bye_" + i, this.Bye[world_id] );
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
			if( !mymod.ConfigJson.Data.Enabled ) { base.PreItemCheck(); }

			// Force de-select of items while shadow walking
			if( this.player.HeldItem != null && this.player.HeldItem.type > 0 ) {
				var buff = (ShadowWalkerBuff)this.mod.GetBuff( "ShadowWalkerBuff" );
				buff.PlayerPreItemCheck( this.player );
			}

			UmbralCowlItem.CheckEquipState( this.mod, this.player );

			return base.PreItemCheck();
		}


		////////////////

		public bool IsCheater() {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( !this.Bye.Keys.Contains( modworld.ID ) ) { return false; }
			return this.Bye[modworld.ID];
		}

		////////////////

		public void SetCheater() {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			this.Bye[modworld.ID] = true;
		}
	}
}
