using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.Debug;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheLunatic.NPCs {
	partial class TheLunaticTownNPC : ModNPC {
		public override void SetupShop( Chest shop, ref int nextSlot ) {
			var mymod = (TheLunaticMod)this.mod;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			bool strict = mymod.Config.LoonyEnforcesBossSequence;
			bool downedMech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
			bool downedTowers = NPC.downedTowerSolar && NPC.downedTowerVortex && NPC.downedTowerNebula && NPC.downedTowerStardust;

			// Bake sale!
			Item sugarCookie = new Item();
			Item gingerbreadCookie = new Item();
			Item christmasPudding = new Item();
			Item pumpkinPie = new Item();
			Item cookedMarshmallow = new Item();

			sugarCookie.SetDefaults( 1919 );
			gingerbreadCookie.SetDefaults( 1920 );
			christmasPudding.SetDefaults( 1911 );
			pumpkinPie.SetDefaults( 1787 );
			cookedMarshmallow.SetDefaults( 969 );

			sugarCookie.value *= 8;
			gingerbreadCookie.value *= 8;
			christmasPudding.value *= 9;
			pumpkinPie.value *= 9;
			cookedMarshmallow.value *= 5;
			
			sugarCookie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			gingerbreadCookie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			christmasPudding.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			pumpkinPie.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";
			cookedMarshmallow.GetGlobalItem<TheLunaticItem>().AddedTooltip = "Bake sale!";

			shop.item[nextSlot++] = sugarCookie;
			shop.item[nextSlot++] = gingerbreadCookie;
			shop.item[nextSlot++] = christmasPudding;
			shop.item[nextSlot++] = pumpkinPie;
			shop.item[nextSlot++] = cookedMarshmallow;

			for( int i=0; i<15; i++ ) {
				shop.item[nextSlot++] = new Item();
			}

			// Boss summon items
			if( mymod.Config.LoonySellsSummonItems ) {
				// Eye of Cthulhu
				if( /*NPC.downedBoss1*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EyeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 43 );		// Suspicious Looking Eye
					summonItem.value = 150000;
					shop.item[nextSlot++] = summonItem;
				}
				// King Slime
				if( /*NPC.downedSlimeKing*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.KingSlimeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 560 );    // Slime Crown
					summonItem.value = 180000;
					shop.item[nextSlot++] = summonItem;
				}
				// Queen Bee
				if( /*NPC.downedQueenBee &&*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BeeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1133 );    // Abeemination
					summonItem.value = 200000;
					shop.item[nextSlot++] = summonItem;
				}
				// Brain of Cthulhu
				if( /*NPC.downedBoss2 && WorldGen.crimson &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BrainMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1331 );	// Bloody Spine
					summonItem.value = 260000;
					shop.item[nextSlot++] = summonItem;
				}
				// Eater of Worlds
				if( /*NPC.downedBoss2 && !WorldGen.crimson &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.EaterMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 70 );    // Worm Food
					summonItem.value = 250000;
					shop.item[nextSlot++] = summonItem;
				}
				// Skeletron
				if( /*NPC.downedBoss3*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1307 );    // Clothier Voodoo Doll
					summonItem.value = 300000;
					shop.item[nextSlot++] = summonItem;
				}
				// Wall of Flesh
				if( /*Main.hardMode*/ myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.FleshMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 267 );    // Guide Voodoo Doll
					summonItem.value = 320000;
					shop.item[nextSlot++] = summonItem;
				}
				// Destroyer
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss1 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DestroyerMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 556 );    // Mechanical Worm
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Twins
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedMechBoss2 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.TwinMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 544 );    // Mechanical Worm
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Skeletron Prime
				if( (!strict ||(Main.hardMode)) &&
						/*NPC.downedMechBoss3 &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.SkeletronPrimeMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 557 );    // Mechanical Skull
					summonItem.value = 1000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Golem
				if( (!strict || (Main.hardMode && downedMech && NPC.downedPlantBoss)) &&
						/*NPC.downedGolemBoss &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.GolemMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 1293 );    // Lihzahrd Power Cell
					summonItem.value = 2000000;
					shop.item[nextSlot++] = summonItem;
				}
				// Duke Fishron
				if( (!strict || (Main.hardMode)) &&
						/*NPC.downedFishron &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.DukeFishronMask ) ) {
					Item summonItem = new Item();
					summonItem.SetDefaults( 2673 );    // Truffle Worm
					summonItem.value = 3500000;
					shop.item[nextSlot++] = summonItem;
				}
				// Moon Lord
				if( (!strict || (Main.hardMode && downedMech && NPC.downedPlantBoss && NPC.downedGolemBoss && NPC.downedAncientCultist)) &&
						/*NPC.downedMoonlord &&*/
						myworld.MaskLogic.GivenVanillaMasksByType.Contains( ItemID.BossMaskMoonlord ) ) {    //&& downed_towers
					Item summonItem = new Item();
					summonItem.SetDefaults( 3601 );    // Celestial Sigil
					summonItem.value = 8000000;
					shop.item[nextSlot++] = summonItem;
				}
			}
		}
	}
}