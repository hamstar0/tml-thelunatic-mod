using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using TheLunatic.NetProtocol;
using TheLunatic.NPCs;


namespace TheLunatic.Logic {
	class GameLogic {
		public bool ReadyServer = false;
		public bool ReadyClient = false;

		public int HalfDaysElapsed { get; private set; }
		public bool KillSurfaceTownNPCs { get; private set; }
		
		public bool HasLoonyArrived { get; private set; }
		public bool HasLoonyQuit { get; private set; }
		public bool HasGameEnded { get; private set; }
		public bool HasWon { get; private set; }
		public bool IsSafe { get; private set; }
		public bool IsLastDay { get; private set; }

		public bool IsApocalypse { get; private set; }

		private bool IsDay;


		////////////////

		public GameLogic() {
			this.HasLoonyArrived = false;
			this.HasLoonyQuit = false;
			this.HasGameEnded = false;
			this.HasWon = false;
			this.IsSafe = false;
			this.HalfDaysElapsed = 0;
			this.IsLastDay = false;
			
			this.IsDay = Main.dayTime;
		}


		public void LoadOnce( bool hasLoonyArrived, bool hasLoonyQuit, bool hasGameEnded, bool hasWon, bool isSafe, int time ) {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();

			if( this.IsLoaded ) {
				LogHelpers.Log( "Redundant Game Logic load. " + hasLoonyArrived + "," + hasLoonyQuit + "," + hasGameEnded + "," + hasWon + "," + isSafe + "," + time +
					"   (" + this.HasLoonyArrived + "," + this.HasLoonyQuit + "," + this.HasGameEnded + "," + this.HasWon + "," + this.IsSafe + "," + this.HalfDaysElapsed + ") " +
					myworld.ID );
				return;
			}
			if( mymod.Config.DebugModeInfo ) {
				LogHelpers.Log( "DEBUG Game Logic loading. " + hasLoonyArrived + "," + hasLoonyQuit + "," + hasGameEnded + "," + hasWon + "," + isSafe + "," + time +
					"   (" + this.HasLoonyArrived + "," + this.HasLoonyQuit + "," + this.HasGameEnded + "," + this.HasWon + "," + this.IsSafe + "," + this.HalfDaysElapsed + ") " +
					myworld.ID );
			}

			this.HasLoonyArrived = hasLoonyArrived;
			this.HasLoonyQuit = hasLoonyQuit;
			this.HasGameEnded = hasGameEnded;
			this.HasWon = hasWon;
			this.IsSafe = isSafe;
			this.HalfDaysElapsed = time;

			this.ApplyDebugOverrides( mymod );

			this.IsLoaded = true;
		}
		private bool IsLoaded = false;


		public void AllowReload() {
			this.IsLoaded = false;
		}

		public void ApplyDebugOverrides( TheLunaticMod mymod ) {
			if( mymod.Config.DebugModeFastTime ) {
				mymod.Config.DaysUntil /= 5;
			}
			if( mymod.Config.DebugModeReset ) {
				LogHelpers.Log( "DEBUG Game Logic reset!" );
				this.HasLoonyArrived = false;
				this.HasGameEnded = false;
				this.HasLoonyQuit = false;
				this.HasWon = false;
				this.IsSafe = false;
				this.HalfDaysElapsed = 0;
			}
			if( mymod.Config.DebugModeSkipToSigns ) {
				if( this.HalfDaysElapsed < mymod.Config.DaysUntil ) {
					this.HalfDaysElapsed = mymod.Config.DaysUntil;
				}
			}
		}


		////////////////

