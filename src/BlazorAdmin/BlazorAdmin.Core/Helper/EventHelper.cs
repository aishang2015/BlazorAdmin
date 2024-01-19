namespace BlazorAdmin.Core.Helper
{
    public class EventHelper<TModel>
    {
        public TModel? Data { get; private set; }

        public event Func<TModel, Task>? OnChange;

        public void NotifyStateChanged(TModel model)
        {
            Data = model;
            OnChange?.Invoke(model);
        }
    }
}
