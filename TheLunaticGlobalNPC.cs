﻿using System;
using Terraria;
using Terraria.ModLoader;
using TheLunatic.Items;
using TheLunatic.Logic;
using Utils;


namespace TheLunatic {
	public class TheLunaticGlobalNPC : GlobalNPC {
		public override void AI( NPC npc ) {
			if( Main.rand == null ) { return; }

			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld == null ) { return; }

			// Kill town NPCs above ground every minute when set to do so
			if( modworld.GameLogic.KillSurfaceTownNPCs ) {
				if( npc.townNPC && npc.position.Y < (Main.worldSurface*16.0) && Main.rand.Next(60*60) == 0 ) {
					NPCHelper.Kill( npc );
					return;
				}
			}
		}


		public override void SetDefaults( NPC npc ) {
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld == null ) { return; }
			if( modworld.GameLogic == null ) { return; }

			// Prevent new town NPCs when set to do so
			if( modworld.GameLogic.KillSurfaceTownNPCs ) {
				if( npc.townNPC && WorldGen.spawnNPC == npc.type ) {
					npc.active = false;
					npc.type = 0;
					npc.life = 0;
					WorldGen.spawnNPC = 0;
				}
			}
		}


		public override void NPCLoot( NPC npc ) {
			if( !npc.boss && npc.type != 551 && npc.type != 398 ) { return; }	// Betsy isn't a boss?
			var modworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( modworld.GameLogic == null ) { throw new Exception( "Game logic not initialized." ); }
			if( modworld.MaskLogic == null ) { throw new Exception( "Mask logic not initialized." ); }
			if( !modworld.GameLogic.HaveWeHopeToWin() ) { return; }

			Item item = null;
			int mask_type = MaskLogic.GetMaskTypeOfNpc( npc.type );
			if( mask_type == -1 ) { return; }

			// Already given this mask?
			bool is_vanilla = MaskLogic.AllVanillaMasks.Contains( mask_type );
			if( !modworld.MaskLogic.GivenVanillaMasksByType.Contains( mask_type ) ) {
				if( !is_vanilla && modworld.MaskLogic.GivenCustomMasksByBossUid.Contains( NPCHelper.GetUniqueId( npc ) ) ) {
					return;
				}
			}

			// No modded masks allowed?
			var mymod = (TheLunaticMod)this.mod;
			if( !is_vanilla && mymod.Config.Data.OnlyVanillaBossesDropMasks ) {
				return;
			}
			
			int which = ItemHelper.CreateItem( npc.position, mask_type, 1, 15, 15 );
			item = Main.item[which];

			if( item != null && !item.IsAir ) {
				if( mask_type == this.mod.ItemType<CustomBossMaskItem>() ) {
					var moditem = (CustomBossMaskItem)item.modItem;
					if( !moditem.SetBoss(npc.type) ) {
						ItemHelper.DestroyWorldItem( which );
					}
				} else {
					/*if( !modworld.MaskLogic.GivenVanillaMasks.Contains( mask_type ) ) {
						// Does this mask already exist in the world?
						for( int i = 0; i < Main.item.Length; i++ ) {
							if( Main.item[i] != null && Main.item[i].active && Main.item[i].type == mask_type ) {
								item = Main.item[i];
								break;
							}
						}
					}*/
				}
			} else {
				ErrorLogger.Log( "Could not spawn a mask of type " + mask_type );
			}
		}


		/*private int HurtAnimateLength = 15;
		private IDictionary<int, int> HurtAnimate = new Dictionary<int, int>();

		public override void DrawEffects( NPC npc, ref Color color ) {
			if( !this.HurtAnimate.Keys.Contains(npc.whoAmI) || this.HurtAnimate[npc.whoAmI] == 0 ) { return; }

			float scale = (float)this.HurtAnimate[npc.whoAmI] / (float)this.HurtAnimateLength;
			scale /= 1.4f;

			if( color.R < 255 ) {
				color.R += (byte)((float)(255 - color.R) * scale);
			}
			color.G -= (byte)(color.G * scale);
			color.B -= (byte)(color.B * scale);

			this.HurtAnimate[npc.whoAmI]--;
		}

		public override bool StrikeNPC( NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit ) {
			this.HurtAnimate[npc.whoAmI] = this.HurtAnimateLength;
			return base.StrikeNPC( npc, ref damage, defense, ref knockback, hitDirection, ref crit );
		}*/
	}
}
