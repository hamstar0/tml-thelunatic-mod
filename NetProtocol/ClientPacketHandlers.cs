using HamstarHelpers.Helpers.DebugHelpers;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic.NetProtocol {
	static class ClientPacketHandlers {
		public static void HandlePacket( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.ModSettings:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Client ModSettings" ); }
				ClientPacketHandlers.ReceiveModSettingsOnClient( reader );
				break;
			case NetProtocolTypes.ModData:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Client ModData" ); }
				ClientPacketHandlers.ReceiveModDataOnClient( reader );
				break;
			case NetProtocolTypes.EndSign:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Client EndSign" ); }
				ClientPacketHandlers.ReceiveEndSignOnClient( reader );
				break;
			case NetProtocolTypes.GiveMaskToClient:
				if( mymod.Config.DebugModeNetInfo ) { LogHelpers.Log( "Client GiveMaskToClient" ); }
				ClientPacketHandlers.ReceiveGivenMaskOnClient( reader );
				break;
			default:
				LogHelpers.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////
		
		public static void SendRequestModSettingsFromClient() {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModSettings );

			packet.Send();
		}

		public static void SendRequestModDataFromClient() {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModData );

			packet.Send();
		}

		public static void SendGivenMaskFromClient( Item mask ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();
			int bossType = -1;
			if( mask.type == mymod.ItemType<CustomBossMaskItem>() ) {
				bossType = mask.GetGlobalItem<CustomBossMaskItemInfo>().BossNpcType;
			}

			packet.Write( (byte)NetProtocolTypes.GiveMaskToServer );
			packet.Write( (int)mask.type );
			packet.Write( bossType );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			bool success;

			mymod.ConfigJson.DeserializeMe( reader.ReadString(), out success );

			//if( mymod.Config.Data.DaysUntil < 0 ) {
			//	DebugHelper.Log( "TheLunaticNetProtocol.ReceiveModSettingsOnClient - Invalid 'DaysUntil' quantity." );
			//	return;
			//}
		}


		private static void ReceiveModDataOnClient( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }
			if( myworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			bool hasLoonyArrived = reader.ReadBoolean();
			bool hasLoonyQuit = reader.ReadBoolean();
			bool hasGameEnd = reader.ReadBoolean();
			bool hasWon = reader.ReadBoolean();
			bool isSafe = reader.ReadBoolean();
			int time = reader.ReadInt32();

			int maskCount = reader.ReadInt32();
			var masks = new int[maskCount];
			for( int i = 0; i < maskCount; i++ ) {
				masks[i] = reader.ReadInt32();
			}

			int customMaskCount = reader.ReadInt32();
			var customMasks = new string[customMaskCount];
			for( int i = 0; i < customMaskCount; i++ ) {
				customMasks[i] = reader.ReadString();
			}

			if( mymod.Config.DebugModeNetInfo ) {
				LogHelpers.Log( "DEBUG Receiving mod data on client. " +
					time + ", " + hasLoonyArrived + ", " + hasLoonyQuit + ", " + hasGameEnd + ", " + hasWon + ", " +
					isSafe + ", " + maskCount + ", [" + String.Join( ",", masks ) + "]" );
			}

			myworld.GameLogic.LoadOnce( hasLoonyArrived, hasLoonyQuit, hasGameEnd, hasWon, isSafe, time );
			myworld.MaskLogic.LoadOnce( masks, customMasks );

			var modplayer = Main.player[Main.myPlayer].GetModPlayer<TheLunaticPlayer>();
			modplayer.PostEnterWorld();
		}


		private static void ReceiveEndSignOnClient( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			int duration = reader.ReadInt32();

			if( duration <= 0 || duration > 7200 ) {    // 2 minutes is a bit long maybe
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveEndSignOnClient - Invalid sign duration " + duration );
				return;
			}

			myworld.GameLogic.ApplyEndSignForMe( duration );
		}


		private static void ReceiveGivenMaskOnClient( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			// Mask is given discreetly
			int fromWho = reader.ReadInt32();
			int maskType = reader.ReadInt32();
			int bossType = reader.ReadInt32();

			if( fromWho < 0 || fromWho >= Main.player.Length || Main.player[fromWho] == null ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnClient - Invalid player id " + fromWho );
				return;
			}
			if( !myworld.MaskLogic.GetRemainingVanillaMasks().Contains( maskType ) ) {
				LogHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnClient - Invalid mask from player " + Main.player[fromWho].name + " of type " + maskType );
				return;
			}

			Player player = null;
			if( fromWho == Main.myPlayer ) {
				player = Main.player[fromWho];
			}

			myworld.MaskLogic.RegisterReceiptOfMask( player, maskType, bossType );
		}
	}
}
