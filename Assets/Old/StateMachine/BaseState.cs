
namespace OldController {
    public abstract class BaseState : State {
        public OldCharacterController owner;

        protected OldCharacterController Character => owner;
    }
}