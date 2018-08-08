using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using TheLunatic.Tiles;
using Terraria.ID;
using System;
using HamstarHelpers.Helpers.DebugHelpers;
using TheLunatic.NetProtocol;
using HamstarHelpers.Components.Config;


namespace TheLunatic {
	partial class TheLunaticMod : Mod {
		public static TheLunaticMod Instance { get; private set; }


		////////////////
		
		public JsonConfig<LunaticConfigData> ConfigJson { get; private set; }
		public LunaticConfigData Config { get { return this.ConfigJson.Data; } }

		internal AnimatedSky Sky { get; private set; }


		////////////////

		public TheLunaticMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
			
			this.ConfigJson = new JsonConfig<LunaticConfigData>( LunaticConfigData.ConfigFileName,
				ConfigurationDataBase.RelativePath, new LunaticConfigData() );
		}

		////////////////

		public override void Load() {
			TheLunaticMod.Instance = this;

			this.LoadConfig();

			if( !Main.dedServ ) {
				this.Sky = new AnimatedSky();
				SkyManager.Instance["TheLunaticMod:AnimatedColorize"] = this.Sky;
			}
		}

		private void LoadConfig() {
			try {
				if( !this.ConfigJson.LoadFile() ) {
					this.ConfigJson.SaveFile();
				}
			} catch( Exception e ) {
				LogHelpers.Log( e.Message );
				this.ConfigJson.SaveFile();
			}

			if( this.ConfigJson.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "The Lunatic updated to " + LunaticConfigData.ConfigVersion.ToString() );
				this.ConfigJson.SaveFile();
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
				LogHelpers.Log( "HandlePacket "+e.ToString() );
			}
		}

		public override bool HijackGetData( ref byte message_type, ref BinaryReader reader, int player_number ) {
			var modworld = this.GetModWorld<TheLunaticWorld>();
			if( modworld != null && modworld.GameLogic != null ) {
				// Let not a peep of town NPC suffering be heard when set to do so
				if( modworld.GameLogic.KillSurfaceTownNPCs ) {
					if( (int)message_type == MessageID.SyncNPCName ) {
						//reader.ReadInt16();
						//reader.ReadString();
						return true;
					}
				}
			}
			return base.HijackGetData( ref message_type, ref reader, player_number );
		}

		////////////////

		public override void PostDrawInterface( SpriteBatch sb ) {
			if( !this.ConfigJson.Data.Enabled ) { return; }

			var modworld = this.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic != null ) {
				modworld.GameLogic.ReadyClient = true;  // Ugh!
			}
		}

		public override void UpdateMusic( ref int music, ref MusicPriority priority ) {
			if( !this.ConfigJson.Data.Enabled ) { return; }

			var modworld = this.GetModWorld<TheLunaticWorld>();

			if( modworld != null && modworld.GameLogic != null ) {
				modworld.GameLogic.UpdateMyMusic( this, ref music );
			}
		}
	}
}
