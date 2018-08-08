using Terraria;
using Terraria.ModLoader;
using TheLunatic.Buffs;


namespace TheLunatic {
	class TheLunaticProjectile : GlobalProjectile {
		public override bool? CanUseGrapple( int type, Player player ) {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { base.CanUseGrapple( type, player ); }

			var buff = (ShadowWalkerBuff)mymod.GetBuff( "ShadowWalkerBuff" );
			bool? can_we = buff.ProjectileCanUseGrapple( type, player );
			if( can_we != null ) {
				return (bool)can_we;
			}
			return base.CanUseGrapple( type, player );
		}
	}
}
