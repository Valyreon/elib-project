using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MVVMLibrary
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public void Set<T>(Expression<Func<T>> propertyExpression, ref T field, T value)
        {
            field = value;
            RaisePropertyChanged(GetName(propertyExpression));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            RaisePropertyChanged(GetName(propertyExpression));
        }

        private static string GetName<T>(Expression<Func<T>> exp)
        {
            if (!(exp.Body is MemberExpression body))
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}