		public bool IsReady() {
			if( Main.netMode == 1 && !this.IsLoaded ) {  // Client
				return false;
			}
			if( Main.netMode != 2 && !this.ReadyClient ) {  // Client or single
				return false;
			}
			if( Main.netMode == 2 && !this.ReadyServer ) {  // Server
				return false;
			}
			if( this.StartupDelay++ < 60*2 ) {	// UGH!!!!!!
				return false;
			}
			return true;
		}
		private int StartupDelay = 0;

		
		public void Update() {
			var mymod = TheLunaticMod.Instance;

			// Simply idle (and keep track of day) until ready
			if( !this.IsReady() ) {
				this.IsDay = Main.dayTime;
				return;
			}

			if( mymod.Config.DebugModeInfo ) {
				var modworld = mymod.GetModWorld<TheLunaticWorld>();
				DebugHelpers.Print( "WorldID", "" + modworld.ID, 20 );
				DebugHelpers.Print( "IsApocalypse", "" + this.IsApocalypse, 20 );
				DebugHelpers.Print( "IsSafe", "" + this.IsSafe, 20 );
				DebugHelpers.Print( "HasLoonyArrived", "" + this.HasLoonyArrived, 20 );
				DebugHelpers.Print( "HasLoonyQuit", "" + this.HasLoonyQuit, 20 );
				DebugHelpers.Print( "HasGameEnded", "" + this.HasGameEnded, 20 );
				DebugHelpers.Print( "HalfDaysElapsed", "" + this.HalfDaysElapsed + " (" + (mymod.Config.DaysUntil * 2) + ")", 20 );
				DebugHelpers.Print( "HaveWeEndSigns", "" + this.HaveWeEndSigns(), 20 );
				DebugHelpers.Print( "HaveHope", "" + this.HaveWeHopeToWin(), 20 );
				DebugHelpers.Print( "TintScale", "" + mymod.Sky.TintScale, 20 );
				DebugHelpers.Print( "RemainingMasks", String.Join( ",", modworld.MaskLogic.GetRemainingVanillaMasks() ), 20 );
				DebugHelpers.Print( "GivenMasks", String.Join( ",", modworld.MaskLogic.GivenVanillaMasksByType ), 20 );
			}
			if( mymod.Config.DebugModeFastTime ) {
				Main.time += 24;
			}

			// Indicate when our loony is here to stay
			if( !this.HasLoonyArrived && TheLunaticTownNPC.AmHere ) {
				this.HasLoonyArrived = true;
			}
			
			if( !this.IsSafe && !this.HasLoonyQuit ) {
				if( !this.IsApocalypse && !NPC.LunarApocalypseIsUp ) {	//!NPC.downedAncientCultist && NPC.MoonLordCountdown == 0 && !NPC.AnyNPCs(398)
					// Advance elapsed time until the end
					if( this.IsDay != Main.dayTime ) {
						this.HalfDaysElapsed++;
					}

					this.UpdateEndSigns();
				}
				
				this.UpdateStateOfApocalypse();
			}
			
			this.IsDay = Main.dayTime;

			// Is loony gone for good?
			if( !this.HasLoonyQuit ) {
				if( !this.HasLoonyArrived ) {
					if( !TheLunaticTownNPC.WantsToSpawnAnew() ) {
						this.Quit();
					}
				} else if( !TheLunaticTownNPC.WantsToSpawn() ) {
					this.Quit();
				}
			}

			// Have we won?
			if( !this.HasWon && this.HaveWeWon() ) {
				this.WinTheGame();
			}

			// Indicate final day
			if( !this.HasGameEnded ) {
				if( !this.IsLastDay ) {
					if( this.HalfDaysElapsed >= (mymod.Config.DaysUntil - 1) * 2 ) {
						SimpleMessage.PostMessage( "Final Day", "", 60 * 5 );
						this.IsLastDay = true;
					}
				} else {
					if( this.HalfDaysElapsed < (mymod.Config.DaysUntil - 1) * 2 ) {
						this.IsLastDay = false;
					}
				}
			}
		}


		public void UpdateMyMusic( ref int music ) {
			var mymod = TheLunaticMod.Instance;
			Player player = Main.player[Main.myPlayer];
			var myplayer = player.GetModPlayer<TheLunaticPlayer>( mymod );

			if( this.IsApocalypse ) {
				if( myplayer.IsInDangerZone ) {
					music = 17;
				}
			}
		}

		public void UpdateBiomes( Player player ) {
			if( this.IsApocalypse ) {
				var myplayer = player.GetModPlayer<TheLunaticPlayer>();

				if( myplayer.IsInDangerZone ) {
					player.ZoneTowerSolar = true;
					player.ManageSpecialBiomeVisuals( "Solar", true, default( Vector2 ) );
				} else {
					player.ZoneTowerSolar = false;
				}
			}
		}

		public void UpdateBiomeVisuals( Player player ) {
			if( this.IsApocalypse ) {
				var myplayer = player.GetModPlayer<TheLunaticPlayer>();
				
				if( myplayer.IsInDangerZone ) {
					Filters.Scene.Activate( "HeatDistortion", player.Center, new object[0] );
					Filters.Scene["HeatDistortion"].GetShader().UseIntensity( 2f );
					Filters.Scene["HeatDistortion"].IsHidden = false;
					Main.UseHeatDistortion = true;
				}
			}
		}

