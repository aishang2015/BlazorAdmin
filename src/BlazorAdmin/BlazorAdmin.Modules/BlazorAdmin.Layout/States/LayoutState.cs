using static BlazorAdmin.Layout.Components.NavMenus.NavItemMenu;

namespace BlazorAdmin.Layout.States
{
    public class LayoutState
    {
        private bool _navIsOpen = true;

        public event Action? NavIsOpenEvent;

        public bool NavIsOpen
        {
            get => _navIsOpen;
            set
            {
                _navIsOpen = value;
                NavIsOpenEvent?.Invoke();
            }
        }


        public event Action<NavMenuItem>? NavToEvent;

        public void NavTo(NavMenuItem item) => NavToEvent?.Invoke(item);
    }
}
