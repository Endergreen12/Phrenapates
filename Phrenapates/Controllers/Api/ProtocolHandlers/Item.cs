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
            Dictionary<long, ItemDB> itemDBParcel = [];
            Dictionary<long, EquipmentDB> equipmentDBParcel = [];

            foreach (var parcel in req.TargetParcels)
            {
                var parcelData = parcelExcel.FirstOrDefault(x => x.RequireParcelId == parcel.Id);
                if(parcelData.RequireParcelType == ParcelType.Item)
                {
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
                    itemDBParcel.Remove(reducedItem.UniqueId);
                    itemDBParcel.Remove(synthItem.UniqueId);
                    itemDBParcel.Add(reducedItem.UniqueId, reducedItem);
                    itemDBParcel.Add(synthItem.UniqueId, synthItem);
                    context.SaveChanges();
                }
                else if (parcelData.RequireParcelType == ParcelType.Equipment)
                {
                    var equipmentParcel = account.Equipment.FirstOrDefault(x => x.UniqueId == parcelData.RequireParcelId);

                    double totalEquipmentRemoved = 0;
                    var totalSynthAdded = 0;
                    if(equipmentParcel.StackCount > parcelData.SynthStartAmount)
                    {
                        totalEquipmentRemoved = equipmentParcel.StackCount - parcelData.SynthEndAmount;
                        totalSynthAdded = (int)Math.Floor(totalEquipmentRemoved / parcelData.RequireParcelAmount);
                    }

                    var reducedEq = account.Equipment.FirstOrDefault(x => x.UniqueId == parcelData.RequireParcelId);
                    var synthEq = account.Equipment.FirstOrDefault(x => x.UniqueId == parcelData.ResultParcelId);
                    reducedEq.StackCount -= (int)totalEquipmentRemoved;
                    synthEq.StackCount += totalSynthAdded;
                    equipmentDBParcel.Remove(reducedEq.UniqueId);
                    equipmentDBParcel.Remove(synthEq.UniqueId);
                    equipmentDBParcel.Add(reducedEq.UniqueId, reducedEq);
                    equipmentDBParcel.Add(synthEq.UniqueId, synthEq);
                    context.SaveChanges();
                }
            }
            return new ItemAutoSynthResponse()
            {
                ParcelResultDB = new()
                {
                    ItemDBs = itemDBParcel,
                    EquipmentDBs = equipmentDBParcel
                }
            };
        }
    }
}
