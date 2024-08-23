using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Component.Pages
{
    public class PageDataGridOne
    {
        public bool HasPagination { get; private set; } = true;

        public PageDataGridOne(bool HasPagination = true)
        {
            this.HasPagination = HasPagination;
        }


        public bool Dense { get; private set; } = true;

        public bool Filterable { get; private set; } = false;

        public ResizeMode ColumnResizeMode { get; private set; } = ResizeMode.Column;

        public SortMode SortMode { get; private set; } = SortMode.None;

        public bool Groupable { get; private set; } = false;

        public bool Virtualize { get; private set; } = true;

        public bool FixedHeader { get; private set; } = true;

        public string Height { get => HasPagination ? "calc(100vh - 225px)" : "calc(100vh - 175px)"; }

        public int Elevation { get; private set; } = 0;

        public bool Outlined { get; private set; } = true;


    }
}
