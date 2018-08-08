﻿using HamstarHelpers.Helpers.DebugHelpers;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic.NetProtocol {
	static class ServerPacketHandlers {
		public static void HandlePacket( TheLunaticMod mymod, BinaryReader reader, int player_who ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestModSettings:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Server RequestModSettings" ); }
				ServerPacketHandlers.ReceiveRequestModSettingsOnServer( mymod, reader, player_who );
				break;
			case NetProtocolTypes.RequestModData:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Server RequestModData" ); }
				ServerPacketHandlers.ReceiveRequestModDataOnServer( mymod, reader, player_who );
				break;
			case NetProtocolTypes.GiveMaskToServer:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Server GiveMaskToServer" ); }
				ServerPacketHandlers.ReceiveGivenMaskOnServer( mymod, reader, player_who );
				break;
			default:
				LogHelpers.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( TheLunaticMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }
			
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.ConfigJson.SerializeMe() );
			
			packet.Send( (int)player.whoAmI );
		}

		public static void SendModDataFromServer( TheLunaticMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModData );
			packet.Write( (bool)modworld.GameLogic.HasLoonyArrived );
			packet.Write( (bool)modworld.GameLogic.HasLoonyQuit );
			packet.Write( (bool)modworld.GameLogic.HasGameEnded );
			packet.Write( (bool)modworld.GameLogic.HasWon );
			packet.Write( (bool)modworld.GameLogic.IsSafe );
			packet.Write( (int)modworld.GameLogic.HalfDaysElapsed );

			packet.Write( (int)modworld.MaskLogic.GivenVanillaMasksByType.Count );
			foreach( int mask in modworld.MaskLogic.GivenVanillaMasksByType ) {
				packet.Write( (int)mask );
			}
			packet.Write( (int)modworld.MaskLogic.GivenCustomMasksByBossUid.Count );
			foreach( string mask_id in modworld.MaskLogic.GivenCustomMasksByBossUid ) {
				packet.Write( (string)mask_id );
			}

			if( mymod.Config.DebugModeNetInfo ) {
				LogHelpers.Log( "DEBUG Sending mod data from server. " + modworld.ID + ", " +
					modworld.GameLogic.HasLoonyArrived + ", " +
					modworld.GameLogic.HasLoonyQuit + ", " +
					modworld.GameLogic.HasGameEnded + ", " +
					modworld.GameLogic.HasWon + ", " +
					modworld.GameLogic.IsSafe + ", " +
					modworld.GameLogic.HalfDaysElapsed + ", " +
					modworld.MaskLogic.GivenVanillaMasksByType.Count + ", [" +
					String.Join( ",", modworld.MaskLogic.GivenVanillaMasksByType.ToArray() ) + "], " +
					modworld.MaskLogic.GivenCustomMasksByBossUid.Count + ", [" +
					String.Join( ",", modworld.MaskLogic.GivenCustomMasksByBossUid.ToArray() ) + "]" );
			}

			packet.Send( (int)player.whoAmI );
		}

		public static void BroadcastEndSignFromServer( TheLunaticMod mymod, int duration ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.EndSign );
			packet.Write( duration );

			for( int i = 0; i < Main.player.Length; i++ ) {
				if( Main.player[i] != null && Main.player[i].active ) {
					packet.Send( i );
				}
			}
		}

		public static void BroadcastGivenMaskFromServer( TheLunaticMod mymod, int from_who, int mask_type, int boss_mask ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.GiveMaskToClient );
			packet.Write( from_who );
			packet.Write( mask_type );
			packet.Write( boss_mask );

			for( int i = 0; i < Main.player.Length; i++ ) {
				if( i != from_who && Main.player[i] != null && Main.player[i].active ) {
					packet.Send( i );
				}
			}
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveRequestModSettingsOnServer( TheLunaticMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }
			
			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModSettingsOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModSettingsOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}

			ServerPacketHandlers.SendModSettingsFromServer( mymod, Main.player[player_who] );
		}


		private static void ReceiveRequestModDataOnServer( TheLunaticMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }
			
			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	Debug.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}

			ServerPacketHandlers.SendModDataFromServer( mymod, Main.player[player_who] );
		}


		private static void ReceiveGivenMaskOnServer( TheLunaticMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }
			
			int mask_type = reader.ReadInt32();
			int boss_type = reader.ReadInt32();
			Item fake_mask = new Item();

			fake_mask.SetDefaults( mask_type );
			if( mask_type == mymod.ItemType<CustomBossMaskItem>() ) {
				var moditem = (CustomBossMaskItem)fake_mask.modItem;
				moditem.SetBoss( boss_type );
			}
			
			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}
			if( modworld.MaskLogic.DoesLoonyHaveThisMask( mymod, fake_mask ) ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid mask from player " + Main.player[player_who].name + " of type " + mask_type );
				return;
			}

			modworld.MaskLogic.RegisterReceiptOfMask( mymod, Main.player[player_who], mask_type, boss_type );
			
			ServerPacketHandlers.BroadcastGivenMaskFromServer( mymod, player_who, mask_type, boss_type );
		}
	}
}
