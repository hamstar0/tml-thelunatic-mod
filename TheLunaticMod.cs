using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using TheLunatic.Tiles;
using Terraria.ID;
using System;
using TheLunatic.NetProtocol;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.TModLoader.Mods;


namespace TheLunatic {
	partial class TheLunaticMod : Mod {
		public static TheLunaticMod Instance { get; private set; }



		////////////////
		
		public LunaticConfig Config => ModContent.GetInstance<LunaticConfig>();

		internal AnimatedSky Sky { get; private set; }



		////////////////

		public TheLunaticMod() {
			TheLunaticMod.Instance = this;
		}

		////////////////

		public override void Load() {
			if( !Main.dedServ ) {
				this.Sky = new AnimatedSky();
				SkyManager.Instance["TheLunaticMod:AnimatedColorize"] = this.Sky;
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
			var myworld = ModContent.GetInstance<TheLunaticWorld>();
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
			if( this.Config == null || !this.Config.Enabled ) { return; }

			var myworld = ModContent.GetInstance<TheLunaticWorld>();
			if( myworld.GameLogic != null ) {
				myworld.GameLogic.ReadyClient = true;  // Ugh!
			}
		}

		public override void UpdateMusic( ref int music, ref MusicPriority priority ) {
			if( this.Config == null || !this.Config.Enabled ) { return; }

			var myworld = ModContent.GetInstance<TheLunaticWorld>();

			if( myworld != null && myworld.GameLogic != null ) {
				myworld.GameLogic.UpdateMyMusic( ref music );
			}
		}


		////////////////

		public override object Call( params object[] args ) {
			return ModBoilerplateHelpers.HandleModCall( typeof(TheLunaticAPI), args );
		}
	}
}
