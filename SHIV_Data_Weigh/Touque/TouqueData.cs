using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHIV_Data_Weigh
{
    public class TouqueData : ObservableObject
    {
        private string header;

        public string Header
        {
            get { return header; }
            set
            {
                header = value;
                OnPropertyChanged("Header");
            }
        }

        private float myValue;

        public float Value
        {
            get { return myValue; }
            set
            {
                myValue = value;
                OnPropertyChanged("Value");
            }
        }


        public TouqueData(string headerInput, float valueInput)
        {
            Header = headerInput;
            Value = valueInput;
        }
    }
}
