using HamstarHelpers.Classes.Errors;
using HamstarHelpers.Helpers.Debug;
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
		
		public static void SendRequestModDataFromClient() {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModData );

			packet.Send();
		}

		public static void SendGivenMaskFromClient( Item maskItem ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();
			int bossNpcType = 0;
			if( maskItem.type == mymod.ItemType<CustomBossMaskItem>() && maskItem.modItem != null ) {
				bossNpcType = ((CustomBossMaskItem)maskItem.modItem).BossNpcType;
			}

			packet.Write( (byte)NetProtocolTypes.GiveMaskToServer );
			packet.Write( (int)maskItem.type );
			packet.Write( bossNpcType );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModDataOnClient( BinaryReader reader ) {
			var mymod = TheLunaticMod.Instance;
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }
			if( myworld.MaskLogic == null ) { throw new ModHelpersException( "Mask logic not initialized." ); }

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
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }

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
			if( myworld.MaskLogic == null ) { throw new ModHelpersException( "Mask logic not initialized." ); }

			// Mask is given discreetly
			int fromWho = reader.ReadInt32();
			int maskType = reader.ReadInt32();
			int bossNpcType = reader.ReadInt32();

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

			myworld.MaskLogic.RegisterReceiptOfMask( player, maskType, bossNpcType );
		}
	}
}
