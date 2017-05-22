using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Utils;
using TheLunatic.Logic;

namespace TheLunatic.Items {
	public class CustomBossMaskItem : ModItem {
		public static Texture2D GetMaskTextureOfPlayer( Player player, Mod mod ) {
			int item_type = mod.ItemType<CustomBossMaskItem>();
			CustomBossMaskItem moditem = null;

			if( player.armor[0] != null && !player.armor[0].IsAir && player.armor[0].type == item_type ) {
				moditem = (CustomBossMaskItem)player.armor[0].modItem;
			} else if( player.armor[10] != null && !player.armor[10].IsAir && player.armor[10].type == item_type ) {
				moditem = (CustomBossMaskItem)player.armor[10].modItem;
			}
			if( moditem == null ) { return null; }

			return moditem.GetTexture();
		}



		public override bool Autoload( ref string name, ref string texture, IList<EquipType> equips ) {
			equips.Add( EquipType.Head );
			return true;
		}

		public override void SetDefaults() {
			this.item.name = "Boss Mask";
			this.item.width = 18;
			this.item.height = 18;
			this.item.value = 10000;
			this.item.rare = 3;
			this.item.vanity = true;
		}

		////////////////

		public override void Load( TagCompound tag ) {
			var info = this.item.GetModInfo<CustomBossMaskItemInfo>( this.mod );
			int npc_type = tag.GetInt( "boss_npc_type" );
			int boss_head_idx = tag.GetInt( "boss_head_index" );
			string boss_uid = tag.GetString( "boss_uid" );
			string boss_name = tag.GetString( "boss_display_name" );

			info.Load( npc_type, boss_head_idx, boss_uid, boss_name );
			
			string name = MaskLogic.GetMaskDisplayName( this.item );
			if( name != null ) {
				this.item.name = name;
			}
		}

		public override TagCompound Save() {
			var info = this.item.GetModInfo<CustomBossMaskItemInfo>( this.mod );
			var tag = new TagCompound {
				{ "boss_npc_type", info.BossNpcType },
				{ "boss_head_index", info.BossHeadIndex },
				{ "boss_uid", info.BossUid },
				{ "boss_display_name", info.BossDisplayName }
			};
			return tag;
		}

		////////////////
		
		public override bool PreDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
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
			var item_info = this.item.GetModInfo<CustomBossMaskItemInfo>( this.mod );
			if( item_info.BossHeadIndex == -1 || item_info.BossHeadIndex >= Main.npcHeadBossTexture.Length ) {
				return null;
			}
			return Main.npcHeadBossTexture[item_info.BossHeadIndex];
		}


		public bool SetBoss( int npc_type ) {
			var info = this.item.GetModInfo<CustomBossMaskItemInfo>( this.mod );
			NPC npc = new NPC();
			npc.SetDefaults( npc_type );
			int idx = npc.GetBossHeadTextureIndex();
			if( idx == -1 || idx >= Main.npcHeadBossTexture.Length || Main.npcHeadBossTexture[idx] == null ) { return false; }

			info.Load( npc_type, idx, NPCHelper.GetUniqueId(npc), npc.displayName );
			this.item.name = npc.displayName + " Mask";

			/*if( Main.netMode != 2 ) {
				var tex = Main.npcHeadBossTexture[info.BossHeadIndex];
				//var old_tex = Main.armorHeadTexture[this.item.headSlot];
				if( tex == null ) { return; }
				//if( old_tex == null ) { return; }

				Main.itemTexture[this.item.type] = tex;

				try {
					var gra_dev = Main.graphics.GraphicsDevice;
					var new_tex = new RenderTarget2D( gra_dev, old_tex.Width, old_tex.Height, false, gra_dev.PresentationParameters.BackBufferFormat, DepthFormat.Depth24 );
					var old_ren_tar = gra_dev.GetRenderTargets();

					gra_dev.SetRenderTarget( new_tex );
					gra_dev.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
					
					gra_dev.Clear( Color.Transparent );
					var sb = new SpriteBatch( gra_dev );
					sb.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend );
					for( int i = 0; i <= 20; i++ ) {
						var rect = new Rectangle( 0, 4 + (i * old_tex.Height), old_tex.Width, old_tex.Height / 20 );
						sb.Draw( tex, rect, Color.White );
					}
					sb.End();
					
					gra_dev.SetRenderTargets( old_ren_tar );

					Main.armorHeadTexture[this.item.headSlot] = new_tex;
				} catch( Exception e ) {
					ErrorLogger.Log( e.ToString() );
				}
			}*/

			return true;
		}
	}


	
	class CustomBossMaskItemInfo : ItemInfo {
		public int BossNpcType = -1;
		public int BossHeadIndex = -1;
		public string BossUid;
		public string BossDisplayName;


		public void Load( int npc_type, int boss_head_index, string uid, string display_name ) {
			var npc = new NPC();
			npc.SetDefaults( npc_type );
			if( NPCHelper.GetUniqueId(npc) != uid ) {
				npc_type = NPCHelper.FindNpcTypeByUniqueId( uid );
				if( npc_type != -1 ) {
					npc.SetDefaults( npc_type );
					this.Load( npc_type, npc.GetBossHeadTextureIndex(), uid, npc.displayName );
				} else {
					ErrorLogger.Log( "Could not find boss head of custom boss mask for npc " + uid );
				}
				return;
			}

			this.BossNpcType = npc_type;
			this.BossHeadIndex = boss_head_index;
			this.BossUid = uid;
			this.BossDisplayName = display_name;
		}

		public override ItemInfo Clone() {
			var clone = (CustomBossMaskItemInfo)base.Clone();
			clone.BossNpcType = this.BossNpcType;
			clone.BossHeadIndex = this.BossHeadIndex;
			clone.BossUid = this.BossUid;
			clone.BossDisplayName = this.BossDisplayName;
			return clone;
		}
	}
}