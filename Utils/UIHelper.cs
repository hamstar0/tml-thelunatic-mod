using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;


namespace Utils {
	public static class UIHelper {
		public static int MessageDuration = 0;
		public static string Message = "";

		
		public static void PostMessage( string msg, int duration ) {
			UIHelper.MessageDuration = duration;
			UIHelper.Message = msg;
		}

		public static void UpdateMessageDisplay( SpriteBatch sb ) {
			if( UIHelper.MessageDuration <= 0 ) { return; }
			UIHelper.MessageDuration--;

			float scale = 2f;
			Vector2 pos = new Vector2( Main.screenWidth / 2f, Main.screenHeight / 2f );
			Vector2 size = Main.fontItemStack.MeasureString( UIHelper.Message );

			pos.X -= (size.X * scale) / 2f;
			pos.Y -= (size.Y * scale) * 2f;

			sb.DrawString( Main.fontItemStack, UIHelper.Message, pos, Color.White, 0f, new Vector2(), scale, SpriteEffects.None, 1f );
		}
	}
}
