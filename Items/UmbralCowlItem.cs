using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Buffs;
using Utils;


namespace TheLunatic.Items {
	public class UmbralCowlItem : ModItem {
		public static int Width = 26;
		public static int Height = 30;


		////////////////

		public static void Give( Mod mod, Player player ) {
			int who = ItemHelper.CreateItem( player.Center, mod.ItemType<UmbralCowlItem>(), 1, UmbralCowlItem.Width, UmbralCowlItem.Height );
			Item item = Main.item[who];
			item.noGrabDelay = 15;

			var info = item.GetModInfo<UmbralCowlItemInfo>( mod );
			info.IsAllowed = true;
		}

		////////////////

		public static void CheckEquipState( Mod mod, Player player ) {
			int cowl_type = mod.ItemType<UmbralCowlItem>();
			bool found = false;

			for( int i=0; i<player.armor.Length; i++ ) {
				if( player.armor[i].type == cowl_type ) {
					found = true;
					break;
				}
			}

			if( !found ) {
				var buff = (ShadowWalkerBuff)mod.GetBuff( "ShadowWalkerBuff" );
				buff.End( player, false );
			}
		}



		////////////////

		public override bool Autoload( ref string name, ref string texture, IList<EquipType> equips ) {
			equips.Add( EquipType.Back );
			equips.Add( EquipType.Front );
			return true;
		}

		public override void SetDefaults() {
			this.item.name = "Umbral Cowl";
			this.item.toolTip = "Be one with the darkness.";
			this.item.width = UmbralCowlItem.Width;
			this.item.height = UmbralCowlItem.Height;
			this.item.maxStack = 1;
			this.item.value = Item.buyPrice( 1, 0, 0, 0 );
			this.item.rare = 11;
			this.item.accessory = true;
		}

		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var info = item.GetModInfo<UmbralCowlItemInfo>( mod );
			if( info.IsAllowed ) {
				TooltipLine tip = new TooltipLine( this.mod, "how_to", "Enter complete darkness to use" );
				TooltipLine tip2 = new TooltipLine( this.mod, "how_to2", "Press Shift to reenter light" );
				TooltipLine tip3 = new TooltipLine( this.mod, "warn", "Beware the grue!" );
				tip3.overrideColor = new Color(32, 32, 32);
				tooltips.Add( tip );
				tooltips.Add( tip2 );
				tooltips.Add( tip3 );
			} else {
				TooltipLine tip = new TooltipLine( this.mod, "how_to", "Cannot use with cheats!" );
				tip.overrideColor = new Color( Main.DiscoR, Main.DiscoG, Main.DiscoB );
				tooltips.Add( tip );
			}
		}


		////////////////

		public override void Load( TagCompound tag ) {
			var info = item.GetModInfo<UmbralCowlItemInfo>( mod );
			info.IsAllowed = tag.GetBool( "is_allowed_use" );
		}

		public override TagCompound Save() {
			var info = item.GetModInfo<UmbralCowlItemInfo>( mod );
			return new TagCompound { { "is_allowed_use", (bool)info.IsAllowed } };
		}


		////////////////
		
		public override void UpdateAccessory( Player player, bool hideVisual ) {
			if( player.whoAmI != Main.myPlayer ) { return; }    // Current player only

			var info = item.GetModInfo<UmbralCowlItemInfo>( this.mod );
			if( !info.IsAllowed ) { return; }	// Allowed to use

			if( ShadowWalkerBuff.CanShadowWalk( player ) ) {
				if( ShadowWalkerBuff.FindBuffIndex( this.mod, player ) == -1 ) {
					ShadowWalkerBuff.AddBuffFor( this.mod, player );
				}

				var modplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
				if( modplayer.Noclip != null ) {
					modplayer.Noclip.UpdateMode( player );   // Redundant?
				}
			}
		}
	}



	class UmbralCowlItemInfo : ItemInfo {
		public bool IsAllowed = false;

		public override ItemInfo Clone() {
			var clone = (UmbralCowlItemInfo)base.Clone();
			clone.IsAllowed = this.IsAllowed;

			return clone;
		}
	}
}
