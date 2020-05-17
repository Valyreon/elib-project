using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;

namespace ElibWpf.ViewModels
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
                lock (this.lockObject)
                {
                    return this.errors.Any(propErrors => propErrors.Value != null && propErrors.Value.Count > 0);
                }
            }
        }

        public IEnumerable GetErrors(string propertyName = null)
        {
            lock (this.lockObject)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    return this.errors.SelectMany(err => err.Value.ToList());
                }

                if (this.errors.ContainsKey(propertyName) && this.errors[propertyName] != null && this.errors[propertyName].Count > 0)
                {
                    return this.errors[propertyName].ToList();
                }
            }

            return null;

        }

        public void ClearErrors()
        {
            lock (this.lockObject)
            {
                this.errors.Clear();
            }
        }

        public void OnErrorsChanged(string propertyName)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate()
        {
            lock (this.lockObject)
            {
                ValidationContext validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                //clear all previous _errors
                var propNames = this.errors.Keys.ToList();
                this.errors.Clear();
                propNames.ForEach(pn => this.OnErrorsChanged(pn));
                this.HandleValidationResults(validationResults);
            }
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            lock (this.lockObject)
            {
                ValidationContext validationContext = new ValidationContext(this, null, null)
                {
                    MemberName = propertyName
                };
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                //clear previous _errors from tested property
                if (this.errors.ContainsKey(propertyName))
                {
                    this.errors.Remove(propertyName);
                }

                this.OnErrorsChanged(propertyName);
                this.HandleValidationResults(validationResults);
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
                this.errors.Add(prop.Key, messages);
                this.OnErrorsChanged(prop.Key);
            }
        }
    }
}