		private void UpdateEndSigns() {
			var mymod = TheLunaticMod.Instance;

			if( this.HaveWeEndSigns() ) {
				int halfDaysLeft = (mymod.Config.DaysUntil * 2) - this.HalfDaysElapsed;
				int rand = Main.rand.Next( halfDaysLeft * 60 * 54 );
				
				if( Main.netMode != 1 && rand == 0 ) {	// Not client
					int duration = (int)(120 + (60 * 4 * Main.rand.NextFloat()));

					if( Main.netMode == 2 ) {	// Server
						ServerPacketHandlers.BroadcastEndSignFromServer( duration );
					} else if( Main.netMode == 0 ) {	// Single-player
						this.ApplyEndSignForMe( duration );
					}
				}

				if( Main.netMode != 2 ) {   // Not server
					if( halfDaysLeft != 0 ) {
						double days = (double)this.HalfDaysElapsed + WorldStateHelpers.GetDayOrNightPercentDone();
						days -= mymod.Config.DaysUntil;
						mymod.Sky.TintScale = (float)days / (float)mymod.Config.DaysUntil;
					} else {
						mymod.Sky.TintScale = 0;
					}
				}
			} else if( Main.netMode != 2 ) {	// Not server
				mymod.Sky.TintScale = 0;
			}
		}

		private void UpdateStateOfApocalypse() {
			// Time's up?
			if( !this.IsApocalypse && this.HaveWeApocalypse() ) {
				this.BeginApocalypse();
			}

			if( this.IsApocalypse ) {
				if( Main.netMode != 2 ) { // Not server
					Player player = Main.player[Main.myPlayer];
					var myplayer = player.GetModPlayer<TheLunaticPlayer>();

					myplayer.QuakeMeFor( -1, 0.1f );	// Perpetual rumble
				}
			}
		}


		////////////////

		public bool HaveWeEndSigns() {
			var mymod = TheLunaticMod.Instance;

			return !this.HaveWeWon()
				//&& this.HasLoonyBegun
				&& !this.IsApocalypse
				//&& !this.HasLoonyQuit
				&& !this.IsSafe
				&& (this.HalfDaysElapsed >= mymod.Config.DaysUntil);
		}

		public bool HaveWeHopeToWin() {
			var mymod = TheLunaticMod.Instance;

			return !this.HasLoonyQuit
				&& !this.IsApocalypse
				&& !this.HaveWeWon();
		}

		private bool HaveWeApocalypse() {
			var mymod = TheLunaticMod.Instance;
			int halfDaysLeft = (mymod.Config.DaysUntil * 2) - this.HalfDaysElapsed;

			return this.IsApocalypse || ( halfDaysLeft == 0
				&& !this.IsSafe
				&& !this.HaveWeWon() );
		}

		private bool HaveWeWon() {
			var mymod = TheLunaticMod.Instance;
			var myworld = mymod.GetModWorld<TheLunaticWorld>();

			return this.HasWon || ( this.HasLoonyArrived
				//&& this.IsSafe
				&& !this.IsApocalypse
				&& myworld.MaskLogic.GivenVanillaMasksByType.Count >= MaskLogic.AvailableMaskCount );
		}


		////////////////

		public void SetTime( int halfDays ) {
			this.HalfDaysElapsed = halfDays;
		}

		public void ApplyEndSignForMe( int duration ) {
			if( Main.netMode == 2 ) { return; }	// Not server

			Player player = Main.player[Main.myPlayer];
			var myplayer = player.GetModPlayer<TheLunaticPlayer>();

			// Quake
			myplayer.QuakeMeFor( duration, 0.35f );
		}

		public void BeginApocalypse() {
			this.IsApocalypse = true;
			//this.HasLoonyQuit = true;	// Loony has nothing more to do here?
			this.KillSurfaceTownNPCs = true;    // Everything not underground dies

			string str = "The end has come!";

			if( Main.netMode == 0 ) {   // Single
				Main.NewText( str, 192, 0, 48, false );
			} else {    // Server
				NetMessage.SendData( 25, -1, -1, NetworkText.FromLiteral(str), 255, 192f, 0f, 48f, 0, 0, 0 );
			}
		}

		public void EndApocalypse() {
			this.IsApocalypse = false;
			this.KillSurfaceTownNPCs = false;
		}

		public void WinTheGame() {
			this.IsSafe = true;
			this.HasGameEnded = true;
			this.HasWon = true;
		}

		public void Quit() {
			this.IsSafe = true;
			this.HasLoonyQuit = true;
			this.HasGameEnded = true;

			string str = "You inexplicably feel like this will now be a boring adventure";

			if( Main.netMode == 0 || Main.netMode == 1 ) {	// Single or client
				Main.NewText( str, 64, 64, 96, false );
			} else if( Main.netMode == 2 ) {	// Server
				NetMessage.SendData( 25, -1, -1, NetworkText.FromLiteral(str), 255, 64f, 64f, 96f, 0, 0, 0 );
			}
		}
	}
}
