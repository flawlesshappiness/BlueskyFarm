public static class ScreenEffects
{
    private static ScreenEffectsView _view;
    private static ScreenEffectsView Instance => _view ?? (_view = View.Get<ScreenEffectsView>());
}
