﻿using Plana.Database;
using Plana.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Queuing : ProtocolHandlerBase
    {
        private readonly SCHALEContext context;

        public Queuing(IProtocolHandlerFactory protocolHandlerFactory, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            context = _context;
        }

        [ProtocolHandler(Protocol.Queuing_GetTicket)]
        public ResponsePacket GetTicketHandler(QueuingGetTicketRequest req)
        {
            return new QueuingGetTicketResponse()
            {
                EnterTicket = $"{req.YostarUID}:{req.YostarToken}"
            };
        }

        [ProtocolHandler(Protocol.Queuing_GetTicketGL)]
        public ResponsePacket GetTicketGLHandler(QueuingGetTicketGLRequest req)
        {
            // Create guest account on here, workaround
            if (!context.GuestAccounts.Any(x => x.Uid == req.NpSN && x.Token == req.NpToken))
            {
                context.GuestAccounts.Add(new()
                {
                    Uid = req.NpSN,
                    DeviceId = "",
                    Token = req.NpToken,
                });

                context.SaveChanges();
            }

            return new QueuingGetTicketGLResponse()
            {
                EnterTicket = $"{req.NpSN}:{req.NpToken}"
            };
        }
    }
}
