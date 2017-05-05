using Terraria;

namespace Utils {
	public static class TileHelper {
		public static float GaugeBrightness( int x, int y, int width, int height ) {
			int i = 0, j = 0;
			float avg = 0f;

			for( i=0; i<width; i++ ) {
				for( j=0; j<height; j++ ) {
					avg += Lighting.Brightness( x + i, y + j );
				}
			}

			return avg / (i * j);
		}
	}
}
