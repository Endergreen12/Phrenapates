using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Phrenapates.Utils;

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
                    ClanSocialGrade = Plana.FlatData.ClanSocialGrade.Member,
                    AccountNickName = account.Nickname
                },
                ClanMemberDBs = [
                    new() {
                        AccountId = account.ServerId,
                        AccountLevel = account.Level,
                        ClanDBId = 777,
                        RepresentCharacterUniqueId = 10000,
                        AttendanceCount = 33,
                        ClanSocialGrade = Plana.FlatData.ClanSocialGrade.Member,
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
            return new ClanAllAssistListResponse()
            {
                AssistCharacterDBs = [
                    new() {
                        AccountId = 1,
                        AssistCharacterServerId = 1,
                        EchelonType = Plana.FlatData.EchelonType.Raid,
                        AssistRelation = Plana.Database.AssistRelation.Friend,
                        EquipmentDBs = [],
                        WeaponDB = new(),
                        NickName = "Plana",
                        UniqueId = 20024,
                    }
                ],
                AssistCharacterRentHistoryDBs = []
            };
        }

        
    }
}
