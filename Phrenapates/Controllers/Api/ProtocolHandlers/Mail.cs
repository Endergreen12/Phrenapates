using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Plana.Parcel;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Mail : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;
		
		public Mail(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory) 
		{
			sessionKeyService = _sessionKeyService;
            context = _context;
		}

        [ProtocolHandler(Protocol.Mail_Check)]
        public ResponsePacket CheckHandler(MailCheckRequest req)
        {

			return new MailCheckResponse()
            {
				
            };
        }
		
		[ProtocolHandler(Protocol.Mail_List)]
		public ResponsePacket ListHandler(MailListRequest req)
		{
			var account = sessionKeyService.GetAccount(req.SessionKey);
			var mailDb = account.Mails.FirstOrDefault();
			if (req.IsReadMail)
			{
			    return new MailListResponse()
			    {
			        MailDB = mailDb,
			        MailDBs = account.Mails.Where(y => y.ReceiptDate is not null).ToList()
			    }
			    return;
			}
			return new MailListResponse()
			{
				MailDB = mailDb,
				MailDBs = account.Mails.Where(y => y.ReceiptDate is null).ToList()
			};
		}

        [ProtocolHandler(Protocol.Mail_Receive)]
        public ResponsePacket RecieveHandler(MailReceiveRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var parcelResultDb = new ParcelResultDB();
            parcelResultDb.DisplaySequence = new();
            parcelResultDb.ParcelForMission = new();
            foreach (var targetMails in req.MailServerIds.Select(x =>
            {
                return account.Mails.Where(y => y.ServerId == x).First();
            }))
            {
                ParcelService.AddOrConsumeWithParcel(account, targetMails.ParcelInfos);
                parcelResultDb.DisplaySequence.AddRange(targetMails.ParcelInfos);
                parcelResultDb.ParcelForMission.AddRange(target Mails.ParcelInfos);
                //account.Mails.Remove(targetMails);
                targetMails.ReceiptDate = DateTime.Now;
            }
            context.SaveChanges();

            parcelResultDb.AccountCurrencyDB = account.Currencies.First();
            // idk why but currently DisplaySequence causes softlock, so don't send it
            //parcelResultDb.DisplaySequence = new();

            return new MailReceiveResponse()
            {
                MailServerIds = req.MailServerIds,
                ParcelResultDB = parcelResultDb
            };
        }

        public static MailDB CreateMail(long accountId)
        {
            List<ParcelInfo> ParcelInfos = new();
            ParcelInfos.Add(new
            {
                Key = new()
                {
                    Type = MailType.System,
                    Id = CurrencyType.GemBonus,
                }
                Amount = 600,
                Multiplier = new(),
                Probability = new(),
            });
            var SystemMail = ExcelTableService
                .GetTable<SystemMailExcelTable>()
                .UnPack()
                .Where(y => y.MailType == MailType.System)
                .First();
            DateTime date = DateTime.Now;
            return new()
            {
                AccountServerId = accountId,
		        Type = MailType.System,
                UniqueId = 2,
                Sender = SystemMail.Sender,
		        Comment = SystemMail.Comment,
                SendDate = date,
                ExpireDate = date.AddDays(SystemMail.ExpiredDay),
				ParcelInfos = ParcelInfos,
				RemainParcelInfos = new(),
            };
        }


    }
}
