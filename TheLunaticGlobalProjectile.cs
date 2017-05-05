using Terraria;
using Terraria.ModLoader;
using TheLunatic.Buffs;


namespace TheLunatic {
	class TheLunaticGlobalProjectile : GlobalProjectile {
		public override bool? CanUseGrapple( int type, Player player ) {
			var buff = (ShadowWalkerBuff)this.mod.GetBuff( "ShadowWalkerBuff" );
			bool? can_we = buff.ProjectileCanUseGrapple( type, player );
			if( can_we != null ) {
				return (bool)can_we;
			}
			return base.CanUseGrapple( type, player );
		}
	}
}
