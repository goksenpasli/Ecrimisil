using System;
using System.ComponentModel;

namespace Ecrimisil
{
    public class EcrimisilVeri : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Aralık { get; set; }

        public decimal BirimFiyat { get; set; }

        public decimal EcrimisilTutarı { get; set; }

        public decimal EcrimisilYüzölçümü { get; set; }

        public int Gün { get; set; }

        public int Yıl { get; set; }
    }
}
