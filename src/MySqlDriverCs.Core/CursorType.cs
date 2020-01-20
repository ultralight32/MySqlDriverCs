using System.Diagnostics.CodeAnalysis;

namespace MySQLDriverCS
{
    /// <summary>
    /// Cursor types
    /// </summary>
    /// <remarks>
    /// enum enum_cursor_type
    /// {
    /// CURSOR_TYPE_NO_CURSOR= 0,
    /// CURSOR_TYPE_READ_ONLY= 1,
    /// CURSOR_TYPE_FOR_UPDATE= 2,
    /// CURSOR_TYPE_SCROLLABLE= 4
    /// };
    /// </remarks>
    public enum CursorType:int
    {
        NoCursor = 0, ReadOnly = 1, ForUpdate=2, Scrollable=4
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal enum enum_cursor_type:uint
    {
        CURSOR_TYPE_NO_CURSOR = 0,
        CURSOR_TYPE_READ_ONLY = 1,
        CURSOR_TYPE_FOR_UPDATE = 2,
        CURSOR_TYPE_SCROLLABLE = 4
    };
}