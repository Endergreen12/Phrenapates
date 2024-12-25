using Phrenapates.Services;
using Phrenapates.Utils;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Clan : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Clan(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Clan_Lobby)]
        public ResponsePacket CheckHandler(ClanLobbyRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new ClanLobbyResponse()
            {
                IrcConfig = new()
                {
                    HostAddress = Config.Instance.IRCAddress,
                    Port = Config.Instance.IRCPort,
                    Password = ""
                },
                AccountClanDB = new()
                {
                    ClanDBId = 777,
                    ClanName = "Throne of Naram-Sin",
                    ClanChannelName = "channel_1",
                    ClanPresidentNickName = "Plana",
                    ClanPresidentRepresentCharacterUniqueId = 10000,
                    ClanNotice = "",
                    ClanMemberCount = 1,
                },
                AccountClanMemberDB = new()
                {
                    AccountId = account.ServerId,
                    AccountLevel = account.Level,
                    ClanDBId = 777,
                    RepresentCharacterUniqueId = 10000,
                    ClanSocialGrade = ClanSocialGrade.Member,
                    AccountNickName = account.Nickname
                },
                ClanMemberDBs = [
                    new() {
                        AccountId = account.ServerId,
                        AccountLevel = account.Level,
                        ClanDBId = 777,
                        RepresentCharacterUniqueId = 10000,
                        AttendanceCount = 33,
                        ClanSocialGrade = ClanSocialGrade.Member,
                        AccountNickName = account.Nickname,
                        AttachmentDB = new() {
                            AccountId = account.ServerId,
                            EmblemUniqueId = 123123
                        }
                    }
                ],
                
            };
        }

        [ProtocolHandler(Protocol.Clan_Check)]
        public ResponsePacket CheckHandler(ClanCheckRequest req)
        {
            return new ClanCheckResponse();
        }

        [ProtocolHandler(Protocol.Clan_AllAssistList)]
        public ResponsePacket AllAssistListHandler(ClanAllAssistListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            /*var equipmentExcel = excelTableService.GetTable<EquipmentExcelTable>().UnPack().DataList;
            var weaponExcel = excelTableService.GetTable<CharacterWeaponExcelTable>().UnPack().DataList;
            var uniqueGearExcel = excelTableService.GetExcelDB<CharacterGearExcel>();
            var characterExcel = excelTableService.GetTable<CharacterExcelTable>().UnPack().DataList.Where(x =>
                x is
                {
                    IsPlayable: true,
                    IsPlayableCharacter: true,
                    IsNPC: false,
                    ProductionStep: ProductionStep.Release,
                }
            );

            List<AssistCharacterDB> assistCharData = [];
            var fakeAssistServerId = 1000000000;
            var fakeCharServerId = 1000000000;
            var fakeEqServerId = 1000000000;
            var fakeWpServerId = 1000000000;
            var fakeUniqueEqServerId = 1000000000;

            foreach (var character in characterExcel)
            {
                // Create assist character data
                var assistCharacter = new AssistCharacterDB
                {
                    // Assist Data
                    AccountId = 69,
                    AssistCharacterServerId = fakeAssistServerId,
                    EchelonType = req.EchelonType,
                    AssistRelation = AssistRelation.Friend,
                    NickName = "Plana",

                    // Character Data
                    UniqueId = character.Id,
                    Level = 90,
                    Exp = 0,
                    FavorRank = 20,
                    FavorExp = 0,
                    StarGrade = 5,
                    ExSkillLevel = 5,
                    PublicSkillLevel = 10,
                    PassiveSkillLevel = 10,
                    ExtraPassiveSkillLevel = 10,
                    LeaderSkillLevel = 1,
                    IsNew = true,
                    PotentialStats = { { 1, 0 }, { 2, 0 }, { 3, 0 } },

                    // Some data are nullable, expect empty value.
                    EquipmentDBs = character.EquipmentSlot.Select(slot =>
                    {
                        var equipmentData = equipmentExcel.FirstOrDefault(e => e.EquipmentCategory == slot && e.MaxLevel == 65);
                        if (equipmentData == null) return new();

                        return new EquipmentDB
                        {
                            UniqueId = equipmentData.Id,
                            Level = equipmentData.MaxLevel,
                            Tier = (int)equipmentData.TierInit,
                            IsNew = true,
                            StackCount = 1,
                            BoundCharacterServerId = fakeCharServerId,
                            ServerId = fakeEqServerId++
                        };
                    }).ToList(),

                    WeaponDB = weaponExcel.FirstOrDefault(w => w.Id == character.Id) is { Unlock: var unlocks }
                        ? new WeaponDB
                        {
                            UniqueId = character.Id,
                            IsLocked = false,
                            StarGrade = unlocks.TakeWhile(unlock => unlock).Count(),
                            Level = 50,
                            BoundCharacterServerId = fakeCharServerId,
                            ServerId = fakeWpServerId++
                        }
                        : new(),

                    GearDB = uniqueGearExcel.FirstOrDefault(g => g.CharacterId == character.Id && g.Tier == 2) is { Id: var gearId }
                        ? new GearDB
                        {
                            UniqueId = gearId,
                            Level = 1,
                            SlotIndex = 4,
                            Tier = 2,
                            Exp = 0,
                            BoundCharacterServerId = fakeCharServerId,
                            ServerId = fakeUniqueEqServerId++
                        }
                        : new()
                };
                assistCharData.Add(assistCharacter);
                fakeAssistServerId++;
                fakeCharServerId++;
            }*/

            return new ClanAllAssistListResponse()
            {
                AssistCharacterDBs = new(), //assistCharData,
                AssistCharacterRentHistoryDBs = []
            };
        }
    }
}
