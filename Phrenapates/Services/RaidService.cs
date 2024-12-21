using Plana.FlatData;
using Plana.MX.Logic.Battles.Summary;
using Plana.MX.Logic.Data;

namespace Phrenapates.Services
{
    public class RaidService
    {
        public static long CalculateTimeScore(float duration, Difficulty difficulty)
        {
            int[] multipliers = [120, 240, 480, 960, 1440, 1920, 2400];
            
            return (long)((3600f - duration) * multipliers[(int)difficulty]);
        }
        
        public static long CalculateGroggyAccumulation(long groggyPointTotal, CharacterStatExcelT characterStat)
        {
            long groggyPoint = groggyPointTotal;
            if(characterStat.GroggyGauge != 0) groggyPoint = groggyPointTotal % characterStat.GroggyGauge;

            return groggyPoint;
        }

        public static List<long> CharacterParticipation(GroupSummary Group01Summary)
        {
            return Group01Summary.Heroes.Select(x => x.ServerId).ToList()
            .Concat(Group01Summary.Supporters.Select(x => x.ServerId)).ToList();
        }

        public static RaidDamage CreateRaidCollection(RaidDamage raidDmg)
        {
            return new RaidDamage()
            {
                Index = raidDmg.Index,
                GivenDamage = raidDmg.GivenDamage,
                GivenGroggyPoint = raidDmg.GivenGroggyPoint
            };
        }

        public static long AIPhaseChecks(
            int bossIndex, long bossHp, long previousPhase,
            EliminateRaidStageExcelT raidStageExcel, List<CharacterStatExcelT> characterStatExcels    
        )
        {
            var characterStat = characterStatExcels.FirstOrDefault(x => x.CharacterId == raidStageExcel.BossCharacterId[bossIndex]);
            if(raidStageExcel.GroundDevName == "Binah")
            {
                // Skips for now
                //*if(bossHp <= characterStat.MaxHP100 * )
                return 0;
            }
            else if(raidStageExcel.GroundDevName == "Chesed")
            {
                // HP Threshold Checks
                if(bossHp < characterStat.MaxHP100) return 1;
                else return 0;
            }
            else if(raidStageExcel.GroundDevName == "ShiroKuro")
            {
                // Boss Index Checks
                if(bossIndex == 1) return 7;
                else return 0;
            }
            else if(raidStageExcel.GroundDevName == "Hieronymus")
            {
                // HP Threshold Checks
                if(bossHp <= (characterStat.MaxHP100 * 0.5)) return 1;
                else return 0;
            }
            else if(raidStageExcel.GroundDevName == "Kaitenger")
            {
                // Boss Index Checks
                if(bossIndex == 1) return 1;
                else return 0;
            }
            // Perorozilla has no checks
            else if(raidStageExcel.GroundDevName == "HOD") return previousPhase; //Continues from previous phase
            else if(raidStageExcel.GroundDevName == "Goz")
            {
                // HP Threshold Checks (Weird phase default)
                if(bossHp <= (characterStat.MaxHP100 * 0.6)) return 2;
                else return 1;
            }
            // Gregorius (EN0005) has no checks
            else if(raidStageExcel.GroundDevName == "HoverCraft")
            {
                // Boss Index Checks
                if(bossIndex == 1) return 4;
                else return 0;
            }
            else if(raidStageExcel.GroundDevName == "EN0006") // Kurokage
            {
                // HP Threshold Checks
                if(bossHp <= (characterStat.MaxHP100 * 0.1)) return 2;
                else if(bossHp <= (characterStat.MaxHP100 * 0.75)) return 1;
                else return 0;
            }
            else return 0;
        }
    }

}
