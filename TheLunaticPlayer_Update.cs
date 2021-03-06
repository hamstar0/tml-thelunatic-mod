﻿using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
using System;
using Terraria;
using Terraria.ModLoader;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		public override void PreUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }
			var myworld = ModContent.GetInstance<TheLunaticWorld>();

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
	throw new ModHelpersException( "", e );
}
		}


		public override void PostUpdate() {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			if( Main.netMode == 2 || this.player.whoAmI == Main.myPlayer ) {   // Server or current player only
				if( this.Noclip != null ) {
try {
					this.Noclip.UpdateMovement( this.player );
} catch( Exception e ) {
	throw new ModHelpersException( "", e );
}
				}
			}
		}
	}
}
