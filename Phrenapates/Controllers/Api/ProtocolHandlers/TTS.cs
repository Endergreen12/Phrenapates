using Phrenapates.Controllers.Api.ProtocolHandlers;
using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class TTS : ProtocolHandlerBase
    {
        private SCHALEContext context;

        public TTS(IProtocolHandlerFactory protocolHandlerFactory, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            context = _context;
        }

        [ProtocolHandler(Protocol.TTS_GetKana)]
        public ResponsePacket GetKanaHandler(TTSGetKanaRequest req)
        {
            string ActualCallName = String.Join<char>(" ", req.CallName.ToLower());
            //To-Do: Figure out a way to change English names to Katakana
            //string CallNameKatakana = Strings.StrConv(req.CallName, VbStrConv.Katakana, 1041);
            string CallNameKatakana = "センセイ";

            return new TTSGetKanaResponse()
            {
                CallName = req.CallName,
                ActualCallName = ActualCallName,
                CallNameKatakana = CallNameKatakana
            };
        }
    }
}