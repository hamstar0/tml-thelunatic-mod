using HamstarHelpers.Helpers.ItemHelpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Buffs;


namespace TheLunatic.Items {
	[AutoloadEquip( EquipType.Back, EquipType.Front )]
	class UmbralCowlItem : ModItem {
		public static int Width = 26;
		public static int Height = 30;


		////////////////

		public static void Give( Player player ) {
			var mymod = TheLunaticMod.Instance;
			int who = ItemHelpers.CreateItem( player.Center, mymod.ItemType<UmbralCowlItem>(), 1, UmbralCowlItem.Width, UmbralCowlItem.Height );
			Item item = Main.item[who];
			item.noGrabDelay = 15;

			var itemInfo = item.GetGlobalItem<UmbralCowlItemInfo>( mymod );
			itemInfo.IsAllowed = true;
		}

		////////////////

		public static void CheckEquipState( Player player ) {
			var mymod = TheLunaticMod.Instance;
			int cowlType = mymod.ItemType<UmbralCowlItem>();
			bool found = false;

			for( int i=0; i<player.armor.Length; i++ ) {
				if( player.armor[i] == null || !player.armor[i].active ) { continue; }
				if( player.armor[i].type == cowlType ) {
					found = true;
					break;
				}
			}

			if( !found ) {
				var buff = (ShadowWalkerBuff)mymod.GetBuff( "ShadowWalkerBuff" );
				buff.End( player, false );
			}
		}



		////////////////

		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Umbral Cowl" );
			this.Tooltip.SetDefault( "Be one with the darkness" );
		}

		public override void SetDefaults() {
			this.item.width = UmbralCowlItem.Width;
			this.item.height = UmbralCowlItem.Height;
			this.item.maxStack = 1;
			this.item.value = Item.buyPrice( 1, 0, 0, 0 );
			this.item.rare = 11;
			this.item.accessory = true;
		}

		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var itemInfo = item.GetGlobalItem<UmbralCowlItemInfo>();
			if( itemInfo.IsAllowed ) {
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
			var itemInfo = item.GetGlobalItem<UmbralCowlItemInfo>();
			itemInfo.IsAllowed = tag.GetBool( "is_allowed_use" );
		}

		public override TagCompound Save() {
			var itemInfo = item.GetGlobalItem<UmbralCowlItemInfo>();
			return new TagCompound { { "is_allowed_use", (bool)itemInfo.IsAllowed } };
		}


		////////////////
		
		public override void UpdateAccessory( Player player, bool hideVisual ) {
			if( player.whoAmI != Main.myPlayer ) { return; }    // Current player only

			var itemInfo = item.GetGlobalItem<UmbralCowlItemInfo>( this.mod );
			if( !itemInfo.IsAllowed ) { return; }	// Allowed to use

			if( ShadowWalkerBuff.CanShadowWalk( player ) ) {
				if( ShadowWalkerBuff.FindBuffIndex( this.mod, player ) == -1 ) {
					ShadowWalkerBuff.AddBuffFor( this.mod, player );
				}

				var myplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );
				if( myplayer.Noclip != null ) {
					myplayer.Noclip.UpdateMode( player );   // Redundant?
				}
			}
		}
	}



	class UmbralCowlItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }
		//public override bool CloneNewInstances { get { return true; } }

		public bool IsAllowed = false;

		public override GlobalItem Clone( Item item, Item itemClone ) {
			var clone = (UmbralCowlItemInfo)base.Clone( item, itemClone );
			clone.IsAllowed = this.IsAllowed;
			return clone;
		}


		public override void NetSend( Item item, BinaryWriter writer ) {
			writer.Write( this.IsAllowed );
		}

		public override void NetReceive( Item item, BinaryReader reader ) {
			this.IsAllowed = reader.ReadBoolean();
		}
	}
}
