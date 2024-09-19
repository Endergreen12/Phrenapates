using Microsoft.EntityFrameworkCore;
using Plana.Database;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Echelon(
        IProtocolHandlerFactory protocolHandlerFactory,
        ISessionKeyService _sessionKeyService,
        SCHALEContext _context,
        ExcelTableService _excelTableService,
        ILogger<Echelon> _logger
    ) : ProtocolHandlerBase(protocolHandlerFactory)
    {
        private readonly ISessionKeyService sessionKeyService = _sessionKeyService;
        private readonly SCHALEContext context = _context;
        private readonly ExcelTableService excelTableService = _excelTableService;
        private readonly ILogger<Echelon> logger = _logger;

        [ProtocolHandler(Protocol.Echelon_List)]
        public ResponsePacket ListHandler(EchelonListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new EchelonListResponse() { EchelonDBs = [.. account.Echelons], ArenaEchelonDB = new() };
        }

        [ProtocolHandler(Protocol.Echelon_Save)]
        public ResponsePacket SaveHandler(EchelonSaveRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var newEchelon = req.EchelonDB;
            var existingEchelon = context.Echelons.FirstOrDefault(e => e.AccountServerId == newEchelon.AccountServerId && e.EchelonType == newEchelon.EchelonType &&
                                                                    e.EchelonNumber == newEchelon.EchelonNumber && e.ExtensionType == newEchelon.ExtensionType);

            if (existingEchelon != null)
            {
                context.Echelons.Remove(existingEchelon);
                context.SaveChanges();
            }

            account.AddEchelons(context, [newEchelon]);
            context.SaveChanges();
            
            return new EchelonSaveResponse() { EchelonDB = req.EchelonDB, };
        }
    }
}
