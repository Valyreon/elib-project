using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Valyreon.Elib.Wpf.ValidationAttributes
{
    public class NotEmpty : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is IList list && list.Count > 0;
        }
    }
}
