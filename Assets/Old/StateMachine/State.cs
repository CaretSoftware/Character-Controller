
namespace OldController {
    public abstract class State {

        public StateMachine stateMachine;

        public abstract void Enter();

        public abstract void Update();

        public abstract void Exit();
    }
}