using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;


namespace TheLunatic {
	public class MyGlobalItem : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }
		//public override bool CloneNewInstances { get { return true; } }

		public string AddedTooltip = "";

		public override GlobalItem Clone( Item item, Item item_clone ) {
			var clone = (MyGlobalItem)base.Clone( item, item_clone );
			clone.AddedTooltip = this.AddedTooltip;
			return clone;
		}



		public override void ModifyTooltips( Item item, List<TooltipLine> tooltips ) {
			var mymod = (TheLunatic)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			bool found = item.type == this.mod.ItemType<CustomBossMaskItem>();
			if( !found ) { found = MaskLogic.AllVanillaMasks.Contains( item.type ); }
			if( found ) {
				TooltipLine line = new TooltipLine( mymod, "lunatic_info", "Contains latent spiritual essence" );
				line.overrideColor = new Color( Main.DiscoR, 64, 64 );
				tooltips.Add( line );
			}

			if( this.AddedTooltip != "" ) {
				tooltips.Add( new TooltipLine( mymod, "lunatic_added", this.AddedTooltip ) );
			}
		}


		public override void UpdateEquip( Item item, Player player ) {
			var mymod = (TheLunatic)this.mod;
			var modplayer = player.GetModPlayer<MyModPlayer>( mymod );

			if( !mymod.Config.Data.Enabled ) { return; }

			if( modplayer.Noclip.IsOn ) {
				for( int i = 0; i < 50; i++ ) {
					if( player.inventory[i] == null || player.inventory[i].IsAir || player.inventory[i].holdStyle == 0 ) {
						player.selectedItem = i;
						break;
					}
				}
			}
		}
	}
}
