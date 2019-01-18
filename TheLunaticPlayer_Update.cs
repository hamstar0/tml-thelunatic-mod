using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.DebugHelpers;
using System;
using Terraria;
using Terraria.ModLoader;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		public override void PreUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();

			if( this.player.position.Y < Main.worldSurface * 16.0 ) {
				this.IsInDangerZone = true;
			} else {
				this.IsInDangerZone = false;
			}

			if( this.QuakeDuration > 0 ) {
				this.QuakeDuration--;
			}

try {
			if( Main.netMode == 2 || this.player.whoAmI == Main.myPlayer ) {   // Server or current player only
				if( this.Noclip != null ) {
					this.Noclip.UpdateMode( this.player );
				}
			}


			if( Main.netMode != 2 ) {   // Not server
				if( myworld.HasCorrectID && this.HasVerifiedGameData ) {
					if( this.player.whoAmI == Main.myPlayer ) { // Current player only
						myworld.GameLogic.Update();
					}
				}
			} else {    // Server
				myworld.GameLogic.ReadyServer = true;  // Needed?
			}
} catch( Exception e ) {
	throw new HamstarException( "!TheLunatic.TheLunaticPlayer.PreUpdate", e );
}
		}


		public override void PostUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

			if( Main.netMode == 2 || this.player.whoAmI == Main.myPlayer ) {   // Server or current player only
				if( this.Noclip != null ) {
try {
					this.Noclip.UpdateMovement( this.player );
} catch( Exception e ) {
	throw new HamstarException( "!TheLunatic.TheLunaticPlayer.PostUpdate", e );
}
				}
			}
		}
	}
}
