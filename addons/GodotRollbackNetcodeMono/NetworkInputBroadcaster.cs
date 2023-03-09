using GDDictionary = Godot.Collections.Dictionary;
using Godot;
using System.Collections.Generic;

namespace GodotRollbackNetcode
{
    #region Interfaces
    public interface INIBSync { }

    // Manual interfaces are not called automatically by the sync manager. Rather, it's the user's
    // Responsibility for calling those.
    public interface INIBProcess : INIBSync
    {
        void _NIBProcess(GDDictionary input);
    }

    public interface INIBPreProcess : INIBSync
    {
        void _NIBPreprocess(GDDictionary input);
    }

    public interface INIBPostProcess : INIBSync
    {
        void _NIBPostprocess(GDDictionary input);
    }

    public interface INIBPredictRemoteInput : INIBSync
    {
        GDDictionary _NIBPredictRemoteInput(GDDictionary previousInput, int ticksSinceRealInput);
    }

    public interface INIBGetLocalInput : INIBSync
    {
        GDDictionary _NIBGetLocalInput();
    }
    #endregion

    /// <summary>
    /// NetworkInputBroadcaster acts as a intermediate node that communicates with the 
    /// rollback netcode addon. It broadcasts NetworkPreProcess, NetworkProcess, NetworkPostProcess 
    /// events using input obtained from InputGetter and InputPredictor.
    /// 
    /// This node lets you split up input fetching from input consumption -- rather than forcing
    /// any node that generates input to also consume it, you can have a separate node generate input and pass it
    /// to a NetworkInputBroadcaster, which then forwards it to nodes that may want to consume it, whether through
    /// NetworkProcess, NetworkPreprocess, or NetworkPostprocess calls.
    /// </summary>
    public class NetworkInputBroadcaster : Node, INetworkPreProcess, INetworkProcess, INetworkPostProcess, IGetLocalInput, IPredictRemoteInput
    {
        [Export]
        private NodePath inputGetterNodePath;
        [Export]
        private NodePath inputPredictorNodePath;
        [Export]
        private List<NodePath> networkReceiverNodePaths = new List<NodePath>();

        public IReadOnlyCollection<Node> NetworkReceiverNodes => networkReceiverNodes;
        public INIBGetLocalInput InputGetter { get; set; }
        public INIBPredictRemoteInput InputPredictor { get; set; }

        private HashSet<Node> networkReceiverNodes = new HashSet<Node>();
        private HashSet<INIBPreProcess> preprocessNodes = new HashSet<INIBPreProcess>();
        private HashSet<INIBProcess> processNodes = new HashSet<INIBProcess>();
        private HashSet<INIBPostProcess> postprocessNodes = new HashSet<INIBPostProcess>();

        public bool AddNetworkReceiver(Node node)
        {
            if (!networkReceiverNodes.Contains(node))
            {
                networkReceiverNodes.Add(node);
                if (node is INIBPreProcess preprocess)
                    preprocessNodes.Add(preprocess);
                if (node is INIBProcess process)
                    processNodes.Add(process);
                if (node is INIBPostProcess postprocess)
                    postprocessNodes.Add(postprocess);
                return true;
            }
            return false;
        }

        public bool RemoveNetworkReciever(Node node)
        {
            if (networkReceiverNodes.Contains(node))
            {
                networkReceiverNodes.Remove(node);
                preprocessNodes.Remove(node as INIBPreProcess);
                processNodes.Remove(node as INIBProcess);
                postprocessNodes.Remove(node as INIBPostProcess);
                return true;
            }
            return false;
        }

        public override void _Ready()
        {
            InputGetter = GetNode<INIBGetLocalInput>(inputGetterNodePath);
            InputPredictor = GetNode<INIBPredictRemoteInput>(inputPredictorNodePath);
            foreach (var path in networkReceiverNodePaths)
                networkReceiverNodes.Add(GetNode(path));
        }

        public void _NetworkPostprocess(GDDictionary input)
        {
            foreach (var node in postprocessNodes)
                node._NIBPostprocess(input);
        }

        public void _NetworkPreprocess(GDDictionary input)
        {
            foreach (var node in preprocessNodes)
                node._NIBPreprocess(input);
        }

        public void _NetworkProcess(GDDictionary input)
        {
            foreach (var node in processNodes)
                node._NIBProcess(input);
        }

        public GDDictionary _GetLocalInput() => InputGetter?._NIBGetLocalInput();
        public GDDictionary _PredictRemoteInput(GDDictionary previousInput, int ticksSinceRealInput) => InputPredictor?._NIBPredictRemoteInput(previousInput, ticksSinceRealInput);
    }
}
