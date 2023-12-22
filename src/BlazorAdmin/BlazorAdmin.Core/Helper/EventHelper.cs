using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
