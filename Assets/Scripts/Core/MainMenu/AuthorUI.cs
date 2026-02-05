using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.MainMenu.UI
{

    public class AuthorUI : MonoBehaviour
    {
        [SerializeField] private Button _back;
        private IMainMenu _mainMenu;

        public void Init(IMainMenu mainMenu)
        {
            _mainMenu = mainMenu;

            if (_mainMenu != null) _back.onClick.AddListener(_mainMenu.Show);
        }

        private void OnDestroy()
        {
            _back.onClick.RemoveAllListeners();
        }
    }

}