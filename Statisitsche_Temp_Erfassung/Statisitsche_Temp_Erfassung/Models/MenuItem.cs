namespace Statisitsche_Temp_Erfassung.Models
{
    public class MenuItem
    {
        public string? Title { get; set; }
        public ObservableCollection<MenuItem> Items { get; set; }
        public bool IsSelected { get; set; }

        public MenuItem()
        {
            Items = new ObservableCollection<MenuItem>();
        }
    }
}
