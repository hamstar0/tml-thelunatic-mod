using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;


namespace PlayerExtend {
	public class PlayerNoclip {
		public bool IsOn { get; private set; }

		private float Speed = 1f;

		private Vector2? LastPos;
		private Vector2? LastPos2;
		private Vector2 CurrentPos;

		private bool IsColliding = false;


		////////////////

		public PlayerNoclip() { }

		public void On( Player player, float speed ) {
			if( this.IsOn ) { return; }

			this.CurrentPos = player.position;
			this.LastPos = null;
			this.LastPos2 = null;
			this.IsOn = true;
			this.Speed = speed;
		}

		public void Off() {
			this.IsOn = false;
		}

		////////////////

		public void UpdateMode( Player player ) {
			if( !this.IsOn ) { return; }

			if( player.FindBuffIndex(68) != -1 ) {
				player.ClearBuff( 68 );
			}

			player.noItems = true;
			player.immune = true;
			player.gravity = 0;
			player.velocity.X = 0;
			player.velocity.Y = 0;
			player.controlJump = false;
			player.controlUseItem = false;
			player.controlUseTile = false;
			player.controlThrow = false;
			player.grappling[0] = -1;
			player.grapCount = 0;

			//player.ResetEffects();
			//player.head = -1;
			//player.body = -1;
			//player.legs = -1;
			//player.handon = -1;
			//player.handoff = -1;
			//player.back = -1;
			//player.front = -1;
			//player.shoe = -1;
			//player.waist = -1;
			//player.shield = -1;
			//player.neck = -1;
			//player.face = -1;
			//player.balloon = -1;
		}


		public void UpdateMovement( Player player ) {
			if( !this.IsOn ) { return; }

			// Failsafe against large distance jumps with the player
			if( Vector2.Distance( player.position, this.CurrentPos) >= 128f ) {
				this.LastPos = null;
				//this.LastPos2 = null;
				this.CurrentPos = player.position;
			}
			
			if( !this.IsColliding ) {
				this.LastPos2 = this.LastPos;
				this.LastPos = this.CurrentPos;

				if( player.controlRight ) {
					player.controlRight = false;
					this.CurrentPos.X += this.Speed;
				}
				if( player.controlLeft ) {
					player.controlLeft = false;
					this.CurrentPos.X -= this.Speed;
				}
				if( player.controlUp ) {
					player.controlUp = false;
					this.CurrentPos.Y -= this.Speed;
				}
				if( player.controlDown ) {
					player.controlDown = false;
					this.CurrentPos.Y += this.Speed;
				}
			}

			player.position = this.CurrentPos;
			this.IsColliding = false;

			if( Main.netMode == 1 ) {
				if( Main.time % 60 == 0 ) {
					NetMessage.SendData( MessageID.Teleport, -1, -1, null, 0, (float)player.whoAmI, this.CurrentPos.X, this.CurrentPos.Y, -1, 0, 0 );
				}
			}
		}


		public bool Collide() {
			if( !this.IsOn ) { return false; }

			bool stuck = false;
			this.IsColliding = true;

			if( this.LastPos != null ) {
				this.CurrentPos = (Vector2)this.LastPos;
				this.LastPos = null;
			} else if( this.LastPos2 != null ) {
				this.CurrentPos = (Vector2)this.LastPos2;
				this.LastPos2 = null;
			} else {
				stuck = true;
			}

			return stuck;
		}
	}
}
