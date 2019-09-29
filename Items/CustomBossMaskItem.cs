using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheLunatic.Logic;
using System.IO;
using Terraria.ID;


namespace TheLunatic.Items {
	[AutoloadEquip( EquipType.Head )]
	class CustomBossMaskItem : ModItem {
		public static Texture2D GetMaskTextureOfPlayer( Player player ) {
			if( Main.dedServ || Main.netMode == 2 ) { return null; }

			int itemType = TheLunaticMod.Instance.ItemType<CustomBossMaskItem>();
			ModItem myitem;
			CustomBossMaskItem mymaskitem = null;

			if( player.armor[0] != null && !player.armor[0].IsAir && player.armor[0].type == itemType ) {
				myitem = player.armor[0].modItem;
				if( myitem != null && myitem is CustomBossMaskItem ) {
					mymaskitem = (CustomBossMaskItem)player.armor[0].modItem;
				}
			} else if( player.armor[10] != null && !player.armor[10].IsAir && player.armor[10].type == itemType ) {
				myitem = player.armor[10].modItem;
				if( myitem != null && myitem is CustomBossMaskItem ) {
					mymaskitem = (CustomBossMaskItem)player.armor[10].modItem;
				}
			}

			return mymaskitem?.GetTexture();
		}



		////////////////

		public override bool CloneNewInstances => true;

		////////////////

		public int BossNpcType = 0;
		public int BossHeadIndex = -1;
		public string BossUid = "";
		public string BossDisplayName = "Unknown Boss";



		////////////////

		public override ModItem Clone() {
			var clone = (CustomBossMaskItem)base.Clone();
			clone.BossNpcType = this.BossNpcType;
			clone.BossHeadIndex = this.BossHeadIndex;
			clone.BossUid = this.BossUid;
			clone.BossDisplayName = this.BossDisplayName;
			return clone;
		}


		////////////////

		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Unknown Boss Mask" );
		}

		public override void SetDefaults() {
			this.item.width = 18;
			this.item.height = 18;
			this.item.value = 10000;
			this.item.rare = 3;
			this.item.vanity = true;
		}


		////

		private bool CustomLoad( int npcType, int bossHeadIndex, string bossNpcUid, string displayName ) {
			if( npcType == 0 || npcType == -1 ) {	// -1 for legacy support
				return false;
			}

			var npc = new NPC();
			npc.SetDefaults( npcType );

			if( NPCID.GetUniqueKey( npc ) != bossNpcUid ) {
				npcType = NPCID.TypeFromUniqueKey( bossNpcUid );
				if( npcType > 0 ) {
					npc.SetDefaults( npcType );
					return this.CustomLoad( npcType, npc.GetBossHeadTextureIndex(), bossNpcUid, npc.GivenName );
				}

				this.mod.Logger.Info( "Could not find boss head of custom boss mask for npc " + bossNpcUid );
				return false;
			}

			this.BossNpcType = npcType;
			this.BossHeadIndex = bossHeadIndex;
			this.BossUid = bossNpcUid;
			this.BossDisplayName = displayName;

			return true;
		}


		////////////////

		public override void Load( TagCompound tag ) {
			if( tag.ContainsKey( "boss_npc_type" ) ) {
				int npcType = tag.GetInt( "boss_npc_type" );
				int bossHeadIdx = tag.GetInt( "boss_head_index" );
				string bossUid = tag.GetString( "boss_uid" );
				string bossName = tag.GetString( "boss_display_name" );

				if( this.CustomLoad( npcType, bossHeadIdx, bossUid, bossName ) ) {
					string maskName = MaskLogic.GetMaskDisplayName( this.item );
					if( maskName != null ) {
						this.DisplayName.SetDefault( maskName );
					}
				}
			}
		}

		public override TagCompound Save() {
			var tag = new TagCompound {
				{ "boss_npc_type", (int)this.BossNpcType },
				{ "boss_head_index", (int)this.BossHeadIndex },
				{ "boss_uid", (string)this.BossUid },
				{ "boss_display_name", (string)this.BossDisplayName }
			};
			return tag;
		}


		////////////////

		public override void NetSend( BinaryWriter writer ) {
			writer.Write( (int)this.BossNpcType );
			writer.Write( (int)this.BossHeadIndex );
			writer.Write( (string)this.BossUid );
			writer.Write( (string)this.BossDisplayName );
		}

		public override void NetRecieve( BinaryReader reader ) {
			this.BossNpcType = reader.ReadInt32();
			this.BossHeadIndex = reader.ReadInt32();
			this.BossUid = reader.ReadString();
			this.BossDisplayName = reader.ReadString();
		}


		////////////////

		public override bool PreDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale ) {
			this.DrawItem( sb, position, Color.White, origin, scale );
			return false;
		}

		public override bool PreDrawInWorld( SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI ) {
			this.DrawItem( sb, this.item.position - Main.screenPosition, Color.White, new Vector2(), scale );
			return false;
		}

		////////////////

		private void DrawItem( SpriteBatch sb, Vector2 pos, Color color, Vector2 origin, float scale ) {
			var tex = this.GetTexture();
			if( tex != null ) {
				sb.Draw( tex, pos, null, color, 0f, origin, scale, SpriteEffects.None, 0f );
			}
		}


		////////////////

		public Texture2D GetTexture() {
			if( this.BossHeadIndex <= -1 || this.BossHeadIndex >= Main.npcHeadBossTexture.Length ) {
				return null;
			}
			return Main.npcHeadBossTexture[ this.BossHeadIndex ];
		}


		////////////////

		public bool SetBoss( int npcType ) {
			NPC npc = new NPC();
			npc.SetDefaults( npcType );

			int idx = npc.GetBossHeadTextureIndex();
			if( idx < 0 || idx >= Main.npcHeadBossTexture.Length || Main.npcHeadBossTexture[idx] == null ) {
				return false;
			}

			if( this.CustomLoad( npcType, idx, NPCID.GetUniqueKey(npc), npc.GivenName ) ) {
				this.item.SetNameOverride( npc.GivenName + " Mask" );
			}


			/*if( Main.netMode != 2 ) {
				var tex = Main.npcHeadBossTexture[info.BossHeadIndex];
				//var old_tex = Main.armorHeadTexture[this.item.headSlot];
				if( tex == null ) { return; }
				//if( old_tex == null ) { return; }

				Main.itemTexture[this.item.type] = tex;

				try {
					var graDev = Main.graphics.GraphicsDevice;
					var newTex = new RenderTarget2D( graDev, oldTex.Width, oldTex.Height, false, graDev.PresentationParameters.BackBufferFormat, DepthFormat.Depth24 );
					var old_ren_tar = graDev.GetRenderTargets();

					graDev.SetRenderTarget( newTex );
					graDev.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
					
					graDev.Clear( Color.Transparent );
					var sb = new SpriteBatch( graDev );
					sb.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend );
					for( int i = 0; i <= 20; i++ ) {
						var rect = new Rectangle( 0, 4 + (i * oldTex.Height), oldTex.Width, oldTex.Height / 20 );
						sb.Draw( tex, rect, Color.White );
					}
					sb.End();
					
					graDev.SetRenderTargets( old_ren_tar );

					Main.armorHeadTexture[this.item.headSlot] = newTex;
				} catch( Exception e ) {
					ErrorLogger.Log( e.ToString() );
				}
			}*/

			return true;
		}
	}
}