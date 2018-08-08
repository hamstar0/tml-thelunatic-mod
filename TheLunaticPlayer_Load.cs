using HamstarHelpers.Helpers.DebugHelpers;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.NetProtocol;


namespace TheLunatic {
	partial class TheLunaticPlayer : ModPlayer {
		private void OnSingleConnect() {
			if( player.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();

				myworld.GameLogic.ApplyDebugOverrides( mymod );

				this.PostEnterWorld();
			}
		}
		private void OnClientConnect() {
			if( player.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();

				myworld.GameLogic.ApplyDebugOverrides( mymod );

				ClientPacketHandlers.SendRequestModSettingsFromClient( mymod );
				ClientPacketHandlers.SendRequestModDataFromClient( mymod );
			}
		}
		private void OnServerConnect() {
			if( player.whoAmI == this.player.whoAmI ) {    // Current player
				var mymod = (TheLunaticMod)this.mod;
				var myworld = mymod.GetModWorld<TheLunaticWorld>();

				myworld.GameLogic.ApplyDebugOverrides( mymod );
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
	}
}
