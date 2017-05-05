using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace Utils {
	public static class PlayerHelper {
		public static void Teleport( Player player, Vector2 pos, int style = -1 ) {
			player.grappling[0] = -1;
			player.grapCount = 0;

			bool is_immune = player.immune;
			int immune_time = player.immuneTime;
			player.Spawn();
			player.immune = is_immune;
			player.immuneTime = immune_time;

			if( Main.netMode <= 1 ) {
				player.Teleport( pos, style );
			} else {
				NetMessage.SendData( 65, -1, -1, "", 0, (float)player.whoAmI, pos.X, pos.Y, style, 0, 0 );
			}
		}


		public static Item FindFirstOfItemFor( Player player, ISet<int> item_types ) {
			int found = ItemHelper.FindFirstOfItemInCollection( player.inventory, item_types );
			if( found != -1 ) {
				return player.inventory[found];
			} else {
				found = ItemHelper.FindFirstOfItemInCollection( player.armor, item_types );
				if( found != -1 ) {
					return player.armor[found];
				}
			}

			return null;
		}
		

		public static bool UnhandItem( Player player ) {
			// Preferably select a blank slot
			for( int i = 0; i < player.inventory.Length; i++ ) {
				if( player.inventory[i] == null || player.inventory[i].IsAir ) {
					player.selectedItem = i;
					return true;
				}
			}
			// Otherwise select a non-usable item
			for( int i = 0; i < player.inventory.Length; i++ ) {
				Item item = player.inventory[i];
				if( item != null && item.holdStyle == 0 && item.createTile == -1 && !item.potion && item.useStyle == 0 ) {
					player.selectedItem = i;
					return true;
				}
			}
			// Otherwise select a non-held item
			for( int i = 0; i < player.inventory.Length; i++ ) {
				Item item = player.inventory[i];
				if( item != null && item.holdStyle == 0 ) {
					player.selectedItem = i;
					return true;
				}
			}
			// Give up?
			return false;
		}


		public static bool IsPlayerNearBoss( Player player ) {
			int x = ((int)player.Center.X - (Main.maxScreenW / 2)) / 16;
			int y = ((int)player.Center.Y - (Main.maxScreenH / 2)) / 16;

			Rectangle player_zone = new Rectangle( x, y, (Main.maxScreenH / 16), (Main.maxScreenH / 16) );
			int boss_radius = 5000;

			for( int i = 0; i < Main.npc.Length; i++ ) {
				NPC check_npc = Main.npc[i];
				if( !check_npc.active || !check_npc.boss ) { continue; }

				int npc_left = (int)(check_npc.position.X + (float)check_npc.width / 2f) - boss_radius;
				int npc_top = (int)(check_npc.position.Y + (float)check_npc.height / 2f) - boss_radius;
				Rectangle npc_zone = new Rectangle( npc_left, npc_top, boss_radius * 2, boss_radius * 2 );

				if( player_zone.Intersects( npc_zone ) ) { return true; }
			}

			return false;
		}


		public static bool IsPlayerNaked( Player player, bool not_vanity = false ) {
			// Armor
			if( !player.armor[0].IsAir ) { return false; }
			if( !player.armor[1].IsAir ) { return false; }
			if( !player.armor[2].IsAir ) { return false; }
			// Accessory
			if( !player.armor[3].IsAir && !player.hideVisual[3] ) { return false; }
			if( !player.armor[4].IsAir && !player.hideVisual[4] ) { return false; }
			if( !player.armor[5].IsAir && !player.hideVisual[5] ) { return false; }
			if( !player.armor[6].IsAir && !player.hideVisual[6] ) { return false; }
			if( !player.armor[7].IsAir && !player.hideVisual[7] ) { return false; }
			if( not_vanity ) {
				// Vanity
				if( !player.armor[8].IsAir ) { return false; }
				if( !player.armor[9].IsAir ) { return false; }
				if( !player.armor[10].IsAir ) { return false; }
				// Vanity Accessory
				if( !player.armor[11].IsAir /*&& !player.hideVisual[3]*/ ) { return false; }
				if( !player.armor[12].IsAir /*&& !player.hideVisual[4]*/ ) { return false; }
				if( !player.armor[13].IsAir /*&& !player.hideVisual[5]*/ ) { return false; }
				if( !player.armor[14].IsAir /*&& !player.hideVisual[6]*/ ) { return false; }
				if( !player.armor[15].IsAir /*&& !player.hideVisual[7]*/ ) { return false; }
			}

			return true;
		}


		public static bool IsRelaxed( Player player, bool not_mounted = true, bool not_grappled = true,
				bool not_pulleyed = true, bool not_frozen = true, bool not_inverted = true ) {
			// Unmoved/moving
			if( player.velocity.X != 0 || player.velocity.Y != 0 ) { return false; }

			// No mounts (includes minecart)
			if( not_mounted && player.mount.Active ) { return false; }

			// Not grappled
			if( not_grappled && player.grappling[0] >= 0 ) { return false; }

			// Not on a pulley
			if( not_pulleyed && player.pulley ) { return false; }

			// Not frozen
			if( not_frozen && player.frozen ) { return false; }

			// Not inverted (gravity)
			if( not_inverted && player.gravDir < 0f ) { return false; }

			return true;
		}
	}
}
