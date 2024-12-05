using Plana.MX.NetworkProtocol;
using Phrenapates.Services;
using Plana.Database;

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

        [ProtocolHandler(Protocol.TimeAttackDungeon_Lobby)]
        public ResponsePacket TimeAttackDungeonLobbyHandle(TimeAttackDungeonLobbyRequest req)
        {
            return new TimeAttackDungeonLobbyResponse()
            {
                
            };
        }
    }
}
