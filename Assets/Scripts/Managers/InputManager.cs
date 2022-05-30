namespace GameJam
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        private PlayerInputActions playerActions;
        public PlayerInputActions PlayerActions => playerActions;

        private void OnEnable() => PlayerActions.Enable();
        private void OnDisable() => PlayerActions.Disable();
        protected override void OnDestroy()
        {
            base.OnDestroy();
            playerActions = null;
        }

        protected override void Awake()
        {
            base.Awake();

            playerActions = new PlayerInputActions();
            playerActions.Enable();
        }

        //TODO if any window panel open
        //private void Update()
        //{
        //    if (!Utils.IsCursorOverUserInterface())
        //    {
        //        PlayerActions.Player.Enable();
        //        PlayerActions.UI.Disable();
        //    }
        //    else
        //    {
        //        PlayerActions.UI.Enable();
        //        PlayerActions.Player.Disable();
        //    }
        //}
    }
}