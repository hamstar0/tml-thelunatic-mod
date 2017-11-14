using Microsoft.Xna.Framework.Audio;
using Terraria.ModLoader;


namespace TheLunatic.Sounds.Custom {
	public class EarthQuake : ModSound {
		public override SoundEffectInstance PlaySound( ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type ) {
			soundInstance.Volume = volume; //* 0.55f;
			soundInstance.Pan = pan;
			return soundInstance;
		}
	}
}
