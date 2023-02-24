using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statisitsche_Temp_Erfassung
{
    public class MenuItem
    {
        public string? Title { get; set; }
        public ObservableCollection<MenuItem> Items { get; set; }

        public MenuItem()
        {
            Items = new ObservableCollection<MenuItem>();
        }
    }


}
