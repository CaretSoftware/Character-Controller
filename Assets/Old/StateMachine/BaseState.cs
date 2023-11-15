
namespace OldController {
    public abstract class BaseState : State {
        
        public OldCharacterController owner;
        //private OldCharacterController _character;

        protected OldCharacterController Character => owner;
    }
}