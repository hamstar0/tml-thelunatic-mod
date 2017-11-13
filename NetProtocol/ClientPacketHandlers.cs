using HamstarHelpers.DebugHelpers;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic.NetProtocol {
	static class ClientPacketHandlers {
		public static void HandlePacket( TheLunaticMod mymod, BinaryReader reader ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.ModSettings:
				if( mymod.IsDisplayNetInfoDebugMode() ) { DebugHelpers.Log( "Client ModSettings" ); }
				ClientPacketHandlers.ReceiveModSettingsOnClient( mymod, reader );
				break;
			case NetProtocolTypes.ModData:
				if( mymod.IsDisplayNetInfoDebugMode() ) { DebugHelpers.Log( "Client ModData" ); }
				ClientPacketHandlers.ReceiveModDataOnClient( mymod, reader );
				break;
			case NetProtocolTypes.EndSign:
				if( mymod.IsDisplayNetInfoDebugMode() ) { DebugHelpers.Log( "Client EndSign" ); }
				ClientPacketHandlers.ReceiveEndSignOnClient( mymod, reader );
				break;
			case NetProtocolTypes.GiveMaskToClient:
				if( mymod.IsDisplayNetInfoDebugMode() ) { DebugHelpers.Log( "Client GiveMaskToClient" ); }
				ClientPacketHandlers.ReceiveGivenMaskOnClient( mymod, reader );
				break;
			default:
				DebugHelpers.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////
		
		public static void SendRequestModSettingsFromClient( TheLunaticMod mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModSettings );

			packet.Send();
		}

		public static void SendRequestModDataFromClient( TheLunaticMod mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModData );

			packet.Send();
		}

		public static void SendGivenMaskFromClient( TheLunaticMod mymod, Item mask ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();
			int boss_type = -1;
			if( mask.type == mymod.ItemType<CustomBossMaskItem>() ) {
				boss_type = mask.GetGlobalItem<CustomBossMaskItemInfo>(mymod).BossNpcType;
			}

			packet.Write( (byte)NetProtocolTypes.GiveMaskToServer );
			packet.Write( (int)mask.type );
			packet.Write( boss_type );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			mymod.Config.DeserializeMe( reader.ReadString() );

			//if( mymod.Config.Data.DaysUntil < 0 ) {
			//	DebugHelper.Log( "TheLunaticNetProtocol.ReceiveModSettingsOnClient - Invalid 'DaysUntil' quantity." );
			//	return;
			//}
		}


		private static void ReceiveModDataOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			bool has_loony_arrived = reader.ReadBoolean();
			bool has_loony_quit = reader.ReadBoolean();
			bool has_game_end = reader.ReadBoolean();
			bool has_won = reader.ReadBoolean();
			bool is_safe = reader.ReadBoolean();
			int time = reader.ReadInt32();

			int mask_count = reader.ReadInt32();
			var masks = new int[mask_count];
			for( int i = 0; i < mask_count; i++ ) {
				masks[i] = reader.ReadInt32();
			}

			int custom_mask_count = reader.ReadInt32();
			var custom_masks = new string[custom_mask_count];
			for( int i = 0; i < custom_mask_count; i++ ) {
				custom_masks[i] = reader.ReadString();
			}

			if( mymod.IsDisplayNetInfoDebugMode() ) {
				DebugHelpers.Log( "DEBUG Receiving mod data on client. " +
					time + ", " + has_loony_arrived + ", " + has_loony_quit + ", " + has_game_end + ", " + has_won + ", " +
					is_safe + ", " + mask_count + ", [" + String.Join( ",", masks ) + "]" );
			}

			modworld.GameLogic.LoadOnce( mymod, has_loony_arrived, has_loony_quit, has_game_end, has_won, is_safe, time );
			modworld.MaskLogic.LoadOnce( mymod, masks, custom_masks );

			var modplayer = Main.player[Main.myPlayer].GetModPlayer<MyPlayer>( mymod );
			modplayer.PostEnterWorld();
		}


		private static void ReceiveEndSignOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			int duration = reader.ReadInt32();

			if( duration <= 0 || duration > 7200 ) {    // 2 minutes is a bit long maybe
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveEndSignOnClient - Invalid sign duration " + duration );
				return;
			}

			modworld.GameLogic.ApplyEndSignForMe( mymod, duration );
		}


		private static void ReceiveGivenMaskOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = mymod.GetModWorld<MyWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			// Mask is given discreetly
			int from_who = reader.ReadInt32();
			int mask_type = reader.ReadInt32();
			int boss_type = reader.ReadInt32();

			if( from_who < 0 || from_who >= Main.player.Length || Main.player[from_who] == null ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnClient - Invalid player id " + from_who );
				return;
			}
			if( !modworld.MaskLogic.GetRemainingVanillaMasks().Contains( mask_type ) ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnClient - Invalid mask from player " + Main.player[from_who].name + " of type " + mask_type );
				return;
			}

			Player player = null;
			if( from_who == Main.myPlayer ) {
				player = Main.player[from_who];
			}

			modworld.MaskLogic.RegisterReceiptOfMask( mymod, player, mask_type, boss_type );
		}
	}
}
