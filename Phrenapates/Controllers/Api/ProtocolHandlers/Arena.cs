using Phrenapates.Services;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Arena(
        IProtocolHandlerFactory protocolHandlerFactory,
        ISessionKeyService _sessionKeyService,
        SCHALEContext _context,
        ExcelTableService _excelTableService
    ) : ProtocolHandlerBase(protocolHandlerFactory)
    {
        private readonly ISessionKeyService sessionKeyService = _sessionKeyService;
        private readonly SCHALEContext context = _context;
        private readonly ExcelTableService excelTableService = _excelTableService;

        [ProtocolHandler(Protocol.Arena_EnterLobby)]
        public ResponsePacket EnterLobbyHandler(ArenaEnterLobbyRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var arenaSeason = excelTableService.GetTable<ArenaSeasonExcelTable>().UnPack().DataList
            .FirstOrDefault(x => x.UniqueId == account.ContentInfo.ArenaDataInfo.SeasonId);

            return new ArenaEnterLobbyResponse()
            {
                ArenaPlayerInfoDB = new()
                {
                    CurrentSeasonId = account.ContentInfo.ArenaDataInfo.SeasonId,
                    PlayerGroupId = 1,
                    CurrentRank = 1,
                    SeasonRecord = 1,
                    AllTimeRecord = 1
                },
                OpponentUserDBs = ArenaService.DummyOpponent(GetDefense(req.AccountId)),
                MapId = 1001
            };
        }

        [ProtocolHandler(Protocol.Arena_OpponentList)]
        public ResponsePacket OpponentListHandler(ArenaOpponentListRequest req)
        {
            return new ArenaOpponentListResponse()
            {
                PlayerRank = 1,
                OpponentUserDBs = ArenaService.DummyOpponent(GetDefense(req.AccountId)),
            };
        }

        [ProtocolHandler(Protocol.Arena_SyncEchelonSettingTime)]
        public ResponsePacket SyncEchelonSettingTimeHandler(ArenaSyncEchelonSettingTimeRequest req)
        {
            return new ArenaSyncEchelonSettingTimeResponse()
            {
                EchelonSettingTime = DateTime.Now
            };
        }

        [ProtocolHandler(Protocol.Arena_EnterBattlePart1)]
        public ResponsePacket EnterBattlePart1Handler(ArenaEnterBattlePart1Request req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var attack = context.Echelons.OrderBy(e => e.ServerId).Last(e =>
                e.AccountServerId == req.AccountId
                && e.EchelonType == EchelonType.ArenaAttack
                && e.EchelonNumber == 1
                && e.ExtensionType == EchelonExtensionType.Base
            );

            ArenaUserDB arenaUserDB = new()
            {
                RepresentCharacterUniqueId = 10059,
                NickName = "You",
                Rank = 1,
                Level = 90,
                TeamSettingDB = Convert(attack)
            };
            return new ArenaEnterBattlePart1Response()
            {
                ArenaBattleDB = new()
                {
                    Season = account.ContentInfo.ArenaDataInfo.SeasonId,
                    Group = 1,
                    BattleStartTime = DateTime.Now,
                    Seed = DateTime.Now.Ticks,
                    AttackingUserDB = arenaUserDB,
                    DefendingUserDB = ArenaService.DummyOpponent(GetDefense(req.AccountId))[0]
                },
            };
        }

        [ProtocolHandler(Protocol.Arena_EnterBattlePart2)]
        public ResponsePacket EnterBattlePart2Handler(ArenaEnterBattlePart2Request req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new ArenaEnterBattlePart2Response()
            {
                ArenaBattleDB = req.ArenaBattleDB,
                ArenaPlayerInfoDB = new ArenaPlayerInfoDB()
                {
                    CurrentSeasonId = account.ContentInfo.ArenaDataInfo.SeasonId,
                    PlayerGroupId = 1,
                    CurrentRank = 1,
                    SeasonRecord = 1,
                    AllTimeRecord = 1
                },
                AccountCurrencyDB = account.Currencies.FirstOrDefault()
            };
        }

        private EquipmentDB? GetEquipmentDB(long accountServerId, long equipmentServerId)
        {
            if (equipmentServerId == 0)
                return null;
            return context.Equipment.First(c =>
                c.AccountServerId == accountServerId && c.ServerId == equipmentServerId
            );
        }
        

        private ArenaCharacterDB? Convert(long accountServerId, long characterServerId)
        {
            if (characterServerId == 0)
                return null;
            var characterDB = context.Characters.First(c =>
                c.AccountServerId == accountServerId && c.ServerId == characterServerId
            );
            return Convert(characterDB);
        }

        private ArenaCharacterDB Convert(CharacterDB db)
        {
            var res = new ArenaCharacterDB
            {
                UniqueId = db.UniqueId,
                StarGrade = db.StarGrade,
                Level = db.Level,
                PublicSkillLevel = db.PublicSkillLevel,
                ExSkillLevel = db.ExSkillLevel,
                PassiveSkillLevel = db.PassiveSkillLevel,
                ExtraPassiveSkillLevel = db.ExtraPassiveSkillLevel,
                LeaderSkillLevel = db.LeaderSkillLevel,
                EquipmentDBs = db
                    .EquipmentServerIds.Select(i => GetEquipmentDB(db.AccountServerId, i))
                    .Where(i => i != null)
                    .ToList()!,
                FavorRankInfo = new Dictionary<long, long>
                {
                    // TODO: add all
                    { db.UniqueId, db.FavorRank }
                },
                PotentialStats = db.PotentialStats
            };
            var weaponDB = context.Weapons.FirstOrDefault(w =>
                w.AccountServerId == db.AccountServerId && w.BoundCharacterServerId == db.ServerId
            );
            if (weaponDB != null)
                res.WeaponDB = weaponDB;
            var gearDB = context.Gears.FirstOrDefault(w =>
                w.AccountServerId == db.AccountServerId && w.BoundCharacterServerId == db.ServerId
            );
            if (gearDB != null)
                res.GearDB = gearDB;
            return res;
        }

        private ArenaTeamSettingDB Convert(EchelonDB db)
        {
            var LeaderCharacterId = context
                .Characters.First(c =>
                    c.AccountServerId == db.AccountServerId && c.ServerId == db.LeaderServerId
                )
                .UniqueId;

            return new ArenaTeamSettingDB()
            {
                EchelonType = db.EchelonType,
                LeaderCharacterId = LeaderCharacterId,
                MainCharacters = db
                    .MainSlotServerIds.Select(i => Convert(db.AccountServerId, i))
                    .Where(i => i != null)
                    .ToList()!,
                SupportCharacters = db
                    .SupportSlotServerIds.Select(i => Convert(db.AccountServerId, i))
                    .Where(i => i != null)
                    .ToList()!,
                MapId = 1001,
            };
        }

        private ArenaTeamSettingDB? GetDefense(long accountId)
        {
            var defense = context.Echelons.OrderBy(e => e.ServerId).LastOrDefault(e =>
                e.AccountServerId == accountId
                && e.EchelonType == EchelonType.ArenaDefence
                && e.EchelonNumber == 1
                && e.ExtensionType == EchelonExtensionType.Base
            );
            if (defense == null)
                return null;
            return Convert(defense);
        }
    }
}
