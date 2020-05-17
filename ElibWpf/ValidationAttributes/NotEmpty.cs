using System.ComponentModel.DataAnnotations;

namespace ElibWpf.ValidationAttributes
{
    public class NotEmpty : ValidationAttribute
    {
        // TODO: try to implement this
        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            return true;
        }
    }
}