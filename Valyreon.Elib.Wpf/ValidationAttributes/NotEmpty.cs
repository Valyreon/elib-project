using System.ComponentModel.DataAnnotations;

namespace Valyreon.Elib.Wpf.ValidationAttributes
{
    public class NotEmpty : ValidationAttribute
    {
        // TODO: try to implement this
        public override bool IsValid(object value)
        {
            return value is null || true;
        }
    }
}
