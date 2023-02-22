using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statisitsche_Temp_Erfassung.Model
{
    class DayTree
    {
        public string Name { get; set; }
        public ObservableCollection<HourTree> Hours { get; set; }

        public DayTree()
        {
            Hours = new ObservableCollection<HourTree>();
        }
    }
}
