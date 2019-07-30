using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.Debug;
using Terraria;
using Terraria.ModLoader;


namespace TheLunatic.NPCs {
	partial class TheLunaticTownNPC : ModNPC {
		public static bool WantsToSpawnAnew() {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }

			bool canSpawn = !myworld.GameLogic.HasLoonyQuit;

			if( canSpawn && mymod.Config.LoonyEnforcesBossSequence ) {
				// Allow spawning if moon lord gone (shop use only)
				if( !NPC.downedMoonlord ) {
					//if( NPC.downedSlimeKing ) { can_spawn = false; }		// King Slime
					if( NPC.downedBoss1 ) { canSpawn = false; }            // Eye of Cthulhu
					if( NPC.downedQueenBee ) { canSpawn = false; }         // Queen Bee
					if( NPC.downedBoss2 ) { canSpawn = false; }            // Brain of Cthulhu && Eater of Worlds
					if( NPC.downedBoss3 ) { canSpawn = false; }            // Skeletron
					if( Main.hardMode ) { canSpawn = false; }              // Wall of Flesh
					if( NPC.downedMechBoss1 ) { canSpawn = false; }        // Destroyer
					if( NPC.downedMechBoss2 ) { canSpawn = false; }        // Twin
					if( NPC.downedMechBoss3 ) { canSpawn = false; }        // Skeletron Prime
					if( NPC.downedFishron ) { canSpawn = false; }          // Duke Fishron
					if( NPC.downedPlantBoss ) { canSpawn = false; }        // Plantera
					if( NPC.downedGolemBoss ) { canSpawn = false; }        // Golem
					if( NPC.downedAncientCultist ) { canSpawn = false; }   // Ancient Cultist
				}
			}

			if( !canSpawn ) {
				LogHelpers.LogOnce( "Could not spawn Loony anew - "
					+ "Quit? " + myworld.GameLogic.HasLoonyQuit
					+ ", downedBoss1? " + NPC.downedBoss1
					+ ", downedQueenBee? " + NPC.downedQueenBee
					+ ", downedBoss2? " + NPC.downedBoss2
					+ ", downedBoss3? " + NPC.downedBoss3
					+ ", hardMode? " + Main.hardMode
					+ ", downedMechBoss1? " + NPC.downedMechBoss1
					+ ", downedMechBoss2? " + NPC.downedMechBoss2
					+ ", downedMechBoss3? " + NPC.downedMechBoss3
					+ ", downedFishron? " + NPC.downedFishron
					+ ", downedPlantBoss? " + NPC.downedPlantBoss
					+ ", downedGolemBoss? " + NPC.downedGolemBoss
					+ ", downedAncientCultist? " + NPC.downedAncientCultist
				);
			}
			
			return canSpawn;
		}

		public static bool WantsToSpawn() {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized."); }

			bool canSpawn = !myworld.GameLogic.HasLoonyQuit;
			bool validHardMode=canSpawn, validMechBosses=canSpawn, validPlantera=canSpawn, validGolem=canSpawn, validAncientCultist=canSpawn;

			if( mymod.Config.LoonyEnforcesBossSequence ) {
				if( canSpawn && !Main.hardMode ) {
					validHardMode = !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && !NPC.downedFishron
						&& !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
					canSpawn = validHardMode;
				}
				if( canSpawn && !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 ) {
					validMechBosses = !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
					canSpawn = validMechBosses;
				}
				if( canSpawn && !NPC.downedPlantBoss ) {
					validPlantera = !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord;
					canSpawn = validPlantera;
				}
				if( canSpawn && !NPC.downedGolemBoss ) {
					validGolem = !NPC.downedAncientCultist && !NPC.downedMoonlord;
					canSpawn = validGolem;
				}
				if( canSpawn && !NPC.downedAncientCultist ) {
					validAncientCultist = !NPC.downedMoonlord;
					canSpawn = validAncientCultist;
				}
			}

			if( !canSpawn ) {
				LogHelpers.LogOnce( "Could not spawn Loony anew - "
					+ "Quit? " + myworld.GameLogic.HasLoonyQuit
					+ ", validHardMode? " + validHardMode
					+ ", validMechBosses? " + validMechBosses
					+ ", validPlantera? " + validPlantera
					+ ", validGolem? " + validGolem
					+ ", validAncientCultist? " + validAncientCultist
				);
			}

			//if( _debug ) {
			//Main.NewText( "WantsToSpawn:"+ can_spawn+" mech1:"+ NPC.downedMechBoss1 + " mech2:"+ NPC.downedMechBoss2+" mech3:"+ NPC.downedMechBoss3
			//	+" fish:"+ NPC.downedFishron+" plant:"+ NPC.downedPlantBoss+" golem:"+ NPC.downedGolemBoss+" cult:"+ NPC.downedAncientCultist
			//	+" moon:"+ NPC.downedMoonlord );
			//}
			return canSpawn;
		}



		////////////////
		
		public override bool CanTownNPCSpawn( int numTownNPCs, int money ) {
			var myworld = mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException("Game logic not initialized."); }

			if( myworld.GameLogic.KillSurfaceTownNPCs ) { return false; }
			return TheLunaticTownNPC.WantsToSpawn();
		}
	}
}