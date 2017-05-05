using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace Utils {
	public static class ItemHelper {
		public static string GetUniqueId( Item item ) {
			if( item.modItem != null ) {
				return item.modItem.mod.Name + " " + Main.itemName[item.type];
			}
			return ""+ item.netID;
		}


		public static int CreateItem( Vector2 pos, int type, int stack, int width, int height, int prefix=0 ) {
			int number = Item.NewItem( (int)pos.X, (int)pos.Y, width, height, type, stack, false, prefix, true, false );
			if( Main.netMode == 1 ) {
				NetMessage.SendData( 21, -1, -1, "", number, 1f, 0f, 0f, 0, 0, 0 );
			}
			return number;
		}

		public static void DestroyWorldItem( int i ) {
			Item item = Main.item[i];
			item.active = false;
			item.type = 0;
			item.name = "";
			item.stack = 0;

			if( Main.netMode == 2 ) {
				NetMessage.SendData( 21, -1, -1, "", i );
			}
		}

		public static int FindFirstOfItemInCollection( Item[] collection, ISet<int> item_types ) {
			for( int i = 0; i < collection.Length; i++ ) {
				Item item = collection[i];
				if( item.stack == 0 ) { continue; }
				if( item_types.Contains( item.type ) ) { return i; }
			}

			return -1;
		}
	}
}
