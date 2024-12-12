using Plana.MX.NetworkProtocol;
using Phrenapates.Services;
using Plana.Database;
using Plana.FlatData;
using Phrenapates.Managers;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.Logic.Battles;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class TimeAttackDungeon : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public TimeAttackDungeon(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_Login)]
        public ResponsePacket TimeAttackDungeonLoginHandler(TimeAttackDungeonLoginRequest req)
        {
            return new TimeAttackDungeonLoginResponse();
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_Lobby)]
        public ResponsePacket TimeAttackDungeonLobbyHandler(TimeAttackDungeonLobbyRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var TADSeasonExcel = excelTableService.GetTable<TimeAttackDungeonSeasonManageExcelTable>().UnPack().DataList;

            var currentRoom = TimeAttackDungeonManager.Instance.GetLobby();
            var previousRoom = TimeAttackDungeonManager.Instance.GetPreviousRoom();
            var packet = new TimeAttackDungeonLobbyResponse
            {
                ServerTimeTicks = TimeAttackDungeonManager.Instance.CreateServerTime(TADSeasonExcel, account.ContentInfo).Ticks
            };

            if (currentRoom != null) packet.RoomDBs = currentRoom;
            if (previousRoom != null) packet.PreviousRoomDB = previousRoom;


            return packet;
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_CreateBattle)]
        public ResponsePacket TimeAttackDungeonCreateBattleHandler(TimeAttackDungeonCreateBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new TimeAttackDungeonCreateBattleResponse()
            {
                RoomDB = TimeAttackDungeonManager.Instance.CreateBattle(account.ServerId, req.IsPractice, account.ContentInfo),
                ServerTimeTicks = TimeAttackDungeonManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_EnterBattle)] // clicked restart
        public ResponsePacket TimeAttackDungeonEnterBattleHandler(TimeAttackDungeonEnterBattleRequest req)
        {
            return new TimeAttackDungeonEnterBattleResponse()
            {
                AssistCharacterDB = new AssistCharacterDB(),
                ServerTimeTicks = TimeAttackDungeonManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_EndBattle)]
        public ResponsePacket TimeAttackDungeonEndBattleHandler(TimeAttackDungeonEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var TADInfo = account.ContentInfo.TimeAttackDungeonDataInfo;
            var TADGeasExcel = excelTableService.GetTable<TimeAttackDungeonGeasExcelTable>().UnPack().DataList;
            var TADGeasData = TADGeasExcel.FirstOrDefault(x => x.Id == req.Summary.StageId);

            var packet = new TimeAttackDungeonEndBattleResponse();
            if(req.Summary.EndType != BattleEndType.Clear) 
            {
                packet.RoomDB = TimeAttackDungeonManager.Instance.GetRoom();
                return packet;
            }

            var dungeonResult = TimeAttackDungeonManager.Instance.BattleResult(req.Summary, TADGeasData);
            var timePoint = TimeAttackDungeonService.CalculateTimeScore(req.Summary.EndFrame/30, TADGeasData);
            var totalpoint = TADGeasData.ClearDefaultPoint + timePoint;


            var seasonBestRecord = TADInfo.SeasonBestRecord;
            var totalAllPoint = TimeAttackDungeonService.CalculateScoreRecord(
                TimeAttackDungeonManager.Instance.GetRoom(),
                TADGeasExcel
            );

            if (seasonBestRecord < totalAllPoint)
            {
                seasonBestRecord = totalAllPoint;
                account.ContentInfo.TimeAttackDungeonDataInfo.SeasonBestRecord = totalAllPoint;
                context.SaveChanges();
            }

            return new TimeAttackDungeonEndBattleResponse()
            {
                TotalPoint = totalpoint,
                DefaultPoint = TADGeasData.ClearDefaultPoint,
                TimePoint = timePoint,
                ServerTimeTicks = TimeAttackDungeonManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.TimeAttackDungeon_GiveUp)]
        public ResponsePacket TimeAttackDungeonGiveUpHandler(TimeAttackDungeonGiveUpRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var room = TimeAttackDungeonManager.Instance.GiveUp();
            return new TimeAttackDungeonGiveUpResponse()
            {
                RoomDB = room,
                SeasonBestRecord = account.ContentInfo.TimeAttackDungeonDataInfo.SeasonBestRecord,
                ServerTimeTicks = TimeAttackDungeonManager.Instance.GetServerTime().Ticks
            };
        }
    }
}
