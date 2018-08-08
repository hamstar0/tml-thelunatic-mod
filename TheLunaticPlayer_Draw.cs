using HamstarHelpers.Helpers.DebugHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		public override void ModifyScreenPosition() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

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
			var mymod = (TheLunaticMod)this.mod;
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();

			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			modworld.GameLogic.UpdateBiomes( mymod, this.player );
		}

		public override void UpdateBiomeVisuals() {
			var mymod = (TheLunaticMod)this.mod;
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();

			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			mymod.Sky.UpdateSky( this.player );
			modworld.GameLogic.UpdateBiomeVisuals( mymod, this.player );
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
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

			this.MaskTex = CustomBossMaskItem.GetMaskTextureOfPlayer( this.player, mymod );
			if( this.MaskTex != null ) {
				TheLunaticPlayer.CustomBossMask.visible = !this.player.dead;
				layers.Add( TheLunaticPlayer.CustomBossMask );
			}
		}



		////////////////

		public void FlashMe() {
			var mymod = (TheLunaticMod)this.mod;
			mymod.Sky.LightenSky();
		}

		public void QuakeMeFor( int duration, float scale ) {
			this.QuakeScale = scale;
			this.QuakeDuration = duration;
			this.QuakeStartDuration = duration;
			
			float vol = scale / 2;
			int sound_slot = this.mod.GetSoundSlot( SoundType.Custom, "Sounds/Custom/EarthQuake" );
			Main.PlaySound( (int)SoundType.Custom, -1, -1, sound_slot, vol );
		}
	}
}
