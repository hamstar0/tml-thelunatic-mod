using System.IO;
using Terraria;
using Terraria.ModLoader;
using System;
using HamstarHelpers.Components.Config;


namespace TheLunatic {
	partial class TheLunaticMod : Mod {
		public static string GithubUserName { get { return "hamstar0"; } }
		public static string GithubProjectName { get { return "tml-thelunatic-mod"; } }

		public static string ConfigFileRelativePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + LunaticConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			if( TheLunaticMod.Instance != null ) {
				if( !TheLunaticMod.Instance.ConfigJson.LoadFile() ) {
					TheLunaticMod.Instance.ConfigJson.SaveFile();
				}
			}
		}

		public static void ResetConfigFromDefaults() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reset to default configs outside of single player." );
			}

			var newConfig = new LunaticConfigData();
			//new_config.SetDefaults();

			TheLunaticMod.Instance.ConfigJson.SetData( newConfig );
			TheLunaticMod.Instance.ConfigJson.SaveFile();
		}
	}
}
