using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using TheLunatic.Tiles;
using Utils.JsonConfig;
using Utils;
using Terraria.ID;
using System;


namespace TheLunatic {
	public class ConfigurationData {
		public string VersionSinceUpdate = "";
		public int DaysUntil = 10;  // Days until The End
		public int HalfDaysRecoveredPerMask = 6;    // Half days recovered per mask
		public float WallOfFleshMultiplier = 3f;    // Added time for WoF kill
		public float HardModeMultiplier = 1.5f;	// Added time for hard mode bosses
		public bool LoonyEnforcesBossSequence = true;
		public bool LoonyAcceptsMasksWithoutBossKill = false;
		public bool LoonySellsSummonItems = true;
		public bool LoonyShunsCheaters = false;
		public bool LoonyGivesCompletionReward = true;
	}



	public class TheLunaticMod : Mod {
		public readonly static Version ConfigVersion = new Version(1, 1, 7);
		public JsonConfig<ConfigurationData> Config { get; private set; }

		public AnimatedSky Sky { get; private set; }


		public TheLunaticMod() {
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
			}
			
			Version vers_since = this.Config.Data.VersionSinceUpdate != "" ?
				new Version( this.Config.Data.VersionSinceUpdate ) :
				new Version();

			if( vers_since < TheLunaticMod.ConfigVersion ) {
				var new_config = new ConfigurationData();
				ErrorLogger.Log( "The Lunatic config updated to " + TheLunaticMod.ConfigVersion.ToString() );

				if( vers_since < new Version(1, 1, 0) ) {
					this.Config.Data.HardModeMultiplier = new_config.HardModeMultiplier;
				}
				if( vers_since < new Version( 1, 1, 7 ) ) {
					this.Config.Data.HalfDaysRecoveredPerMask = new_config.HalfDaysRecoveredPerMask;
				}
				
				this.Config.Data.VersionSinceUpdate = TheLunaticMod.ConfigVersion.ToString();
				this.Config.SaveFile();
			}

			if( !Main.dedServ ) {
				this.Sky = new AnimatedSky();
				SkyManager.Instance["TheLunaticMod:AnimatedColorize"] = this.Sky;
			}
		}



		////////////////

		public override void HandlePacket( BinaryReader reader, int whoAmI ) {
			TheLunaticNetProtocol.RouteReceivedPackets( this, reader );
		}

		public override bool HijackGetData( ref byte messageType, ref BinaryReader reader, int playerNumber ) {
			var modworld = this.GetModWorld<TheLunaticWorld>();
			if( modworld != null && modworld.GameLogic != null ) {
				// Let not a peep of town NPC suffering be heard when set to do so
				if( modworld.GameLogic.KillSurfaceTownNPCs ) {
					if( (int)messageType == MessageID.NPCName ) {
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
			var modworld = this.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic != null ) {
				modworld.GameLogic.ReadyClient = true;  // Ugh!
			}

			DebugHelper.PrintToBatch( sb );
		}

		public override void UpdateMusic( ref int music ) {
			var modworld = this.GetModWorld<TheLunaticWorld>();
			if( modworld != null && modworld.GameLogic != null ) {
				modworld.GameLogic.UpdateMyMusic( ref music );
			}
		}
	}
}
