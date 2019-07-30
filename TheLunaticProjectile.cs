using Terraria;
using Terraria.ModLoader;
using TheLunatic.Buffs;


namespace TheLunatic {
	class TheLunaticProjectile : GlobalProjectile {
		public override bool? CanUseGrapple( int type, Player player ) {
			var mymod = (TheLunaticMod)this.mod;
			if( !mymod.Config.Enabled ) { base.CanUseGrapple( type, player ); }

			var buff = (ShadowWalkerBuff)mymod.GetBuff( "ShadowWalkerBuff" );
			bool? canWe = buff.ProjectileCanUseGrapple( type, player );
			if( canWe != null ) {
				return (bool)canWe;
			}
			return base.CanUseGrapple( type, player );
		}
	}
}
