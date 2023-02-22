using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statisitsche_Temp_Erfassung.Model
{
    class WeekTree
    {
        public string Name { get; set; }
        public ObservableCollection<DayTree> Days { get; set; }

        public WeekTree()
        {
            Days = new ObservableCollection<DayTree>();
        }
    }
}
