using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.Logic.Battles.Summary;

namespace Phrenapates.Services
{
    public class TimeAttackDungeonService
    {
        public static long CalculateTimeScore(float duration, TimeAttackDungeonGeasExcelT taDungeonData)
        {
            long totalBattleDuration = taDungeonData.BattleDuration / 1000;
            long timeWeightConst = taDungeonData.TimeWeightConst / 10000;
            
            // Formula: ROUNDDOWN(ClearTimeWeightPoint * (1 - PlayerClearTime / ((BattleDuration / 1000) + ((TimeWeightConst / 10000) * PlayerClearTime))))
            return (long)Math.Floor(taDungeonData.ClearTimeWeightPoint * 1 - (duration / (totalBattleDuration + (timeWeightConst * duration))));
        }

        public static List<TimeAttackDungeonCharacterDB> ConvertHeroSummaryToCollection(HeroSummaryCollection heroSummary)
        {
            return heroSummary.Select(x => {
                return new TimeAttackDungeonCharacterDB()
                {
                    ServerId = x.ServerId,
                    UniqueId = x.CharacterId,
                    StarGrade = x.Grade,
                    Level = x.Level,
                    HasWeapon = x.CharacterWeapon != null,
                    WeaponDB = x.CharacterWeapon == null ? null : new WeaponDB()
                    {
                        UniqueId = x.CharacterWeapon.Value.UniqueId,
                        Level = x.CharacterWeapon.Value.Level
                    }
                };
            }).ToList();
        }

        public static long CalculateScoreRecord(TimeAttackDungeonRoomDB rooms, List<TimeAttackDungeonGeasExcelT> TADGeasExcel)
        {
            long totalPoint = 0;
            foreach (var room in rooms.BattleHistoryDBs)
            {
                var geasData = TADGeasExcel.FirstOrDefault(x => x.Id == room.GeasId);
                var timePoint = CalculateTimeScore(room.EndFrame/30, geasData);
                totalPoint += geasData.ClearDefaultPoint + timePoint;
            }

            return totalPoint;
        }

        public static TimeAttackDungeonBattleHistoryDB DummyBattleHistory(TimeAttackDungeonGeasExcelT TADgeasData)
        {
            TimeAttackDungeonBattleHistoryDB dummyBattleHistory = new()
            {
                DungeonType = TADgeasData.TimeAttackDungeonType,
                GeasId = TADgeasData.Id,
                DefaultPoint = TADgeasData.ClearDefaultPoint,
                ClearTimePoint = CalculateTimeScore(1750/30f, TADgeasData),
                EndFrame = 1750,
                MainCharacterDBs = [
                    new TimeAttackDungeonCharacterDB()
                    {
                        ServerId = 1,
                        UniqueId = 10000,
                        StarGrade = 4,
                        Level = 90,
                    }
                ],
                SupportCharacterDBs = [
                    new TimeAttackDungeonCharacterDB()
                    {
                        ServerId = 2,
                        UniqueId = 26000,
                        StarGrade = 4,
                        Level = 90,
                    }
                ]
                
            };


            return dummyBattleHistory;
        }
    }
}