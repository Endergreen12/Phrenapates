using Plana.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class ProofToken : ProtocolHandlerBase
    {
        public ProofToken(IProtocolHandlerFactory protocolHandlerFactory) : base(protocolHandlerFactory) { }

        [ProtocolHandler(Protocol.ProofToken_RequestQuestion)]
        public ResponsePacket RequestQuestionHandler(ProofTokenRequestQuestionRequest req)
        {
            return new ProofTokenRequestQuestionResponse()
            {
                Hint = 69,
                Question = "seggs"
            };
        }

        [ProtocolHandler(Protocol.ProofToken_Submit)]
        public ResponsePacket SubmitHandler(ProofTokenSubmitRequest req)
        {
            return new ProofTokenSubmitResponse();
        }
    }
}
