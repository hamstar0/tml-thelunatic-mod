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
using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.DotNetHelpers;


namespace TheLunatic {
	partial class TheLunaticMod : Mod {
		public static TheLunaticMod Instance { get; private set; }


		////////////////
		
		internal JsonConfig<LunaticConfigData> ConfigJson { get; private set; }
		public LunaticConfigData Config => this.ConfigJson.Data;

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
				ErrorLogger.Log( "The Lunatic updated to " + this.Version.ToString() );
				this.ConfigJson.SaveFile();
			}
		}

		public override void Unload() {
			TheLunaticMod.Instance = null;
		}


		////////////////

		public override void HandlePacket( BinaryReader reader, int playerWho ) {
			try {
				if( Main.netMode == 1 ) {
					ClientPacketHandlers.HandlePacket( reader );
				} else if( Main.netMode == 2 ) {
					ServerPacketHandlers.HandlePacket( reader, playerWho );
				}
			} catch( Exception e ) {
				LogHelpers.Log( "HandlePacket "+e.ToString() );
			}
		}

		public override bool HijackGetData( ref byte messageType, ref BinaryReader reader, int playerNumber ) {
			var myworld = this.GetModWorld<TheLunaticWorld>();
			if( myworld != null && myworld.GameLogic != null ) {
				// Let not a peep of town NPC suffering be heard when set to do so
				if( myworld.GameLogic.KillSurfaceTownNPCs ) {
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
			if( !this.ConfigJson.Data.Enabled ) { return; }

			var myworld = this.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic != null ) {
				myworld.GameLogic.ReadyClient = true;  // Ugh!
			}
		}

		public override void UpdateMusic( ref int music, ref MusicPriority priority ) {
			if( !this.ConfigJson.Data.Enabled ) { return; }

			var myworld = this.GetModWorld<TheLunaticWorld>();

			if( myworld != null && myworld.GameLogic != null ) {
				myworld.GameLogic.UpdateMyMusic( ref music );
			}
		}


		////////////////

		public override object Call( params object[] args ) {
			if( args == null || args.Length == 0 ) { throw new HamstarException( "Undefined call type." ); }

			string callType = args[0] as string;
			if( callType == null ) { throw new HamstarException( "Invalid call type." ); }

			var methodInfo = typeof( TheLunaticAPI ).GetMethod( callType );
			if( methodInfo == null ) { throw new HamstarException( "Invalid call type " + callType ); }

			var newArgs = new object[args.Length - 1];
			Array.Copy( args, 1, newArgs, 0, args.Length - 1 );

			try {
				return ReflectionHelpers.SafeCall( methodInfo, null, newArgs );
			} catch( Exception e ) {
				throw new HamstarException( "Bad API call.", e );
			}
		}
	}
}
