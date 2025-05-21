﻿using MudBlazor;

namespace BlazorAdmin.Servers.Core.Components.Pages
{
    public class PageDataGridConfig
    {
        public static bool Dense { get; private set; } = true;

        public static bool Filterable { get; private set; } = false;

        public static ResizeMode ColumnResizeMode { get; private set; } = ResizeMode.Column;

        public static SortMode SortMode { get; private set; } = SortMode.None;

        public static bool Groupable { get; private set; } = false;

        public static bool Virtualize { get; private set; } = true;

        public static bool FixedHeader { get; private set; } = true;

        public static int Elevation { get; private set; } = 1;

        public static bool Outlined { get; private set; } = false;

        public static string Style { get; private set; } = "flex: 1;";

        public static bool HorizontalScrollbar = true;
    }
}
