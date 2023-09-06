using Valyreon.Elib.DataLayer.Filters;

namespace Valyreon.Elib.Wpf.Models
{
    public record ViewerFilters
    {
        public BookFilter BookFilter { get; set; }
        public Filter OthersFilter { get; set; }
    }
}
