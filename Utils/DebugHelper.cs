using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace Utils {
	public static class DebugHelper {
		public static int DEBUGMODE = 0;	// 1: Display info, 2: Fast time, 4: Reset, 8: Clear win, 16: Skip to signs
		public static bool Once;
		public static int OnceInAWhile;
		public static IDictionary<string, string> Display = new Dictionary<string, string>();

		private static int Logged = 0;


		public static void MsgOnce( string msg ) {
			if( DebugHelper.Once ) { return; }
			DebugHelper.Once = true;

			Main.NewText( msg );
		}

		public static void MsgOnceInAWhile( string msg ) {
			if( DebugHelper.OnceInAWhile > 0 ) { return; }
			DebugHelper.OnceInAWhile = 60 * 10;

			Main.NewText( msg );
		}

		
		public static void PrintToBatch( SpriteBatch sb ) {
			int i = 0;

			foreach( string key in DebugHelper.Display.Keys.ToList() ) {
				string msg = key + ":  " + DebugHelper.Display[key];
				sb.DrawString( Main.fontMouseText, msg, new Vector2( 8, (Main.screenHeight - 32) - (i * 24) ), Color.White );

				//Debug.Display[key] = "";
				i++;
			}
		}
		

		public static void Log( string msg ) {
			string now = DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1, 0, 0, 0 ) ).TotalSeconds.ToString( "N2" );
			string logged = "" + Main.netMode + ":" + DebugHelper.Logged + " " + now;
			logged += new String( ' ', 24 - logged.Length );

			ErrorLogger.Log( logged+msg );

			DebugHelper.Logged++;
		}
	}
}
