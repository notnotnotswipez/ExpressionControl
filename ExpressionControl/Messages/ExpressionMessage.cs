using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace ExpressionControl.Messages
{
    public class ExpressionMessageData : IFusionSerializable
    {
        public PlayerId playerId;
        public string dictString;

        public void Deserialize(FusionReader reader)
        {
            playerId = PlayerIdManager.GetPlayerId(reader.ReadByte());
            dictString = reader.ReadString();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(playerId.SmallId);
            writer.Write(dictString);
        }

        public static ExpressionMessageData Create(Dictionary<string, bool> dict)
        {
            return new ExpressionMessageData()
            {
                playerId = PlayerIdManager.LocalId,
                dictString = Core.GetStringFromDictionary(dict)
            };
        }
    }

    public class ExpressionMessage : ModuleMessageHandler
    {
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                ExpressionMessageData data = reader.ReadFusionSerializable<ExpressionMessageData>();

                if (NetworkInfo.IsServer && isServerHandled)
                {
                    using (var message = FusionMessage.ModuleCreate<ExpressionMessage>(bytes))
                    {
                        MessageSender.BroadcastMessageExcept(data.playerId, NetworkChannel.Reliable, message);
                    }
                }

                if (data.playerId.IsMe)
                {
                    return;
                }

                if (NetworkPlayerManager.TryGetPlayer(data.playerId, out var player))
                {
                    Dictionary<string, bool> dict = Core.GetDictionaryFromString(data.dictString);
                    if (dict != null)
                    {
                        Core.ApplyBlendshapesToRigmanager(player.RigRefs.RigManager, dict);
                    }
                }
            }
        }
    }
}
