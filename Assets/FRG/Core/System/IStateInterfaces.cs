using System;

namespace FRG.Core {
    public interface IState {
        void Enter();
        void Refresh(float delta);
        //void FixedRefresh(float delta);
        void Exit();
        bool Finished { get; }
        bool Cancelled { get; }
        int Choice { get; }
    }
}
