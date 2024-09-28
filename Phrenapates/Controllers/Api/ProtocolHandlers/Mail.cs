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
			return new MailListResponse()
			{
				MailDB = mailDb,
				MailDBs = account.Mails.ToList()
			};
		}

        [ProtocolHandler(Protocol.Mail_Receive)]
        public ResponsePacket RecieveHandler(MailReceiveRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var parcelResultDb = new ParcelResultDB();
            parcelResultDb.DisplaySequence = new();
            foreach (var targetMails in req.MailServerIds.Select(x =>
            {
                return account.Mails.Where(y => y.ServerId == x).First();
            }))
            {
                ParcelService.AddOrConsumeWithParcel(account, targetMails.ParcelInfos);
                parcelResultDb.DisplaySequence.AddRange(targetMails.ParcelInfos);
                account.Mails.Remove(targetMails);
            }
            context.SaveChanges();

            parcelResultDb.AccountCurrencyDB = account.Currencies.First();
            // idk why but currently DisplaySequence causes softlock, so don't send it
            parcelResultDb.DisplaySequence = new();

            return new MailReceiveResponse()
            {
                MailServerIds = req.MailServerIds,
                ParcelResultDB = parcelResultDb
            };
        }

        public static MailDB CreateMail(long accountId)
        {
            return new()
            {
                AccountServerId = accountId,
		        Type = MailType.System,
                UniqueId = 1,
                Sender = "Plana",
		        Comment = "This is test, Sensei~",
                SendDate = DateTime.Now,
                ExpireDate = DateTime.MaxValue,
				ParcelInfos = new(),
				RemainParcelInfos = new(),
            };
        }


    }
}
