using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Serilog;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Mission : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;

        public Mission(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
        }

        [ProtocolHandler(Protocol.Mission_Sync)]
        public ResponsePacket SyncHandler(MissionSyncRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new MissionSyncResponse()
            {
                MissionProgressDBs = [.. account.MissionProgresses]
            };
        }

        [ProtocolHandler(Protocol.Mission_List)]
        public ResponsePacket ListHandler(MissionListRequest req)
        {
            Log.Debug($"MissionListRequest EventContentId: {req.EventContentId}");

            var missionProgresses = context.MissionProgresses.Where(x => x.AccountServerId == sessionKeyService.GetAccount(req.SessionKey).ServerId).ToList();

            return new MissionListResponse
            {
                ProgressDBs = missionProgresses
            };
        }

        [ProtocolHandler(Protocol.Mission_GuideMissionSeasonList)]
        public ResponsePacket GuideMissionSeasonListHandler(GuideMissionSeasonListRequest req)
        {
            return new GuideMissionSeasonListResponse();
        }
    }
}
