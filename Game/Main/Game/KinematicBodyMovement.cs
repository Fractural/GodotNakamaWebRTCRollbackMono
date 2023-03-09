using Fractural.GodotCodeGenerator.Attributes;
using Godot;
using GodotRollbackNetcode;
using GDC = Godot.Collections;

namespace NakamaWebRTCDemo
{
    public partial class KinematicBodyMovement : Node, IMovement, IEnable, INetworkSerializable
    {
        [OnReadyGet]
        private KinematicBody2D body;

        [Export]
        public bool Enabled { get; set; } = true;
        [Export]
        public Vector2 Direction { get; set; }
        [Export]
        public float Speed { get; set; } = 10f;

        [Puppet]
        private void UpdateRemoteBody(Vector2 position)
        {
            body.GlobalPosition = position;
        }

        #region Rollback
        public void _ManualNetworkProcess(GDC.Dictionary input)
        {
            if (!Enabled || this.TryIsNotNetworkMaster()) return;

            body.MoveAndSlide(Direction * Speed);

            this.TryRpc(RpcType.Local | RpcType.Master | RpcType.Unreliable, nameof(UpdateRemoteBody), body.GlobalPosition);
        }

        public GDC.Dictionary _SaveState()
        {
            return new GDC.Dictionary()
            {
                ["position"] = 1234
            };
        }

        public void _LoadState(GDC.Dictionary state)
        {
            throw new NotImplementedException();
        }

        public void _InterpolateState(GDC.Dictionary oldState, GDC.Dictionary newState, float weight)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
