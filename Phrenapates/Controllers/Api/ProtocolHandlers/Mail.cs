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
			    };
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
                parcelResultDb.ParcelForMission.AddRange(targetMails.ParcelInfos);
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
            ParcelInfos.Add(new()
            {
                Key = new()
                {
                    Type = ParcelType.Currency,
                    Id = 3,
                }
                Amount = 600,
                Multiplier = new(),
                Probability = new(),
            });
            DateTime date = DateTime.Now;
            return new()
            {
                AccountServerId = accountId,
		        Type = MailType.System,
                UniqueId = 2,
                Sender = "UI_MAILBOX_POST_SENDER_ARONA",
		        Comment = "Mail_NewUserBonus",
                SendDate = date,
                ExpireDate = date.AddDays(7),
				ParcelInfos = ParcelInfos,
				RemainParcelInfos = new(),
            };
        }


    }
}
