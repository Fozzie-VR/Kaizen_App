using UnityEngine.UIElements;


namespace KaizenApp
{
    public class MainMenuView: IView
    {
        //receives input from the user and sends it to the controller via events
        //receives events from the controller and updates the view accordingly

        private const string MAKE_LAYOUT_BUTTON = "btn_pre_kaizen_layout";
        
        //event strings
        public const string PRE_KAIZEN_LAYOUT_CLICKED = "PreKaizenLayoutClicked";

        private VisualElement _menuRoot;
        
        private Button _makeLayoutButton;

        //constructor that takes in the root element of the UI
        public MainMenuView(VisualElement root)
        {
            _menuRoot = root;
            _makeLayoutButton = _menuRoot.Q<Button>(MAKE_LAYOUT_BUTTON);
            SetupButtonCallbacks();
        }

        public void SetupButtonCallbacks()
        {
            _makeLayoutButton.clicked += OnMakeLayoutClicked;
        }

        private void OnMakeLayoutClicked()
        {
            EventManager.TriggerEvent(PRE_KAIZEN_LAYOUT_CLICKED, null);
        }
    }
}
