using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;


namespace TheLunatic {
	public class TheLunaticGlobalItem : GlobalItem {
		public override void ModifyTooltips( Item item, List<TooltipLine> tooltips ) {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			bool found = item.type == this.mod.ItemType<CustomBossMaskItem>();
			if( !found ) { found = MaskLogic.AllVanillaMasks.Contains( item.type ); }
			if( !found ) { return; }
			
			TooltipLine line = new TooltipLine( mod, "lunatic_info", "Contains latent spiritual essence" );
			line.overrideColor = new Color( Main.DiscoR, 64, 64 );
			tooltips.Add( line );
		}


		public override void UpdateEquip( Item item, Player player ) {
			var mymod = (TheLunaticMod)this.mod;
			var modplayer = player.GetModPlayer<TheLunaticPlayer>( this.mod );

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
