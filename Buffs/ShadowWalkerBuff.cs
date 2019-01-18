using HamstarHelpers.Helpers.PlayerHelpers;
using HamstarHelpers.Helpers.TileHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheLunatic.Buffs {
	class ShadowWalkerBuff : ModBuff {
		public static float Speed = 7f;
		public static int GrueETA = 60 * 7;
		public static int GrueOdds = 60 * 60;

		private static IDictionary<int, int> Grue = new Dictionary<int, int>();


		////////////////

		public static void AddBuffFor( Mod mod, Player player ) {
			player.AddBuff( mod.BuffType( "ShadowWalkerBuff" ), 4 );
		}

		public static int FindBuffIndex( Mod mod, Player player ) {
			return player.FindBuffIndex( mod.BuffType( "ShadowWalkerBuff" ) );
		}

		////////////////

		public static bool CanShadowWalk( Player player ) {
			float brightness = TileWorldHelpers.GaugeBrightnessWithin( (int)(player.position.X / 16f), (int)(player.position.Y / 16f), 2, 3 );
			return brightness <= 0.005;
		}


		////////////////

		public override void SetDefaults() {
			this.DisplayName.SetDefault( "Shadow Walker" );
			this.Description.SetDefault( "Move freely through the shadows" );

			Main.debuff[this.Type] = false;
			Main.buffNoTimeDisplay[this.Type] = true;
			Main.buffNoSave[this.Type] = true;

			this.canBeCleared = false;
		}

		public override void Update( Player player, ref int buff_idx ) {
			if( player.dead ) {
				this.End( player );
				return;
			}
			if( player.buffTime[buff_idx] > 1 ) {
				this.Run( player );
			} else {
				this.End( player );
			}
		}


		////////////////

		public bool? ProjectileCanUseGrapple( int type, Player player ) {
			if( player.FindBuffIndex( this.Type ) == -1 ) { return null; }

			return false;
		}

		public void PlayerPreItemCheck( Player player ) {
			if( player.FindBuffIndex( this.Type ) == -1 ) { return; }

			// Force item unselect
			if( !PlayerItemHelpers.UnhandItem(player) ) {
				player.DropSelectedItem();
			}
		}

		////////////////

		private void Run( Player player ) {
			var myplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
			if( myplayer.Noclip == null ) { return; }

			player.AddBuff( mod.BuffType("ShadowWalkerBuff"), 4 );

			myplayer.Noclip.On( player, ShadowWalkerBuff.Speed );

			if( !ShadowWalkerBuff.CanShadowWalk( player ) ) {
				// If holding shift, allow escape from darkness
				Keys[] pressedKeys = Main.keyState.GetPressedKeys();
				for( int i = 0; i < pressedKeys.Length; i++ ) {
					if( pressedKeys[i] == Keys.LeftShift || pressedKeys[i] == Keys.RightShift ) {
						this.End( player );
						break;
					}
				}
				// Otherwise, obey collision rules
				if( myplayer.Noclip.Collide() ) {
					this.End( player );
					return;
				}
			}

			myplayer.Noclip.UpdateMode( player );  // Redundant?

			var grue = ShadowWalkerBuff.Grue;
			if( (!grue.Keys.Contains(player.whoAmI) || grue[player.whoAmI] == 0) && Main.rand.Next(ShadowWalkerBuff.GrueOdds) == 0 ) {
				grue[player.whoAmI] = 1;
			}
			this.RunGrue( player );
		}


		public void End( Player player, bool safely=true ) {
			var grue = ShadowWalkerBuff.Grue;
			if( !safely && grue.Keys.Contains(player.whoAmI) && grue[player.whoAmI] > 0 ) {
				this.GrueEatsYou( player );
				return;
			}

			var myplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
			if( myplayer.Noclip != null ) {
				myplayer.Noclip.Off();
			}

			player.ClearBuff( mod.BuffType( "ShadowWalkerBuff" ) );
			grue[player.whoAmI] = 0;
		}


		////////////////

		private void RunGrue( Player player ) {
			var grue = ShadowWalkerBuff.Grue;
			if( !grue.Keys.Contains( player.whoAmI ) || grue[player.whoAmI] == 0 ) { return; }

			if( grue[player.whoAmI] == 1 ) {
				Vector2 pos = new Vector2( player.Center.X, player.Center.Y );
				pos.X += Main.rand.Next( -128, 128 );
				pos.Y += Main.rand.Next( -128, 128 );
				Main.PlaySound( SoundID.Zombie, pos, Main.rand.Next( 93, 97 ) );
			}

			if( grue[player.whoAmI] >= ShadowWalkerBuff.GrueETA ) {
				this.GrueEatsYou( player );
			} else {
				grue[player.whoAmI]++;
			}
		}

		private void GrueEatsYou( Player player ) {
			player.immune = false;
			player.Hurt( PlayerDeathReason.ByCustomReason( " was eaten by a grue." ), 999, 0 );
			ShadowWalkerBuff.Grue[ player.whoAmI ] = 0;

			this.End( player );
		}
	}
}
