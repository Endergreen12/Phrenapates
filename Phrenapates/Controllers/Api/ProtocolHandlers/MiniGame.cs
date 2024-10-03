using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class MiniGame : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;

        public MiniGame(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
        }

        [ProtocolHandler(Protocol.MiniGame_StageList)]
        public ResponsePacket StageListHandler(MiniGameStageListRequest req)
        {
            return new MiniGameStageListResponse();
        }

        [ProtocolHandler(Protocol.MiniGame_MissionList)]
        public ResponsePacket MissionListHandler(MiniGameMissionListRequest req)
        {
            return new MiniGameMissionListResponse();
        }

        [ProtocolHandler(Protocol.MiniGame_EnterStage)]
        public ResponsePacket EnterStageHandler(MiniGameEnterStageRequest req)
        {
            return new MiniGameEnterStageResponse();
        }
    }
}
