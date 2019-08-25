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
			int itemType = TheLunaticMod.Instance.ItemType<CustomBossMaskItem>();
			CustomBossMaskItem moditem = null;

			if( player.armor[0] != null && !player.armor[0].IsAir && player.armor[0].type == itemType ) {
				moditem = (CustomBossMaskItem)player.armor[0].modItem;
			} else if( player.armor[10] != null && !player.armor[10].IsAir && player.armor[10].type == itemType ) {
				moditem = (CustomBossMaskItem)player.armor[10].modItem;
			}
			if( moditem == null ) { return null; }

			return moditem.GetTexture();
		}


		
		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Boss Mask" );
		}

		public override void SetDefaults() {
			this.item.width = 18;
			this.item.height = 18;
			this.item.value = 10000;
			this.item.rare = 3;
			this.item.vanity = true;
		}

		////////////////

		public override void Load( TagCompound tag ) {
			var itemInfo = this.item.GetGlobalItem<CustomBossMaskItemInfo>( this.mod );
			int npcType = tag.GetInt( "boss_npc_type" );
			int bossHeadIdx = tag.GetInt( "boss_head_index" );
			string bossUid = tag.GetString( "boss_uid" );
			string bossName = tag.GetString( "boss_display_name" );

			itemInfo.Load( npcType, bossHeadIdx, bossUid, bossName );
			
			string name = MaskLogic.GetMaskDisplayName( this.item );
			if( name != null ) {
				this.DisplayName.SetDefault( name );
			}
		}

		public override TagCompound Save() {
			var itemInfo = this.item.GetGlobalItem<CustomBossMaskItemInfo>( this.mod );
			var tag = new TagCompound {
				{ "boss_npc_type", itemInfo.BossNpcType },
				{ "boss_head_index", itemInfo.BossHeadIndex },
				{ "boss_uid", itemInfo.BossUid },
				{ "boss_display_name", itemInfo.BossDisplayName }
			};
			return tag;
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
				Main.spriteBatch.Draw( tex, pos, null, color, 0f, origin, scale, SpriteEffects.None, 0f );
			}
		}

		////////////////

		public Texture2D GetTexture() {
			var itemInfo = this.item.GetGlobalItem<CustomBossMaskItemInfo>( this.mod );
			if( itemInfo.BossHeadIndex == -1 || itemInfo.BossHeadIndex >= Main.npcHeadBossTexture.Length ) {
				return null;
			}
			return Main.npcHeadBossTexture[itemInfo.BossHeadIndex];
		}


		public bool SetBoss( int npcType ) {
			var itemInfo = this.item.GetGlobalItem<CustomBossMaskItemInfo>( this.mod );
			NPC npc = new NPC();
			npc.SetDefaults( npcType );
			int idx = npc.GetBossHeadTextureIndex();
			if( idx == -1 || idx >= Main.npcHeadBossTexture.Length || Main.npcHeadBossTexture[idx] == null ) { return false; }

			itemInfo.Load( npcType, idx, NPCID.GetUniqueKey(npc), npc.GivenName );
			this.item.SetNameOverride( npc.GivenName + " Mask" );
			

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


	
	class CustomBossMaskItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }
		//public override bool CloneNewInstances { get { return true; } }

		public int BossNpcType = -1;
		public int BossHeadIndex = -1;
		public string BossUid = "";
		public string BossDisplayName = "";

		
		public override GlobalItem Clone( Item item, Item itemClone ) {
			var clone = (CustomBossMaskItemInfo)base.Clone( item, itemClone );
			clone.BossNpcType = this.BossNpcType;
			clone.BossHeadIndex = this.BossHeadIndex;
			clone.BossUid = this.BossUid;
			clone.BossDisplayName = this.BossDisplayName;
			return clone;
		}

		public void Load( int npcType, int bossHeadIndex, string uid, string displayName ) {
			var npc = new NPC();
			npc.SetDefaults( npcType );

			if( NPCID.GetUniqueKey(npc) != uid ) {
				npcType = NPCID.TypeFromUniqueKey( uid );
				if( npcType != -1 ) {
					npc.SetDefaults( npcType );
					this.Load( npcType, npc.GetBossHeadTextureIndex(), uid, npc.GivenName );
				} else {
					this.mod.Logger.Info( "Could not find boss head of custom boss mask for npc " + uid );
				}
				return;
			}

			this.BossNpcType = npcType;
			this.BossHeadIndex = bossHeadIndex;
			this.BossUid = uid;
			this.BossDisplayName = displayName;
		}

		
		public override void NetSend( Item item, BinaryWriter writer ) {
			writer.Write( (int)this.BossNpcType );
			writer.Write( (int)this.BossHeadIndex );
			writer.Write( (string)this.BossUid );
			writer.Write( (string)this.BossDisplayName );
		}

		public override void NetReceive( Item item, BinaryReader reader ) {
			this.BossNpcType = reader.ReadInt32();
			this.BossHeadIndex = reader.ReadInt32();
			this.BossUid = reader.ReadString();
			this.BossDisplayName = reader.ReadString();
		}
	}
}