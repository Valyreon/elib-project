using Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ElibWpf.ValidationAttributes
{
    public class NotEmpty : ValidationAttribute
    {

        public NotEmpty()
        {
        }

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
