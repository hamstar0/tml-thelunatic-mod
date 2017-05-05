using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayerExtend;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Buffs;
using TheLunatic.Items;
using Utils;


namespace TheLunatic {
	public class TheLunaticPlayer : ModPlayer {
		public IDictionary<string, bool> Bye { get; private set; }
		public PlayerNoclip Noclip { get; private set; }

		public bool HasEnteredWorld { get; private set; }
		public bool IsInDangerZone { get; private set; }

		private float QuakeScale = 1f;
		private int QuakeDuration = 0;
		private int QuakeStartDuration = 0;

		public Texture2D MaskTex = null;
		

		////////////////

		public override void Initialize() {
			if( this.Noclip == null ) {
				this.Noclip = new PlayerNoclip();
			}
			if( this.Bye == null ) {
				this.Bye = new Dictionary<string, bool>();
			}
			base.Initialize();
		}

		public override void clientClone( ModPlayer clone ) {
			var myclone = (TheLunaticPlayer)clone;
			myclone.Bye = this.Bye;
			myclone.Noclip = this.Noclip;
			myclone.HasEnteredWorld = this.HasEnteredWorld;
			myclone.QuakeScale = this.QuakeScale;
			myclone.QuakeDuration = this.QuakeDuration;
			myclone.QuakeStartDuration = this.QuakeStartDuration;
			myclone.MaskTex = this.MaskTex;
		}
		
		public override void OnEnterWorld( Player player ) {    // This seems to be as close to a constructor as we're gonna get!
			if( player.whoAmI == this.player.whoAmI ) {    // Current player
				if( Main.netMode != 2 ) {   // Not server
					var mymod = (TheLunaticMod)this.mod;
					if( !mymod.Config.LoadFile() ) {
						mymod.Config.SaveFile();
					}
				}

				if( Main.netMode == 1 ) {	// Client
					TheLunaticNetProtocol.SendRequestModSettingsFromClient( this.mod );
					TheLunaticNetProtocol.SendRequestModDataFromClient( this.mod );
				}
			}
			
			this.HasEnteredWorld = true;
		}

		////////////////

		public override void Load( TagCompound tags ) {
			try {
				int worlds = tags.GetInt( "world_count" );
				this.Bye = new Dictionary<string, bool>();

				for( int i = 0; i < worlds; i++ ) {
					string world_id = tags.GetString( "world_id_" + i );
					this.Bye[world_id] = tags.GetBool( "bye_" + i );
				}

				if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
					DebugHelper.Log( "DEBUG Load player. {" +
						string.Join( ";", this.Bye.Select( x => x.Key + "=" + x.Value ).ToArray() ) + "}" );
				}
			} catch( Exception e ) {
				DebugHelper.Log( e.ToString() );
			}
		}

		public override TagCompound Save() {
			var tags = new TagCompound { { "world_count", this.Bye.Count } };
			int i = 0;

			foreach( var kv in this.Bye ) {
				string world_id = kv.Key;
				tags.SetTag( "world_id_" + i, world_id );
				tags.SetTag( "bye_" + i, this.Bye[world_id] );
				i++;
			}

			if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				DebugHelper.Log( "DEBUG Save player. {" +
					string.Join(";", this.Bye.Select(x => x.Key + "=" + x.Value).ToArray()) + "}" );
			}

