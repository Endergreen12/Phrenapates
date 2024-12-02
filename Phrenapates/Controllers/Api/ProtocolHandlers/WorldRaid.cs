using Plana.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class WorldRaid : ProtocolHandlerBase
    {
        public WorldRaid(IProtocolHandlerFactory protocolHandlerFactory) : base(protocolHandlerFactory) { }

        [ProtocolHandler(Protocol.WorldRaid_Lobby)]
        public ResponsePacket WorldRaidLobbyHandler(WorldRaidLobbyRequest req)
        {
            return new WorldRaidLobbyResponse();
        }

        [ProtocolHandler(Protocol.WorldRaid_BossList)]
        public ResponsePacket WorldRaidBossListHandler(WorldRaidBossListRequest req)
        {
            return new WorldRaidBossListResponse();
        }

        [ProtocolHandler(Protocol.WorldRaid_EnterBattle)]
        public ResponsePacket WorldRaidEnterBattleHandler(WorldRaidEnterBattleRequest req)
        {
            return new WorldRaidEnterBattleResponse();
        }

        [ProtocolHandler(Protocol.WorldRaid_BattleResult)]
        public ResponsePacket WorldRaidBattleResultHandler(WorldRaidBattleResultRequest req)
        {
            return new WorldRaidBattleResultResponse();
        }

        [ProtocolHandler(Protocol.WorldRaid_ReceiveReward)]
        public ResponsePacket WorldRaidReceiveRewardHandler(WorldRaidReceiveRewardRequest req)
        {
            return new WorldRaidReceiveRewardResponse();
        }
    }
}
