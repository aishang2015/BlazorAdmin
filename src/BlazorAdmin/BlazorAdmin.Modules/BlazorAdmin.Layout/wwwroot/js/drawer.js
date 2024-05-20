export function handleDrawerToggled(isOpen) {
    const layoutElement = document.querySelector(".mud-layout");
    if (isOpen) {
        layoutElement.classList.replace("mud-drawer-close-responsive-md-left", "mud-drawer-open-responsive-md-left");
    } else {
        layoutElement.classList.replace("mud-drawer-open-responsive-md-left", "mud-drawer-close-responsive-md-left");
    }
}