			return tags;
		}


		////////////////

		public override void PreUpdate() {
			if( this.player.position.Y < Main.worldSurface * 16.0 ) {
				this.IsInDangerZone = true;
			} else {
				this.IsInDangerZone = false;
			}

			if( this.QuakeDuration > 0 ) {
				this.QuakeDuration--;
			}

			if( Main.netMode == 2 || this.player.whoAmI == Main.myPlayer ) {   // Server or current player only
				if( this.Noclip != null ) {
					this.Noclip.UpdateMode( this.player );
				}
			}

			if( Main.netMode != 2 ) {	// Not server
				if( this.player.whoAmI == Main.myPlayer ) { // Current player only
					if( this.HasEnteredWorld ) {
						var modworld = this.mod.GetModWorld<TheLunaticWorld>();
						modworld.GameLogic.Update();
					}
				}
			} else {	// Server
				var modworld = this.mod.GetModWorld<TheLunaticWorld>();
				modworld.GameLogic.ReadyServer = true;	// Ugh!
			}
		}


		public override void PostUpdate() {
			if( Main.netMode == 2 || this.player.whoAmI == Main.myPlayer ) {   // Server or current player only
				if( this.Noclip != null ) {
					this.Noclip.UpdateMovement( this.player );
				}
			}
		}


		public override bool PreItemCheck() {
			// Force de-select of items while shadow walking
			if( this.player.HeldItem != null && this.player.HeldItem.type > 0 ) {
				var buff = (ShadowWalkerBuff)this.mod.GetBuff( "ShadowWalkerBuff" );
				buff.PlayerPreItemCheck( this.player );
			}

			UmbralCowlItem.CheckEquipState( this.mod, this.player );

			return base.PreItemCheck();
		}

		////////////////

		public override void ModifyScreenPosition() {
			if( this.QuakeDuration != 0 ) {
				float quake_scale = this.QuakeScale;
				float duration_range = this.QuakeStartDuration > 0f ? this.QuakeStartDuration : this.QuakeStartDuration * 2f;

				if( this.player.velocity.Y != 0 ) {
					quake_scale /= 8;
				} else {
					this.player.AddBuff( 32, 2 );
				}

				float progress = (float)this.QuakeDuration / duration_range;
				float spike = 1f - (Math.Abs(progress - 0.5f) * 2f);
				float full_spike = spike * 32f;
				float shake_range = full_spike * quake_scale;

//Debug.Display["quake"] = "progress: "+progress+" spike: "+spike+" full_spike: "+full_spike+" quake_scale: "+quake_scale;
				Main.screenPosition.X += (shake_range/2) - (Main.rand.NextFloat() * shake_range);
				Main.screenPosition.Y += (shake_range/2) - (Main.rand.NextFloat() * shake_range);
			}
		}

		public override void UpdateBiomes() {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			modworld.GameLogic.UpdateBiomes( this.player );
		}

		public override void UpdateBiomeVisuals() {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			mymod.Sky.UpdateSky( this.player );
			modworld.GameLogic.UpdateBiomeVisuals( this.player );
		}


		public static readonly PlayerLayer CustomBossMask = new PlayerLayer(
			"TheLunatic", "CustomBossMask", PlayerLayer.MiscEffectsFront,
			delegate ( PlayerDrawInfo draw_info ) {
				Player player = draw_info.drawPlayer;

				var mod = ModLoader.GetMod( "TheLunatic" );
				var modplayer = player.GetModPlayer<TheLunaticPlayer>( mod );
				var tex = modplayer.MaskTex;

				var se = player.direction != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				var pos = player.position;
				var rel_pos = pos - Main.screenPosition;
				rel_pos.Y -= 8;
				int x = (int)((pos.X + (player.width / 2f)) / 16f);
				int y = (int)((pos.Y + (player.headFrame.Height / 2f)) / 16f);
				Color light_color = Lighting.GetColor( x, y );
				DrawData data = new DrawData( tex, rel_pos, new Rectangle(0, 0, tex.Width, tex.Height), light_color, 0f, new Vector2(), 1f, se, 0 );
				Main.playerDrawData.Add( data );
			}
		);

		public override void ModifyDrawLayers( List<PlayerLayer> layers ) {
			this.MaskTex = CustomBossMaskItem.GetMaskTextureOfPlayer( this.player, this.mod );
			TheLunaticPlayer.CustomBossMask.visible = this.MaskTex != null && !this.player.dead;
			layers.Add( TheLunaticPlayer.CustomBossMask );
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

		public void FlashMe() {
			var mymod = (TheLunaticMod)this.mod;
			mymod.Sky.LightenSky();
		}

		public void QuakeMeFor( int duration, float scale ) {
			this.QuakeScale = scale;
			this.QuakeDuration = duration;
			this.QuakeStartDuration = duration;

			int sound_type = SoundLoader.customSoundType;
			float vol = scale / 2;
			int sound_slot = this.mod.GetSoundSlot( SoundType.Custom, "Sounds/Custom/EarthQuake" );
			Main.PlaySound( sound_type, -1, -1, sound_slot, vol );
		}
	}
}
