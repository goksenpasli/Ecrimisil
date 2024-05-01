using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Ecrimisil
{

    [XmlRoot(ElementName = "Ydo")]

    public class Ydo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlAttribute(AttributeName = "Oran")]

        public decimal Oran { get; set; }

        [XmlAttribute(AttributeName = "Yıl")]

        public int Yıl { get; set; }
    }
}
