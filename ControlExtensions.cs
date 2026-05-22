// ============================================================
// ControlExtensions.cs — Shared WinForms extension helpers
// Used across all Activity 6 forms for fluent UI wiring.
// ============================================================

using System;
using System.Windows.Forms;

namespace LibrarySystem.UI
{
    /// <summary>Fluent helper: Tap() lets you set properties inline without breaking method chains.</summary>
    public static class ControlExtensions
    {
        public static T Tap<T>(this T ctrl, Action<T> action) where T : Control
        {
            action(ctrl);
            return ctrl;
        }
    }
}
