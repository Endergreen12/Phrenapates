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
            // Mission_Sync is called periodically, but if it is called during the execution of a command,
            // an exception will be raised because the DbContext will be accessed at the same time
            List<MissionProgressDB> missionList = new();
            try
            {
                missionList = (List<MissionProgressDB>)sessionKeyService.GetAccount(req.SessionKey).MissionProgresses;
            }
            catch (Exception ex)
            {

            }

            return new MissionSyncResponse()
            {
                MissionProgressDBs = missionList
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
