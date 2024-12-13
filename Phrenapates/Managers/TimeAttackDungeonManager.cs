using Plana.MX.GameLogic.DBModel;
using Plana.FlatData;
using Plana.Database;
using Phrenapates.Utils;
using Phrenapates.Services;
using Plana.MX.Logic.Battles.Summary;
using Microsoft.IdentityModel.Tokens;

namespace Phrenapates.Managers
{
    public class TimeAttackDungeonManager : Singleton<TimeAttackDungeonManager>
    {
        public Dictionary<long, TimeAttackDungeonRoomDB> TimeAttackDungeonRooms { get; private set; }
        public List<TimeAttackDungeonBattleHistoryDB> TimeAttackDungeonBattleHistoryDBs { get; private set; } = [];
        public long SeasonId { get; private set; }
        public DateTime OverrideServerTimeTicks { get; private set; }
        
        public DateTime CreateServerTime(TimeAttackDungeonSeasonManageExcelT targetSeason, ContentInfo contentInfo)
        {
            if (OverrideServerTimeTicks == null || SeasonId != contentInfo.TimeAttackDungeonDataInfo.SeasonId)
            {
                OverrideServerTimeTicks = DateTime.Parse(targetSeason.StartDate);
            }
            return OverrideServerTimeTicks;
        }

        public DateTime GetServerTime() => OverrideServerTimeTicks;
        public Dictionary<long, TimeAttackDungeonRoomDB> GetLobby() => TimeAttackDungeonRooms;
        public TimeAttackDungeonRoomDB GetRoom() => TimeAttackDungeonRooms[1];

        public TimeAttackDungeonRoomDB GetPreviousRoom()
        {
            if (TimeAttackDungeonBattleHistoryDBs.Count == 3) return TimeAttackDungeonRooms[1];
            else return null;
        }

        public TimeAttackDungeonRoomDB CreateBattle(long ownerId, bool IsPractice, ContentInfo contentInfo)
        {
            if(TimeAttackDungeonRooms == null || TimeAttackDungeonRooms[1].SeasonId != contentInfo.TimeAttackDungeonDataInfo.SeasonId)
            {
                TimeAttackDungeonRooms = new Dictionary<long, TimeAttackDungeonRoomDB>
                {
                    { 
                        1,
                        new TimeAttackDungeonRoomDB
                        { 
                            RoomId = 1,
                            AccountId = ownerId,
                            SeasonId = contentInfo.TimeAttackDungeonDataInfo.SeasonId,
                            CreateDate = OverrideServerTimeTicks,
                            IsPractice = IsPractice
                        }
                    }
                };
            }

            return TimeAttackDungeonRooms[1];
        }

        public TimeAttackDungeonRoomDB BattleResult(BattleSummary battleSummary, TimeAttackDungeonGeasExcelT TADgeasData)
        {
            if(TimeAttackDungeonBattleHistoryDBs.IsNullOrEmpty())
            {
                TimeAttackDungeonBattleHistoryDBs = [
                    new TimeAttackDungeonBattleHistoryDB()
                    {
                        DungeonType = TADgeasData.TimeAttackDungeonType,
                        GeasId = TADgeasData.Id,
                        DefaultPoint = TADgeasData.ClearDefaultPoint,
                        ClearTimePoint = TimeAttackDungeonService.CalculateTimeScore(battleSummary.EndFrame/30f, TADgeasData),
                        EndFrame = battleSummary.EndFrame,
                        MainCharacterDBs = TimeAttackDungeonService.ConvertHeroSummaryToCollection(battleSummary.Group01Summary.Heroes),
                        SupportCharacterDBs = TimeAttackDungeonService.ConvertHeroSummaryToCollection(battleSummary.Group01Summary.Supporters)
                    }
                ];
            }

            else
            {
                TimeAttackDungeonBattleHistoryDBs.Add(
                    new TimeAttackDungeonBattleHistoryDB()
                    {
                        DungeonType = TADgeasData.TimeAttackDungeonType,
                        GeasId = TADgeasData.Id,
                        DefaultPoint = TADgeasData.ClearDefaultPoint,
                        ClearTimePoint = TimeAttackDungeonService.CalculateTimeScore(battleSummary.EndFrame/30f, TADgeasData),
                        EndFrame = battleSummary.EndFrame,
                        MainCharacterDBs = TimeAttackDungeonService.ConvertHeroSummaryToCollection(battleSummary.Group01Summary.Heroes),
                        SupportCharacterDBs = TimeAttackDungeonService.ConvertHeroSummaryToCollection(battleSummary.Group01Summary.Supporters)
                    }
                );
            }

            TimeAttackDungeonRooms[1].BattleHistoryDBs = TimeAttackDungeonBattleHistoryDBs;
            if (TimeAttackDungeonBattleHistoryDBs.Count == 3)
            {
                TimeAttackDungeonBattleHistoryDBs = new List<TimeAttackDungeonBattleHistoryDB>();
                TimeAttackDungeonRooms[1].RewardDate = DateTime.Now;
            }

            return TimeAttackDungeonRooms[1];
        }

        public TimeAttackDungeonRoomDB GiveUp()
        {
            var tempData = TimeAttackDungeonRooms[1];
            tempData.RewardDate = DateTime.Now;
            TimeAttackDungeonRooms = null;
            TimeAttackDungeonBattleHistoryDBs = new List<TimeAttackDungeonBattleHistoryDB>();

            return tempData;
        }
    }
}
