using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Messages;
using HamstarHelpers.WorldHelpers;
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


		public void LoadOnce( TheLunaticMod mymod, bool has_loony_arrived, bool has_loony_quit, bool has_game_ended, bool has_won, bool is_safe, int time ) {
			var modworld = mymod.GetModWorld<MyModWorld>();

			if( this.IsLoaded ) {
				DebugHelpers.Log( "Redundant Game Logic load. " + has_loony_arrived + "," + has_loony_quit + "," + has_game_ended + "," + has_won + "," + is_safe + "," + time +
					"   (" + this.HasLoonyArrived + "," + this.HasLoonyQuit + "," + this.HasGameEnded + "," + this.HasWon + "," + this.IsSafe + "," + this.HalfDaysElapsed + ") " +
					modworld.ID );
				return;
			}
			if( mymod.IsDisplayInfoDebugMode() ) {
				DebugHelpers.Log( "DEBUG Game Logic loading. " + has_loony_arrived + "," + has_loony_quit + "," + has_game_ended + "," + has_won + "," + is_safe + "," + time +
					"   (" + this.HasLoonyArrived + "," + this.HasLoonyQuit + "," + this.HasGameEnded + "," + this.HasWon + "," + this.IsSafe + "," + this.HalfDaysElapsed + ") " +
					modworld.ID );
			}

			this.HasLoonyArrived = has_loony_arrived;
			this.HasLoonyQuit = has_loony_quit;
			this.HasGameEnded = has_game_ended;
			this.HasWon = has_won;
			this.IsSafe = is_safe;
			this.HalfDaysElapsed = time;

			this.ApplyDebugOverrides( mymod );

			this.IsLoaded = true;
		}
		private bool IsLoaded = false;


		public void AllowReload() {
			this.IsLoaded = false;
		}

		public void ApplyDebugOverrides( TheLunaticMod mymod ) {
			if( mymod.IsFastTimeDebugMode() ) {
				mymod.Config.Data.DaysUntil /= 5;
			}
			if( mymod.IsResetDebugMode() ) {
				DebugHelpers.Log( "DEBUG Game Logic reset!" );
				this.HasLoonyArrived = false;
				this.HasGameEnded = false;
				this.HasLoonyQuit = false;
				this.HasWon = false;
				this.IsSafe = false;
				this.HalfDaysElapsed = 0;
			}
			if( mymod.IsSkipToSignsDebugMode() ) {
				if( this.HalfDaysElapsed < mymod.Config.Data.DaysUntil ) {
					this.HalfDaysElapsed = mymod.Config.Data.DaysUntil;
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

		
		public void Update( TheLunaticMod mymod ) {
			// Simply idle (and keep track of day) until ready
			if( !this.IsReady() ) {
				this.IsDay = Main.dayTime;
				return;
			}
			
			if( mymod.IsDisplayInfoDebugMode() ) {
				var modworld = mymod.GetModWorld<MyModWorld>();
				DebugHelpers.Display["WorldID"] = "" + modworld.ID;
				DebugHelpers.Display["IsApocalypse"] = "" + this.IsApocalypse;
				DebugHelpers.Display["IsSafe"] = "" + this.IsSafe;
				DebugHelpers.Display["HasLoonyArrived"] = "" + this.HasLoonyArrived;
				DebugHelpers.Display["HasLoonyQuit"] = "" + this.HasLoonyQuit;
				DebugHelpers.Display["HasGameEnded"] = "" + this.HasGameEnded;
				DebugHelpers.Display["HalfDaysElapsed"] = "" + this.HalfDaysElapsed + " (" + (mymod.Config.Data.DaysUntil * 2) + ")";
				DebugHelpers.Display["HaveWeEndSigns"] = "" + this.HaveWeEndSigns( mymod );
				DebugHelpers.Display["HaveHope"] = "" + this.HaveWeHopeToWin( mymod );
				DebugHelpers.Display["TintScale"] = "" + mymod.Sky.TintScale;
				DebugHelpers.Display["RemainingMasks"] = String.Join( ",", modworld.MaskLogic.GetRemainingVanillaMasks() );
				DebugHelpers.Display["GivenMasks"] = String.Join( ",", modworld.MaskLogic.GivenVanillaMasksByType );
			}
			if( mymod.IsFastTimeDebugMode() ) {
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

					this.UpdateEndSigns( mymod );
				}
				
				this.UpdateStateOfApocalypse( mymod );
			}
			
			this.IsDay = Main.dayTime;

			// Is loony gone for good?
			if( !this.HasLoonyQuit ) {
				if( !this.HasLoonyArrived ) {
					if( !TheLunaticTownNPC.WantsToSpawnAnew(mymod) ) {
						this.Quit();
					}
				} else if( !TheLunaticTownNPC.WantsToSpawn(mymod) ) {
					this.Quit();
				}
			}

			// Have we won?
			if( !this.HasWon && this.HaveWeWon(mymod) ) {
				this.WinTheGame();
			}

			// Indicate final day
			if( !this.HasGameEnded ) {
				if( !this.IsLastDay ) {
					if( this.HalfDaysElapsed >= (mymod.Config.Data.DaysUntil - 1) * 2 ) {
						SimpleMessage.PostMessage( "Final Day", "", 60 * 5 );
						this.IsLastDay = true;
					}
				} else {
					if( this.HalfDaysElapsed < (mymod.Config.Data.DaysUntil - 1) * 2 ) {
						this.IsLastDay = false;
					}
				}
			}
		}


		public void UpdateMyMusic( TheLunaticMod mymod, ref int music ) {
			Player player = Main.player[Main.myPlayer];
			var modplayer = player.GetModPlayer<MyModPlayer>( mymod );

			if( this.IsApocalypse ) {
				if( modplayer.IsInDangerZone ) {
					music = 17;
				}
			}
		}

		public void UpdateBiomes( TheLunaticMod mymod, Player player ) {
			if( this.IsApocalypse ) {
				var modplayer = player.GetModPlayer<MyModPlayer>( mymod );

				if( modplayer.IsInDangerZone ) {
					player.ZoneTowerSolar = true;
					player.ManageSpecialBiomeVisuals( "Solar", true, default( Vector2 ) );
				} else {
					player.ZoneTowerSolar = false;
				}
			}
		}

		public void UpdateBiomeVisuals( TheLunaticMod mymod, Player player ) {
			if( this.IsApocalypse ) {
				var modplayer = player.GetModPlayer<MyModPlayer>( mymod );
				
				if( modplayer.IsInDangerZone ) {
					Filters.Scene.Activate( "HeatDistortion", player.Center, new object[0] );
					Filters.Scene["HeatDistortion"].GetShader().UseIntensity( 2f );
					Filters.Scene["HeatDistortion"].IsHidden = false;
					Main.UseHeatDistortion = true;
				}
			}
		}

		private void UpdateEndSigns( TheLunaticMod mymod ) {
			if( this.HaveWeEndSigns(mymod) ) {
				int half_days_left = (mymod.Config.Data.DaysUntil * 2) - this.HalfDaysElapsed;
				int rand = Main.rand.Next( half_days_left * 60 * 54 );
				
				if( Main.netMode != 1 && rand == 0 ) {	// Not client
					int duration = (int)(120 + (60 * 4 * Main.rand.NextFloat()));

					if( Main.netMode == 2 ) {	// Server
						ServerPacketHandlers.BroadcastEndSignFromServer( mymod, duration );
					} else if( Main.netMode == 0 ) {	// Single-player
						this.ApplyEndSignForMe( mymod, duration );
					}
				}

				if( Main.netMode != 2 ) {   // Not server
					if( half_days_left != 0 ) {
						double days = (double)this.HalfDaysElapsed + WorldHelpers.GetDayOrNightPercentDone();
						days -= mymod.Config.Data.DaysUntil;
						mymod.Sky.TintScale = (float)days / (float)mymod.Config.Data.DaysUntil;
					} else {
						mymod.Sky.TintScale = 0;
					}
				}
			} else if( Main.netMode != 2 ) {	// Not server
				mymod.Sky.TintScale = 0;
			}
		}

		private void UpdateStateOfApocalypse( TheLunaticMod mymod ) {
			// Time's up?
			if( !this.IsApocalypse && this.HaveWeApocalypse(mymod) ) {
				this.BeginApocalypse();
			}

			if( this.IsApocalypse ) {
				if( Main.netMode != 2 ) { // Not server
					Player player = Main.player[Main.myPlayer];
					var modplayer = player.GetModPlayer<MyModPlayer>( mymod );

					modplayer.QuakeMeFor( -1, 0.1f );	// Perpetual rumble
				}
			}
		}


		////////////////

		public bool HaveWeEndSigns( TheLunaticMod mymod ) {
			return !this.HaveWeWon(mymod)
				//&& this.HasLoonyBegun
				&& !this.IsApocalypse
				//&& !this.HasLoonyQuit
				&& !this.IsSafe
				&& (this.HalfDaysElapsed >= mymod.Config.Data.DaysUntil);
		}

		public bool HaveWeHopeToWin( TheLunaticMod mymod ) {
			return !this.HasLoonyQuit
				&& !this.IsApocalypse
				&& !this.HaveWeWon(mymod);
		}

		private bool HaveWeApocalypse( TheLunaticMod mymod ) {
			int half_days_left = (mymod.Config.Data.DaysUntil * 2) - this.HalfDaysElapsed;

			return this.IsApocalypse || ( half_days_left == 0
				&& !this.IsSafe
				&& !this.HaveWeWon(mymod) );
		}

		private bool HaveWeWon( TheLunaticMod mymod ) {
			var modworld = mymod.GetModWorld<MyModWorld>();

			return this.HasWon || ( this.HasLoonyArrived
				//&& this.IsSafe
				&& !this.IsApocalypse
				&& modworld.MaskLogic.GivenVanillaMasksByType.Count >= MaskLogic.AvailableMaskCount );
		}


		////////////////

		public void SetTime( int half_days ) {
			this.HalfDaysElapsed = half_days;
		}

		public void ApplyEndSignForMe( TheLunaticMod mymod, int duration ) {
			if( Main.netMode == 2 ) { return; }	// Not server

			Player player = Main.player[Main.myPlayer];
			var modplayer = player.GetModPlayer<MyModPlayer>( mymod );

			// Quake
			modplayer.QuakeMeFor( duration, 0.35f );
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
