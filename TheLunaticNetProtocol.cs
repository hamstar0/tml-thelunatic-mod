using HamstarHelpers.MiscHelpers;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;


namespace TheLunatic {
	public enum TheLunaticNetProtocolTypes : byte {
		RequestModSettings,
		RequestModData,
		ModSettings,
		ModData,
		EndSign,
		GiveMaskToServer,
		GiveMaskToClient
	}


	public static class TheLunaticNetProtocol {
		public static void RouteReceivedPackets( TheLunaticMod mymod, BinaryReader reader ) {
			TheLunaticNetProtocolTypes protocol = (TheLunaticNetProtocolTypes)reader.ReadByte();
			bool is_debug = (TheLunaticMod.DEBUGMODE & 1) > 0;

			switch( protocol ) {
			case TheLunaticNetProtocolTypes.RequestModSettings:
				if( is_debug ) { DebugHelpers.Log( "Packet RequestModSettings" ); }
				TheLunaticNetProtocol.ReceiveRequestModSettingsOnServer( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.RequestModData:
				//if( is_debug ) { DebugHelpers.Log( "Packet RequestModData" ); }
				TheLunaticNetProtocol.ReceiveRequestModDataOnServer( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.ModSettings:
				//if( is_debug ) { DebugHelpers.Log( "Packet ModSettings" ); }
				TheLunaticNetProtocol.ReceiveModSettingsOnClient( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.ModData:
				//if( is_debug ) { DebugHelpers.Log( "Packet ModData" ); }
				TheLunaticNetProtocol.ReceiveModDataOnClient( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.EndSign:
				//if( is_debug ) { DebugHelpers.Log( "Packet EndSign" ); }
				TheLunaticNetProtocol.ReceiveEndSignOnClient( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.GiveMaskToServer:
				//if( is_debug ) { DebugHelpers.Log( "Packet GiveMaskToServer" ); }
				TheLunaticNetProtocol.ReceiveGivenMaskOnServer( mymod, reader );
				break;
			case TheLunaticNetProtocolTypes.GiveMaskToClient:
				//if( is_debug ) { DebugHelpers.Log( "Packet GiveMaskToClient" ); }
				TheLunaticNetProtocol.ReceiveGivenMaskOnClient( mymod, reader );
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

			packet.Write( (byte)TheLunaticNetProtocolTypes.RequestModSettings );
			packet.Write( (int)Main.myPlayer );

			packet.Send();
		}

		public static void SendRequestModDataFromClient( TheLunaticMod mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)TheLunaticNetProtocolTypes.RequestModData );
			packet.Write( (int)Main.myPlayer );

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

			packet.Write( (byte)TheLunaticNetProtocolTypes.GiveMaskToServer );
			packet.Write( (int)Main.myPlayer );
			packet.Write( (int)mask.type );
			packet.Write( boss_type );

			packet.Send();
		}


		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( TheLunaticMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }
			
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)TheLunaticNetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );
			
			packet.Send( (int)player.whoAmI );
		}

		public static void SendModDataFromServer( TheLunaticMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)TheLunaticNetProtocolTypes.ModData );
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

			if( (TheLunaticMod.DEBUGMODE & 1) > 0 ) {
				DebugHelpers.Log( "DEBUG Sending mod data from server. " + modworld.ID + ", " +
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

			packet.Write( (byte)TheLunaticNetProtocolTypes.EndSign );
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

			packet.Write( (byte)TheLunaticNetProtocolTypes.GiveMaskToClient );
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

			TheLunaticMod.DEBUGMODE = mymod.Config.Data.DEBUGFLAGS;
		}


		private static void ReceiveModDataOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var modworld = mymod.GetModWorld<TheLunaticWorld>();
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

			if( (TheLunaticMod.DEBUGMODE & 1) > 0 ) {
				DebugHelpers.Log( "DEBUG Receiving mod data on client. " +
					time + ", " + has_loony_arrived + ", " + has_loony_quit + ", " + has_game_end + ", " + has_won + ", " +
					is_safe + ", " + mask_count + ", [" + String.Join( ",", masks ) + "]" );
			}

			modworld.GameLogic.LoadOnce( has_loony_arrived, has_loony_quit, has_game_end, has_won, is_safe, time );
			modworld.MaskLogic.LoadOnce( masks, custom_masks );

			var modplayer = Main.player[Main.myPlayer].GetModPlayer<TheLunaticPlayer>( mymod );
			modplayer.PostEnterWorld();
		}


		private static void ReceiveEndSignOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }

			int duration = reader.ReadInt32();

			if( duration <= 0 || duration > 7200 ) {    // 2 minutes is a bit long maybe
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveEndSignOnClient - Invalid sign duration " + duration );
				return;
			}

			modworld.GameLogic.ApplyEndSignForMe( duration );
		}


		private static void ReceiveGivenMaskOnClient( TheLunaticMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
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

			modworld.MaskLogic.RegisterReceiptOfMask( player, mask_type, boss_type );
		}


		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveRequestModSettingsOnServer( TheLunaticMod mymod, BinaryReader reader ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			int player_who = reader.ReadInt32();

			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModSettingsOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModSettingsOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}

			TheLunaticNetProtocol.SendModSettingsFromServer( mymod, Main.player[player_who] );
		}


		private static void ReceiveRequestModDataOnServer( TheLunaticMod mymod, BinaryReader reader ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			int player_who = reader.ReadInt32();

			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	Debug.Log( "TheLunaticNetProtocol.ReceiveRequestModDataOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}

			TheLunaticNetProtocol.SendModDataFromServer( mymod, Main.player[player_who] );
		}


		private static void ReceiveGivenMaskOnServer( TheLunaticMod mymod, BinaryReader reader ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var modworld = mymod.GetModWorld<TheLunaticWorld>();
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }

			int player_who = reader.ReadInt32();
			int mask_type = reader.ReadInt32();
			int boss_type = reader.ReadInt32();
			Item fake_mask = new Item();

			fake_mask.SetDefaults( mask_type );
			if( mask_type == mymod.ItemType<CustomBossMaskItem>() ) {
				var moditem = (CustomBossMaskItem)fake_mask.modItem;
				moditem.SetBoss( boss_type );
			}
			
			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid player id " + player_who );
				return;
			}
			//if( !Main.player[player_who].active ) {
			//	DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Inactive player " + Main.player[player_who].name );
			//	return;
			//}
			if( modworld.MaskLogic.DoesLoonyHaveThisMask(fake_mask) ) {
				DebugHelpers.Log( "TheLunaticNetProtocol.ReceiveGivenMaskOnServer - Invalid mask from player " + Main.player[player_who].name + " of type " + mask_type );
				return;
			}

			modworld.MaskLogic.RegisterReceiptOfMask( Main.player[player_who], mask_type, boss_type );
			
			TheLunaticNetProtocol.BroadcastGivenMaskFromServer( mymod, player_who, mask_type, boss_type );
		}
	}
}
