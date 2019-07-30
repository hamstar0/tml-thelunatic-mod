using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.Debug;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheLunatic.NPCs {
	[AutoloadHead]
	partial class TheLunaticTownNPC : ModNPC {
		public static string[] PossibleNames { get; private set; }
		public static string[] DismissalReplies { get; private set; }
		public static string[] NormalReplies { get; private set; }

		public static bool AlertedToImpendingDoom { get; private set; }
		public static bool AmHere { get; private set; }

		////

		static TheLunaticTownNPC() {
			TheLunaticTownNPC.AlertedToImpendingDoom = false;
			TheLunaticTownNPC.AmHere = false;

			TheLunaticTownNPC.InitializeReplies();

			TheLunaticTownNPC.PossibleNames = new string[] {
				"Whateley",
				"Dunwich",
				"Waite",
				"Olmstead",
				"Marsh",
				"Jenkin",
				"Jermyn",
				"Mason",
				"Alhazred",
				"Castro"
			};
		}



		////////////////

		public override string Texture => "TheLunatic/NPCs/TheLunaticTownNPC";

		public override bool Autoload( ref string name ) {
			name = "The Lunatic";
			return this.mod.Properties.Autoload;
		}

		public override void SetStaticDefaults() {
			int npcType = this.npc.type;

			this.DisplayName.SetDefault( "The Lunatic" );

			Main.npcFrameCount[npcType] = 26;
			NPCID.Sets.AttackFrameCount[npcType] = 5;
			NPCID.Sets.DangerDetectRange[npcType] = 700;
			NPCID.Sets.AttackType[npcType] = 1;
			NPCID.Sets.AttackTime[npcType] = 30;
			NPCID.Sets.AttackAverageChance[npcType] = 30;
			NPCID.Sets.HatOffsetY[npcType] = 4;
		}

		public override void SetDefaults() {
			int npcType = this.npc.type;
			
			this.npc.townNPC = true;
			this.npc.friendly = true;
			this.npc.width = 18;
			this.npc.height = 40;
			this.npc.aiStyle = 7;
			this.npc.damage = 10;
			this.npc.defense = 15;
			this.npc.lifeMax = 250;
			this.npc.HitSound = SoundID.NPCHit1;
			this.npc.DeathSound = SoundID.NPCDeath1;
			this.npc.knockBackResist = 0.5f;
			this.animationType = NPCID.Guide;
		}
		
		public override void TownNPCAttackStrength( ref int damage, ref float knockback ) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown( ref int cooldown, ref int randExtraCooldown ) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj( ref int projType, ref int attackDelay ) {
			projType = 1;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed( ref float multiplier, ref float gravityCorrection, ref float randomOffset ) {
			multiplier = 12f;
			randomOffset = 2f;
		}

		public override string TownNPCName() {
			return TheLunaticTownNPC.PossibleNames[ Main.rand.Next(TheLunaticTownNPC.PossibleNames.Length) ];
			//return "The Lunatic";
		}

		public override bool UsesPartyHat() {
			return false;   // :(
		}
		
		public override void AI() {
			var myworld = this.mod.GetModWorld<TheLunaticWorld>();
			if( myworld.GameLogic == null ) { throw new ModHelpersException( "Game logic not initialized." ); }

			TheLunaticTownNPC.AmHere = true;

			if( myworld.GameLogic.HaveWeEndSigns() ) {
				TheLunaticTownNPC.AlertedToImpendingDoom = true;
			}
		}
	}
}