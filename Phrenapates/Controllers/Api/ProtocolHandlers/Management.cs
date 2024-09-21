using Phrenapates.Controllers.Api.ProtocolHandlers;
using Plana.Database;
using Plana.NetworkProtocol;

namespace Plana.Controllers.Api.ProtocolHandlers
{
    public class Management : ProtocolHandlerBase
    {
        private SCHALEContext context;

        public Management(IProtocolHandlerFactory protocolHandlerFactory, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            context = _context;
        }

        [ProtocolHandler(Protocol.Management_BannerList)]
        public ResponsePacket BannerListHandler(ManagementBannerListRequest req)
        {
            //To-Do: Maybe use a DB to fetch this?
            var BannerDB = new List<BannerDB>
            {
                new() {
                    BannerOrder = 1,
                    StartDate = DateTime.Parse("2024-05-28T11:10:00"),
                    EndDate = DateTime.Parse("2024-06-28T11:10:00"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Minievent2.png",
                    LinkedLobbyBannerId = 60005,
                    BannerType = Plana.FlatData.EventContentType.MiniEvent
                },
                new() {
                    BannerOrder = 1,
                    StartDate = DateTime.Parse("2024-05-28T11:00:00"),
                    EndDate = DateTime.Parse("2024-06-28T10:59:59"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Recruitment175.png",
                    LinkedLobbyBannerId = 338,
                    BannerType = Plana.FlatData.EventContentType.Gacha,
                    BannerDisplayType = Plana.MX.Data.BannerDisplayType.Gacha
                },
                new() {
                    BannerOrder = 2,
                    StartDate = DateTime.Parse("2024-05-28T11:10:00"),
                    EndDate = DateTime.Parse("2024-06-28T10:59:59"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Student228.png",
                    LinkedLobbyBannerId = 338,
                    BannerType = Plana.FlatData.EventContentType.Gacha
                },
                new() {
                    BannerOrder = 2,
                    StartDate = DateTime.Parse("2024-05-28T11:00:00"),
                    EndDate = DateTime.Parse("2024-06-28T10:59:59"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Recruitment176.png",
                    LinkedLobbyBannerId = 339,
                    BannerType = Plana.FlatData.EventContentType.Gacha,
                    BannerDisplayType = Plana.MX.Data.BannerDisplayType.Gacha
                },
                new() {
                    BannerOrder = 3,
                    StartDate = DateTime.Parse("2024-05-28T11:10:00"),
                    EndDate = DateTime.Parse("2024-06-28T10:59:59"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Student229.png",
                    LinkedLobbyBannerId = 339,
                    BannerType = Plana.FlatData.EventContentType.Gacha
                },
                new() {
                    BannerOrder = 4,
                    StartDate = DateTime.Parse("2024-05-28T11:10:00"),
                    EndDate = DateTime.Parse("2024-06-28T10:59:59"),
                    Url = "https://ba.dn.nexoncdn.co.kr/p6tei/",
                    FileName = "Raid11B.png",
                    BannerType = Plana.FlatData.EventContentType.Raid
                }
            };

            return new ManagementBannerListResponse()
            {
                BannerDBs = BannerDB
            };
        }

        [ProtocolHandler(Protocol.Management_ProtocolLockList)]
        public ResponsePacket ProtocolLockListHandler(ManagementProtocolLockListRequest req)
        {
            var ProtocolLockDB = new List<ProtocolLockDB>
            {
                new()
            };

            return new ManagementProtocolLockListResponse()
            {
                ProtocolLockDBs = ProtocolLockDB
            };
        }

    }
}