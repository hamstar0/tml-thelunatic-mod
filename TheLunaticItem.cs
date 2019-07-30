using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;


namespace TheLunatic {
	class TheLunaticItem : GlobalItem {
		public override bool InstancePerEntity => true;
		//public override bool CloneNewInstances { get { return true; } }


		////////////////

		public string AddedTooltip = "";



		////////////////

		public override GlobalItem Clone( Item item, Item itemClone ) {
			var clone = (TheLunaticItem)base.Clone( item, itemClone );
			clone.AddedTooltip = this.AddedTooltip;
			return clone;
		}

		////////////////

		public override void ModifyTooltips( Item item, List<TooltipLine> tooltips ) {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

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
			var mymod = (TheLunaticMod)this.mod;
			var myplayer = player.GetModPlayer<TheLunaticPlayer>( mymod );

			if( !mymod.ConfigJson.Data.Enabled ) { return; }

			if( myplayer.Noclip.IsOn ) {
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
