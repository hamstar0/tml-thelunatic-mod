using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.WorldHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;


namespace TheLunatic.Tiles {
	class AnimatedSky : CustomSky {
		public int SkyFlashMaxDuration = 64;
		public int SkyFlash { get; private set; }
		public int SkyFlashDuration { get; private set; }

		public float TintScale = 0f;

		public int Shade { get; private set; }
		private bool IsNowActive = false;


		////////////////

		public void UpdateSky( Player player ) {
			if( this.SkyFlashDuration > 0 ) {
				this.SkyFlashDuration--;
			} else {
				this.SkyFlash = 0;
			}

			if( !this.IsActive() ) {
				SkyManager.Instance.Activate( "TheLunaticMod:AnimatedColorize", player.Center, new object[0] );
			}
			//bool can_sky = this.SkyFlash != 0 || this.TintScale > 0;
			//
			//if( can_sky != this.IsActive() ) {
			//	if( can_sky ) {
			//		SkyManager.Instance.Activate( "TheLunaticMod:AnimatedColorize", player.Center, new object[0] );
			//	} else {
			//		SkyManager.Instance.Deactivate( "TheLunaticMod:AnimatedColorize", new object[0] );
			//	}
			//}
		}

		////////////////

		public void LightenSky() {
			this.SkyFlash = 1;
			this.SkyFlashDuration = this.SkyFlashMaxDuration;
		}

		public void DarkenSky() {
			this.SkyFlash = -1;
			this.SkyFlashDuration = this.SkyFlashMaxDuration;
		}

		////////////////

		public Color GetFlashColor() {
			Color color = Color.Black;

			if( this.SkyFlash > 0 ) {
				color.R = color.G = color.B = color.A = (byte)this.Shade;
			} else if( this.SkyFlash < 0 ) {
				color.A = (byte)this.Shade;
			}

			return color;
		}

		public Color GetTintColor() {
			var mymod = TheLunaticMod.Instance;
			Color color = Color.Black;
			float tintScale = MathHelper.Clamp( this.TintScale, 0f, 1f );
			float daySpike = (float)Math.Abs( WorldStateHelpers.GetDayOrNightPercentDone() - 0.5d );

			if( Main.dayTime ) {
				tintScale *= 1f - daySpike;
			} else {
				tintScale *= (daySpike * 0.6f) + 0.2f;
			}

			color.R = (byte)(255f * tintScale);
			color.G = (byte)(128f * tintScale);
			color.A = (byte)(255f * tintScale);

			if( mymod.Config.DebugModeInfo ) {
				DebugHelpers.Print( "Sky", color.ToString(), 20 );
			}
			return color;
		}


		////////////////

		public override void OnLoad() { }


		public override void Update( GameTime gameTime ) {
			int len = this.SkyFlashMaxDuration / 2;

			if( this.SkyFlash != 0 ) {
				this.Shade = Math.Abs( len - this.SkyFlashDuration );
				this.Shade = (int)((float)(len - this.Shade) * (255f / (float)len));
			}
		}

		//public override Color OnTileColor( Color color ) {
		//	float scale = (float)this.Shade / 255f;
		//	if( TheLunaticWorld.SkyFlash > 0 ) {
		//		color.R = (byte)(color.R + ((float)(255 - color.R) * scale));
		//		color.G = (byte)(color.G + ((float)(255 - color.G) * scale));
		//		color.B = (byte)(color.B + ((float)(255 - color.B) * scale));
		//	} else if( TheLunaticWorld.SkyFlash < 0 ) {
		//		color.R = (byte)((float)color.R * scale);
		//		color.G = (byte)((float)color.G * scale);
		//		color.B = (byte)((float)color.B * scale);
		//	}
		//	return color;
		//}
		
		public override void Draw( SpriteBatch sb, float minDepth, float maxDepth ) {
			if( this.TintScale > 0f ) {
				Color color = this.GetTintColor();

				if( minDepth >= 6f ) {
					sb.Draw( Main.blackTileTexture, new Rectangle( 0, 0, Main.screenWidth, Main.screenHeight ), color );
				}
			}

			if( this.SkyFlash != 0 ) {  // && max_depth >= 0f && min_depth < 0f
				Color color = this.GetFlashColor();

				sb.Draw( Main.blackTileTexture, new Rectangle( 0, 0, Main.screenWidth, Main.screenHeight ), color );
			}
		}

		//public override float GetCloudAlpha() {
		//	return 1f - (float)this.Shade / 255f;
		//}

		public override void Activate( Vector2 position, params object[] args ) {
			this.IsNowActive = true;
		}
		public override void Deactivate( params object[] args ) {
			this.IsNowActive = false;
		}
		public override void Reset() {
			this.IsNowActive = false;
		}
		public override bool IsActive() {
			return this.IsNowActive;
		}
	}
}
