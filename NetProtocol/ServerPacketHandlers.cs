using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic.NetProtocol {
	static class ServerPacketHandlers {
		public static void HandlePacket( BinaryReader reader, int playerWho ) {
			var mymod = TheLunaticMod.Instance;
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestModData:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Server RequestModData" ); }
				ServerPacketHandlers.ReceiveRequestModDataOnServer( reader, playerWho );
				break;
			case NetProtocolTypes.GiveMaskToServer:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Server GiveMaskToServer" ); }
				ServerPacketHandlers.ReceiveGivenMaskOnServer( reader, playerWho );
				break;
			default:
				LogHelpers.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModDataFromServer( Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModData );
			packet.Write( (bool)myworld.GameLogic.HasLoonyArrived );
			packet.Write( (bool)myworld.GameLogic.HasLoonyQuit );
			packet.Write( (bool)myworld.GameLogic.HasGameEnded );
			packet.Write( (bool)myworld.GameLogic.HasWon );
			packet.Write( (bool)myworld.GameLogic.IsSafe );
			packet.Write( (int)myworld.GameLogic.HalfDaysElapsed );

			packet.Write( (int)myworld.MaskLogic.GivenVanillaMasksByType.Count );
			foreach( int mask in myworld.MaskLogic.GivenVanillaMasksByType ) {
				packet.Write( (int)mask );
			}
			packet.Write( (int)myworld.MaskLogic.GivenCustomMasksByBossUid.Count );
			foreach( string maskId in myworld.MaskLogic.GivenCustomMasksByBossUid ) {
				packet.Write( (string)maskId );
			}

			if( mymod.Config.DebugModeNetInfo ) {
				LogHelpers.Log( "DEBUG Sending mod data from server. " + myworld.ID + ", " +
					myworld.GameLogic.HasLoonyArrived + ", " +
					myworld.GameLogic.HasLoonyQuit + ", " +
					myworld.GameLogic.HasGameEnded + ", " +
					myworld.GameLogic.HasWon + ", " +
					myworld.GameLogic.IsSafe + ", " +
					myworld.GameLogic.HalfDaysElapsed + ", " +
					myworld.MaskLogic.GivenVanillaMasksByType.Count + ", [" +
					String.Join( ",", myworld.MaskLogic.GivenVanillaMasksByType.ToArray() ) + "], " +
					myworld.MaskLogic.GivenCustomMasksByBossUid.Count + ", [" +
					String.Join( ",", myworld.MaskLogic.GivenCustomMasksByBossUid.ToArray() ) + "]" );
			}

			packet.Send( (int)player.whoAmI );
		}

		public static void BroadcastEndSignFromServer( int duration ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = TheLunaticMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.EndSign );
			packet.Write( duration );

			for( int i = 0; i < Main.player.Length; i++ ) {
				if( Main.player[i] != null && Main.player[i].active ) {
					packet.Send( i );
				}
			}
		}

		public static void BroadcastGivenMaskFromServer( int fromWho, int maskType, int bossMask ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = TheLunaticMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.GiveMaskToClient );
			packet.Write( fromWho );
			packet.Write( maskType );
			packet.Write( bossMask );

			for( int i = 0; i < Main.player.Length; i++ ) {
				if( i != fromWho && Main.player[i] != null && Main.player[i].active ) {
					packet.Send( i );
				}
			}
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveRequestModDataOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }
			
			if( playerWho < 0 || playerWho >= Main.player.Length || Main.player[playerWho] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Invalid player id " + playerWho );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	Debug.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Inactive player " + Main.player[playerWho].name );
			//	return;
			//}

			ServerPacketHandlers.SendModDataFromServer( Main.player[playerWho] );
		}


		private static void ReceiveGivenMaskOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.MaskLogic == null ) { throw new ModHelpersException( "Mask logic not initialized." ); }
			
			int maskType = reader.ReadInt32();
			int bossType = reader.ReadInt32();
			Item fakeMask = new Item();

			fakeMask.SetDefaults( maskType );
			if( maskType == mymod.ItemType<CustomBossMaskItem>() ) {
				var moditem = (CustomBossMaskItem)fakeMask.modItem;
				moditem.SetBoss( bossType );
			}
			
			if( playerWho < 0 || playerWho >= Main.player.Length || Main.player[playerWho] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid player id " + playerWho );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}
			if( myworld.MaskLogic.DoesLoonyHaveThisMask( fakeMask ) ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid mask from player " + Main.player[playerWho].name + " of type " + maskType );
				return;
			}

			myworld.MaskLogic.RegisterReceiptOfMask( Main.player[playerWho], maskType, bossType );
			
			ServerPacketHandlers.BroadcastGivenMaskFromServer( playerWho, maskType, bossType );
		}
	}
}
