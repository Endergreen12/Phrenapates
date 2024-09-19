using Plana.Database;
using Plana.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Academy : ProtocolHandlerBase
    {
        private SCHALEContext context;

        public Academy(IProtocolHandlerFactory protocolHandlerFactory, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            context = _context;
        }

        [ProtocolHandler(Protocol.Academy_GetInfo)]
        public ResponsePacket GetInfoHandler(AcademyGetInfoRequest req)
        {
            // context.CurrentPlayer.MissionProgressDBs
            var MissionProgressDBs = new List<MissionProgressDB> 
            { 
                new() {
                    MissionUniqueId = 1700,
                    Complete = false,
                    StartTime = DateTime.UtcNow,
                    ProgressParameters = new Dictionary<long, long>
                    {
                        { 0, 2 }
                    }
                }
            };

            return new AcademyGetInfoResponse()
            {
                AcademyDB = new()
                {
                    AccountId = req.SessionKey?.AccountServerId ?? 0
                },
                AcademyLocationDBs = [],
                MissionProgressDBs = MissionProgressDBs,
                //MissionProgressDBs = context.CurrentPlayer.MissionProgressDBs,
            };
        }
    }
}
