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
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var parcelExcel = excelTableService.GetTable<ParcelAutoSynthExcelTable>().UnPack().DataList;
            Dictionary<long, ItemDB> parcelResult = [];

            foreach (var parcel in req.TargetParcels)
            {
                var parcelData = parcelExcel.FirstOrDefault(x => x.RequireParcelId == parcel.Id);
                var itemParcel = account.Items.FirstOrDefault(x => x.UniqueId == parcelData.RequireParcelId);

                double totalItemRemoved = 0;
                var totalSynthAdded = 0;
                if(itemParcel.StackCount > parcelData.SynthStartAmount)
                {
                    totalItemRemoved = itemParcel.StackCount - parcelData.SynthEndAmount;
                    totalSynthAdded = (int)Math.Floor(totalItemRemoved / parcelData.RequireParcelAmount);
                }

                var reducedItem = account.Items.FirstOrDefault(x => x.UniqueId == parcelData.RequireParcelId);
                var synthItem = account.Items.FirstOrDefault(x => x.UniqueId == parcelData.ResultParcelId);
                reducedItem.StackCount -= (int)totalItemRemoved;
                synthItem.StackCount += totalSynthAdded;
                parcelResult.Remove(reducedItem.UniqueId);
                parcelResult.Remove(synthItem.UniqueId);
                parcelResult.Add(reducedItem.UniqueId, reducedItem);
                parcelResult.Add(synthItem.UniqueId, synthItem);
                account.Items.FirstOrDefault(x => x.UniqueId == parcelData.RequireParcelId).StackCount = reducedItem.StackCount;
                account.Items.FirstOrDefault(x => x.UniqueId == parcelData.ResultParcelId).StackCount = synthItem.StackCount;
                context.SaveChanges();
            }

            return new ItemAutoSynthResponse()
            {
                ParcelResultDB = new()
                {
                    ItemDBs = parcelResult
                }
            };
        }
    }
}
