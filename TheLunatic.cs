using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using TheLunatic.Tiles;
using Terraria.ID;
using System;
using HamstarHelpers.Utilities.Config;
using HamstarHelpers.DebugHelpers;
using TheLunatic.NetProtocol;


namespace TheLunatic {
	public class ConfigurationData {
		public string VersionSinceUpdate = "";

		public bool Enabled = true;

		public int DaysUntil = 9;  // Days until The End
		public int HalfDaysRecoveredPerMask = 4;    // Half days recovered per mask
		public float WallOfFleshMultiplier = 2.5f;    // Added time for WoF kill
		public float HardModeMultiplier = 1.5f;	// Added time for hard mode bosses

		public bool LoonyEnforcesBossSequence = true;
		public bool LoonyAcceptsMasksWithoutBossKill = false;
		public bool LoonySellsSummonItems = true;
		public bool LoonyShunsCheaters = false;
		public bool LoonyGivesCompletionReward = true;
		public bool LoonyIndicatesDaysRemaining = true;

		public bool OnlyVanillaBossesDropMasks = false;
		public bool MoonLordMaskWins = false;
		
		public int DEBUGFLAGS = 0;	// 1: Display info, 2: Fast time, 4: Reset, 8: Reset win, 16: Skip to signs, 32: Display net info
	}



	public class TheLunatic : Mod {
		public readonly static Version ConfigVersion = new Version(1, 2, 5);
		public JsonConfig<ConfigurationData> Config { get; private set; }

		public AnimatedSky Sky { get; private set; }

		public int DEBUGMODE = 0;


		public TheLunatic() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			string filename = "The Lunatic Config.json";
			this.Config = new JsonConfig<ConfigurationData>( filename, "Mod Configs", new ConfigurationData() );
		}

		public override void Load() {
			var old_config = new JsonConfig<ConfigurationData>( "The Lunatic 1.0.1.json", "", new ConfigurationData() );
			// Update old config to new location
			if( old_config.LoadFile() ) {
				old_config.DestroyFile();
				old_config.SetFilePath( this.Config.FileName, "Mod Configs" );
				this.Config = old_config;
			} else if( !this.Config.LoadFile() ) {
				this.Config.SaveFile();
			} else {
				Version vers_since = this.Config.Data.VersionSinceUpdate != "" ?
					new Version( this.Config.Data.VersionSinceUpdate ) :
					new Version();

				if( vers_since < TheLunatic.ConfigVersion ) {
					var new_config = new ConfigurationData();
					ErrorLogger.Log( "The Lunatic config updated to " + TheLunatic.ConfigVersion.ToString() );
				
					if( vers_since < new Version( 1, 2, 2 ) ) {
						this.Config.Data.DaysUntil = new_config.DaysUntil;
						this.Config.Data.HardModeMultiplier = new_config.HardModeMultiplier;
						this.Config.Data.HalfDaysRecoveredPerMask = new_config.HalfDaysRecoveredPerMask;
					}

					this.Config.Data.VersionSinceUpdate = TheLunatic.ConfigVersion.ToString();
					this.Config.SaveFile();
				}
			}

			if( !Main.dedServ ) {
				this.Sky = new AnimatedSky();
				SkyManager.Instance["TheLunaticMod:AnimatedColorize"] = this.Sky;
			}

			this.DEBUGMODE = this.Config.Data.DEBUGFLAGS;
		}



		////////////////

		public override void HandlePacket( BinaryReader reader, int player_who ) {
			try {
				if( Main.netMode == 1 ) {
					ClientPacketHandlers.HandlePacket( this, reader );
				} else if( Main.netMode == 2 ) {
					ServerPacketHandlers.HandlePacket( this, reader, player_who );
				}
			} catch( Exception e ) {
				DebugHelpers.Log( "HandlePacket "+e.ToString() );
			}
		}

		public override bool HijackGetData( ref byte messageType, ref BinaryReader reader, int playerNumber ) {
			var modworld = this.GetModWorld<MyModWorld>();
			if( modworld != null && modworld.GameLogic != null ) {
				// Let not a peep of town NPC suffering be heard when set to do so
				if( modworld.GameLogic.KillSurfaceTownNPCs ) {
					if( (int)messageType == MessageID.SyncNPCName ) {
						//reader.ReadInt16();
						//reader.ReadString();
						return true;
					}
				}
			}
			return base.HijackGetData( ref messageType, ref reader, playerNumber );
		}

		////////////////

		public override void PostDrawInterface( SpriteBatch sb ) {
			if( !this.Config.Data.Enabled ) { return; }

			var modworld = this.GetModWorld<MyModWorld>();
			if( modworld.GameLogic != null ) {
				modworld.GameLogic.ReadyClient = true;  // Ugh!
			}
		}

		public override void UpdateMusic( ref int music ) {
			if( !this.Config.Data.Enabled ) { return; }

			var modworld = this.GetModWorld<MyModWorld>();

			if( modworld != null && modworld.GameLogic != null ) {
				modworld.GameLogic.UpdateMyMusic( this, ref music );
			}
		}


		////////////////

		public bool IsDisplayInfoDebugMode() {
			return (this.DEBUGMODE & 1) > 0;
		}

		public bool IsFastTimeDebugMode() {
			return (this.DEBUGMODE & 2) > 0;
		}

		public bool IsResetDebugMode() {
			return (this.DEBUGMODE & 4) > 0;
		}

		public bool IsResetWinDebugMode() {
			return (this.DEBUGMODE & 8) > 0;
		}

		public bool IsSkipToSignsDebugMode() {
			return (this.DEBUGMODE & 16) > 0;
		}

		public bool IsDisplayNetInfoDebugMode() {
			return (this.DEBUGMODE & 32) > 0;
		}
	}
}
