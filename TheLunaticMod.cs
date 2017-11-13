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
	class TheLunaticMod : Mod {
		public static string GithubUserName { get { return "hamstar0"; } }
		public static string GithubProjectName { get { return "tml-thelunatic-mod"; } }

		public static string ConfigRelativeFilePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + LunaticConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			if( TheLunaticMod.Instance != null ) {
				TheLunaticMod.Instance.Config.LoadFile();
			}
		}

		public static TheLunaticMod Instance { get; private set; }


		////////////////

		public JsonConfig<LunaticConfigData> Config { get; private set; }
		internal AnimatedSky Sky { get; private set; }


		////////////////

		public TheLunaticMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			string filename = "The Lunatic Config.json";
			this.Config = new JsonConfig<LunaticConfigData>( filename, "Mod Configs", new LunaticConfigData() );
		}

		////////////////

		public override void Load() {
			TheLunaticMod.Instance = this;

			var hamhelpmod = ModLoader.GetMod( "HamstarHelpers" );
			var min_vers = new Version( 1, 0, 17 );

			if( hamhelpmod.Version < min_vers ) {
				throw new Exception( "Hamstar's Helpers must be version " + min_vers.ToString() + " or greater." );
			}

			this.LoadConfig();

			if( !Main.dedServ ) {
				this.Sky = new AnimatedSky();
				SkyManager.Instance["TheLunaticMod:AnimatedColorize"] = this.Sky;
			}
		}

		private void LoadConfig() {
			var old_config = new JsonConfig<LunaticConfigData>( "The Lunatic 1.0.1.json", "", new LunaticConfigData() );

			// Update old config to new location
			if( old_config.LoadFile() ) {
				old_config.DestroyFile();
				old_config.SetFilePath( this.Config.FileName, "Mod Configs" );
				this.Config = old_config;
			}

			try {
				if( !this.Config.LoadFile() ) {
					this.Config.SaveFile();
				}
			} catch( Exception e ) {
				DebugHelpers.Log( e.Message );
				this.Config.SaveFile();
			}

			if( this.Config.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "The Lunatic updated to " + LunaticConfigData.CurrentVersion.ToString() );
				this.Config.SaveFile();
			}
		}

		public override void Unload() {
			TheLunaticMod.Instance = null;
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
			var modworld = this.GetModWorld<MyWorld>();
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

			var modworld = this.GetModWorld<MyWorld>();
			if( modworld.GameLogic != null ) {
				modworld.GameLogic.ReadyClient = true;  // Ugh!
			}
		}

		public override void UpdateMusic( ref int music ) {
			if( !this.Config.Data.Enabled ) { return; }

			var modworld = this.GetModWorld<MyWorld>();

			if( modworld != null && modworld.GameLogic != null ) {
				modworld.GameLogic.UpdateMyMusic( this, ref music );
			}
		}


		////////////////

		public bool IsDisplayInfoDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 1) > 0;
		}
		public bool IsFastTimeDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 2) > 0;
		}
		public bool IsResetDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 4) > 0;
		}
		public bool IsResetWinDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 8) > 0;
		}
		public bool IsSkipToSignsDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 16) > 0;
		}
		public bool IsDisplayNetInfoDebugMode() {
			return (this.Config.Data.DEBUGFLAGS & 32) > 0;
		}
	}
}
