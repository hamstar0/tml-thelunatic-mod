using HamstarHelpers.Helpers.DebugHelpers;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.NetProtocol;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		private void OnSingleConnect() {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();

			myworld.GameLogic.ApplyDebugOverrides( mymod );

			this.PostEnterWorld();
		}

		private void OnClientConnect( Player clientPlr ) {
			if( clientPlr.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();

				myworld.GameLogic.ApplyDebugOverrides( mymod );

				ClientPacketHandlers.SendRequestModSettingsFromClient();
				ClientPacketHandlers.SendRequestModDataFromClient();
			}
		}

		private void OnServerConnect( Player clientPlr ) {
			if( clientPlr.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();

				myworld.GameLogic.ApplyDebugOverrides( mymod );
			}
		}


		public void PostEnterWorld() {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();

			if( myworld.GameLogic.HasGameEnded && !myworld.GameLogic.HasWon ) {
				Main.NewText( "You inexplicably feel like this will now be a boring adventure.", 64, 64, 96, false );
			}

			this.HasVerifiedGameData = true;
		}
	}
}
