using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Plana.FlatData;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Item : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Item(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Item_List)]
        public ResponsePacket ListHandler(ItemListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new ItemListResponse()
            {
                ItemDBs = [.. account.Items],
                ExpiryItemDBs = []
            };
        }

        [ProtocolHandler(Protocol.Item_AutoSynth)]
        public ResponsePacket AutoSynthHandler(ItemAutoSynthRequest req)
        {
            //TODO: Implement
            /*var account = sessionKeyService.GetAccount(req.SessionKey);
            var parcelExcel = excelTableService.GetTable<ParcelAutoSynthExcelTable>().UnPack().DataList;
            var itemExcel = excelTableService.GetTable<ItemExcelTable>().UnPack().DataList;

            foreach (var parcel in req.TargetParcels)
            {
                var parcelData = parcelExcel.FirstOrDefault(x => parcel.Equals(x.RequireParcelId));
            }*/

            return new ItemAutoSynthResponse()
            {
                ParcelResultDB = new()
            };
        }
    }
}
