using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public abstract class ViewModelWithValidation : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        private readonly object lockObject = new object();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors
        {
            get
            {
                lock (lockObject)
                {
                    return errors.Any(propErrors => propErrors.Value?.Count > 0);
                }
            }
        }

        public IEnumerable GetErrors(string propertyName = null)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    return errors.SelectMany(err => err.Value.ToList());
                }

                if (errors.ContainsKey(propertyName) && errors[propertyName]?.Count > 0)
                {
                    return errors[propertyName].ToList();
                }
            }

            return null;
        }

        public void ClearErrors()
        {
            lock (lockObject)
            {
                errors.Clear();
            }
        }

        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate()
        {
            lock (lockObject)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                //clear all previous _errors
                var propNames = errors.Keys.ToList();
                errors.Clear();
                propNames.ForEach(pn => OnErrorsChanged(pn));
                HandleValidationResults(validationResults);
            }
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            lock (lockObject)
            {
                var validationContext = new ValidationContext(this, null, null)
                {
                    MemberName = propertyName
                };
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                //clear previous _errors from tested property
                if (errors.ContainsKey(propertyName))
                {
                    errors.Remove(propertyName);
                }

                OnErrorsChanged(propertyName);
                HandleValidationResults(validationResults);
            }
        }

        private void HandleValidationResults(IEnumerable<ValidationResult> validationResults)
        {
            var resultsByPropNames = from res in validationResults
                                     from mname in res.MemberNames
                                     group res by mname
                                     into g
                                     select g;
            foreach (var prop in resultsByPropNames)
            {
                var messages = prop.Select(r => r.ErrorMessage).ToList();
                errors.Add(prop.Key, messages);
                OnErrorsChanged(prop.Key);
            }
        }
    }
}
