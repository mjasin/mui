using System.ComponentModel;
using ModernUI.Presentation;

namespace ModernUI.App
{
    public class SampleFormViewModel
        : NotifyPropertyChanged, IDataErrorInfo
    {
        string firstName = "John";
        string lastName;

        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName != value)
                {
                    firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }

        public string LastName
        {
            get => lastName;
            set
            {
                if (lastName != value)
                {
                    lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == "FirstName")
                {
                    return string.IsNullOrEmpty(firstName) ? "Required value" : null;
                }
                if (columnName == "LastName")
                {
                    return string.IsNullOrEmpty(lastName) ? "Required value" : null;
                }
                return null;
            }
        }
    }
}