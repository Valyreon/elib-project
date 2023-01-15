using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Valyreon.Elib.Domain
{
    public class ObservableEntity : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public void Set<T>(Expression<Func<T>> propertyExpression, ref T field, T value)
        {
            field = value;
            RaisePropertyChanged(GetName(propertyExpression));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            RaisePropertyChanged(GetName(propertyExpression));
        }

        private static string GetName<T>(Expression<Func<T>> exp)
        {
            if (exp.Body is not MemberExpression body)
            {
                var ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}
