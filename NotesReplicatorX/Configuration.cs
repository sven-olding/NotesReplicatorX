using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NotesReplicatorX
{
    public class Configuration : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private readonly Dictionary<string, List<string>> errors;

        private string name;
        private string sourceServer;
        private string sourceDirectory;
        private string targetServer;

        public Configuration()
        {
            errors = new Dictionary<string, List<string>>();
        }

        [Required(ErrorMessage = "Name cannot be empty")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
                Validate(value);
            }
        }

        [Required(ErrorMessage = "Source server cannot be empty")]
        public string SourceServer
        {
            get => sourceServer;
            set
            {
                sourceServer = value;
                OnPropertyChanged();
                Validate(value);
            }
        }


        [Required(ErrorMessage = "Source directory cannot be empty")]
        public string SourceDirectory
        {
            get
            {
                return sourceDirectory;
            }
            set
            {
                sourceDirectory = value;
                OnPropertyChanged();
                Validate(value);
            }
        }

        [Required(ErrorMessage = "Target server cannot be empty")]
        public string TargetServer
        {
            get
            {
                return targetServer;
            }
            set
            {
                targetServer = value;
                OnPropertyChanged();
                Validate(value);
            }
        }


        private void Validate(object value, [CallerMemberName] string propertyName = null)
        {
            ClearErrors(propertyName);
            ValidationContext validationContext = new ValidationContext(this) { MemberName = propertyName };
            List<ValidationResult> results = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, validationContext, results))
            {
                errors[propertyName] = results.Select(x => x.ErrorMessage).ToList();
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }


        private void ClearErrors(string propertyName)
        {
            if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }


        public void AddError(string propertyName, string error, bool isWarning)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }

            if (!errors[propertyName].Contains(error))
            {
                if (isWarning)
                {
                    errors[propertyName].Add(error);
                }
                else
                {
                    errors[propertyName].Insert(0, error);
                }
                OnErrorsChanged(propertyName);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName) &&
                errors[propertyName].Contains(error))
            {
                errors[propertyName].Remove(error);
                if (errors[propertyName].Count == 0)
                {
                    errors.Remove(propertyName);
                }
                OnErrorsChanged(propertyName);
            }
        }

        public bool HasErrors => errors.Any();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return errors.ContainsKey(propertyName) ? errors[propertyName] : null;
        }
    }
}
