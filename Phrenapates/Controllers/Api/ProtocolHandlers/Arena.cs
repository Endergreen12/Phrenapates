using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Arena(
        IProtocolHandlerFactory protocolHandlerFactory,
        ISessionKeyService _sessionKeyService,
        SCHALEContext _context
    ) : ProtocolHandlerBase(protocolHandlerFactory)
    {
        private readonly ISessionKeyService sessionKeyService = _sessionKeyService;
        private readonly SCHALEContext context = _context;

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

        private static readonly ArenaTeamSettingDB dummyTeam =
            new()
            {
                EchelonType = EchelonType.ArenaDefence,
                LeaderCharacterId = 10065,
                MainCharacters =
                [
                    new ArenaCharacterDB()
                    {
                        UniqueId = 10065,
                        StarGrade = 3,
                        Level = 90,
                        PublicSkillLevel = 1,
                        ExSkillLevel = 1,
                        PassiveSkillLevel = 1,
                        ExtraPassiveSkillLevel = 1,
                        LeaderSkillLevel = 1
                    }
                ],
                MapId = 1001,
            };

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

        private static List<ArenaUserDB> DummyOpponent(ArenaTeamSettingDB? team)
        {
            return
            [
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 20024,
                    NickName = "your",
                    Rank = 2,
                    Level = 90,
                    TeamSettingDB = team ?? dummyTeam
                },
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 10059,
                    NickName = "defense",
                    Rank = 3,
                    Level = 90,
                    TeamSettingDB = team ?? dummyTeam
                },
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 10065,
                    NickName = "team",
                    Rank = 4,
                    Level = 90,
                    TeamSettingDB = team ?? dummyTeam
                }
            ];
        }

        [ProtocolHandler(Protocol.Arena_EnterLobby)]
        public ResponsePacket EnterLobbyHandler(ArenaEnterLobbyRequest req)
        {
            return new ArenaEnterLobbyResponse()
            {
                ArenaPlayerInfoDB = new()
                {
                    CurrentSeasonId = 6,
                    PlayerGroupId = 1,
                    CurrentRank = 1,
                    SeasonRecord = 1,
                    AllTimeRecord = 1
                },
                OpponentUserDBs = DummyOpponent(GetDefense(req.AccountId)),
                MapId = 1001
            };
        }

        [ProtocolHandler(Protocol.Arena_OpponentList)]
        public ResponsePacket OpponentListHandler(ArenaOpponentListRequest req)
        {
            return new ArenaOpponentListResponse()
            {
                PlayerRank = 1,
                OpponentUserDBs = DummyOpponent(GetDefense(req.AccountId))
            };
        }

        [ProtocolHandler(Protocol.Arena_SyncEchelonSettingTime)]
        public ResponsePacket SyncEchelonSettingTimeHandler(ArenaSyncEchelonSettingTimeRequest req)
        {
            return new ArenaSyncEchelonSettingTimeResponse() { EchelonSettingTime = DateTime.Now };
        }

        [ProtocolHandler(Protocol.Arena_EnterBattlePart1)]
        public ResponsePacket EnterBattlePart1Handler(ArenaEnterBattlePart1Request req)
        {
            var attack = context.Echelons.OrderBy(e => e.ServerId).Last(e =>
                e.AccountServerId == req.AccountId
                && e.EchelonType == EchelonType.ArenaAttack
                && e.EchelonNumber == 1
                && e.ExtensionType == EchelonExtensionType.Base
            );

            ArenaUserDB arenaUserDB =
                new()
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
                    Season = 6,
                    Group = 1,
                    BattleStartTime = DateTime.Now,
                    Seed = 1,
                    AttackingUserDB = arenaUserDB,
                    DefendingUserDB = DummyOpponent(GetDefense(req.AccountId))[0]
                }
            };
        }

        [ProtocolHandler(Protocol.Arena_EnterBattlePart2)]
        public ResponsePacket EnterBattlePart2Handler(ArenaEnterBattlePart2Request req)
        {
            return new ArenaEnterBattlePart2Response()
            {
                ArenaBattleDB = req.ArenaBattleDB,
                ArenaPlayerInfoDB = new ArenaPlayerInfoDB()
                {
                    CurrentSeasonId = 6,
                    PlayerGroupId = 1,
                    CurrentRank = 1,
                    SeasonRecord = 1,
                    AllTimeRecord = 1
                },
                AccountCurrencyDB = new AccountCurrencyDB
                {
                    AccountLevel = 90,
                    AcademyLocationRankSum = 1,
                    CurrencyDict = new Dictionary<CurrencyTypes, long>
                    {
                        { CurrencyTypes.Gem, long.MaxValue }, // gacha currency 600
                        { CurrencyTypes.GemPaid, 0 },
                        { CurrencyTypes.GemBonus, long.MaxValue }, // default blue gem?
                        { CurrencyTypes.Gold, 962_350_000 }, // credit 10,000
                        { CurrencyTypes.ActionPoint, long.MaxValue }, // energy  24
                        { CurrencyTypes.AcademyTicket, 3 },
                        { CurrencyTypes.ArenaTicket, 5 },
                        { CurrencyTypes.RaidTicket, 3 },
                        { CurrencyTypes.WeekDungeonChaserATicket, 0 },
                        { CurrencyTypes.WeekDungeonChaserBTicket, 0 },
                        { CurrencyTypes.WeekDungeonChaserCTicket, 0 },
                        { CurrencyTypes.SchoolDungeonATicket, 0 },
                        { CurrencyTypes.SchoolDungeonBTicket, 0 },
                        { CurrencyTypes.SchoolDungeonCTicket, 0 },
                        { CurrencyTypes.TimeAttackDungeonTicket, 3 },
                        { CurrencyTypes.MasterCoin, 0 },
                        { CurrencyTypes.WorldRaidTicketA, 40 },
                        { CurrencyTypes.WorldRaidTicketB, 40 },
                        { CurrencyTypes.WorldRaidTicketC, 40 },
                        { CurrencyTypes.ChaserTotalTicket, 6 },
                        { CurrencyTypes.SchoolDungeonTotalTicket, 6 },
                        { CurrencyTypes.EliminateTicketA, 1 },
                        { CurrencyTypes.EliminateTicketB, 1 },
                        { CurrencyTypes.EliminateTicketC, 1 },
                        { CurrencyTypes.EliminateTicketD, 1 }
                    },
                    UpdateTimeDict = new Dictionary<CurrencyTypes, DateTime>
                    {
                        { CurrencyTypes.ActionPoint, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.AcademyTicket, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.ArenaTicket, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.RaidTicket, DateTime.Parse("2024-04-26T19:29:12") },
                        {
                            CurrencyTypes.WeekDungeonChaserATicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.WeekDungeonChaserBTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.WeekDungeonChaserCTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.SchoolDungeonATicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.SchoolDungeonBTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.SchoolDungeonCTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        {
                            CurrencyTypes.TimeAttackDungeonTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        { CurrencyTypes.MasterCoin, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.WorldRaidTicketA, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.WorldRaidTicketB, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.WorldRaidTicketC, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.ChaserTotalTicket, DateTime.Parse("2024-04-26T19:29:12") },
                        {
                            CurrencyTypes.SchoolDungeonTotalTicket,
                            DateTime.Parse("2024-04-26T19:29:12")
                        },
                        { CurrencyTypes.EliminateTicketA, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.EliminateTicketB, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.EliminateTicketC, DateTime.Parse("2024-04-26T19:29:12") },
                        { CurrencyTypes.EliminateTicketD, DateTime.Parse("2024-04-26T19:29:12") }
                    }
                }
            };
        }
    }
}
