namespace Statisitsche_Temp_Erfassung
{
    //[ObservableObject]
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<ICartesianAxis> XAxes { get; set; }
        public List<Axis> YAxes { get; set; }
        public IEnumerable<ISeries> CurrentHumiditySeries { get; set; }
        public IEnumerable<ISeries> CurrentTemperatureSeries { get; set; }
        private readonly ObservableCollection<DateTimePoint> humidityValues = new ObservableCollection<DateTimePoint>();
        private readonly ObservableCollection<DateTimePoint> temperatureValues = new ObservableCollection<DateTimePoint>();
        private readonly ObservableValue currentHumidity = new ObservableValue { Value = 0 };
        private readonly ObservableValue currentTemperature = new ObservableValue { Value = 0 };
        public ObservableCollection<MenuItem> Menus { get; set; } = new ObservableCollection<MenuItem>();
        public RelayCommand LoadData { get; set; }
        public RelayCommand RequestData { get; set; }
        public RelayCommand CurrentData { get; set; }
        public List<List<List<List<SensorDaten>>>> measures { get; set; }
        public List<List<List<List<SensorDaten>>>> measuresClean { get; set; }
        private double durchschnittTemp;
        public double DurchschnittTemp
        {
            get { return durchschnittTemp; }
            set
            {
                SetProperty(ref durchschnittTemp, value);
            }
        }
        private double maxTemp;
        public double MaxTemp
        {
            get { return maxTemp; }
            set
            {
                SetProperty(ref maxTemp, value);
            }
        }
        private double minTemp;
        public double MinTemp
        {
            get { return minTemp; }
            set
            {
                SetProperty(ref minTemp, value);
            }
        }
        private double durchschnittHumi;
        public double DurchschnittHumi
        {
            get { return durchschnittHumi; }
            set
            {
                SetProperty(ref durchschnittHumi, value);
            }
        }
        private double maxHumi;
        public double MaxHumi
        {
            get { return maxHumi; }
            set
            {
                SetProperty(ref maxHumi, value);
            }
        }
        private double minHumi;
        public double MinHumi
        {
            get { return minHumi; }
            set
            {
                SetProperty(ref minHumi, value);
            }
        }
        private string? lastAction;
        public string LastAction
        {
            get { return lastAction!; }
            set
            {
                SetProperty(ref lastAction, value);
            }
        }
        private string? zeit;
        public string Zeit
        {
            get { return zeit!; }
            set
            {
                SetProperty(ref zeit, value);
            }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
            }
        }
        private TreeView? _trvMenu;
        public TreeView trvMenu
        {
            get { return _trvMenu; }
            set { SetProperty(ref _trvMenu, value); }
        }

        SensorDaten sensor = new();
        DispatcherTimer LiveTime = new();
        SensorDaten sensorDaten = new();
        HttpRequestHandler req = new();

        public MainWindowViewModel()
        {
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();

            LoadData = new RelayCommand(LoadSelectedData);
            RequestData = new RelayCommand(RequestDataHttp);
            CurrentData = new RelayCommand(CurrentTempSeries);

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    TooltipLabelFormatter = ChartPoint => $"Feuchtigkeit: {ChartPoint.PrimaryValue}% vom: {new DateTime((long)ChartPoint.SecondaryValue):dd.MM HH:mm}",
                    Values = humidityValues,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness= 1
                },
                new LineSeries<DateTimePoint>
                {
                    TooltipLabelFormatter = ChartPoint => $"Temperatur: {ChartPoint.PrimaryValue}°C vom: {new DateTime((long)ChartPoint.SecondaryValue):dd.MM HH:mm}",
                    Values = temperatureValues,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness= 1,
                    ScalesYAt = 1
                }
            };
            XAxes = new ObservableCollection<ICartesianAxis>
            {
                new Axis
                {
                    LabelsRotation = -45,
                    Labeler = value => value > 0 ? new DateTime((long)value).ToString("dd.MM. HH") + "h" : "",
                    UnitWidth = TimeSpan.FromMinutes(15).Ticks,
                    MinStep = TimeSpan.FromHours(2).Ticks,
                }
            };
            YAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Luftfeuchte in %",
                    MinLimit= 0,
                    MaxLimit= 100,
                    Position = AxisPosition.End,
                },

                new Axis
                {
                    Name = "Temperatur in °C",
                    MinLimit= 0,
                    MaxLimit= 60,
                    Position = AxisPosition.Start,
                }
            };
            
            GetMenu();
            SetupPieCharts();
            CurrentTempSeries();
        }
        private void RequestDataHttp(object obj)
        {
            int httpResponseCode = 0;
            LastAction = "Http Anfrage läuft...";
            Task.Run(() =>
            {
                httpResponseCode = req.StartGetData().Result;

                if (httpResponseCode == 200)
                {
                    LastAction = "Http Anfrage erfolgreich!";
                }
                else if (httpResponseCode == 1)
                {
                    LastAction = "Http Anfrage fehlgeschlagen!";
                }
                else
                {
                    GetMenu();
                    LastAction = "Http Anfrage Erfolgreich! Akutellster Stand!";
                }
            });
        }
        public void LoadSelectedData(object obj)
        {
            if (sensor is not null)
            {

                LastAction = "Lade gewählte Daten...";
                bool selectedIn0 = false,
                     selectedIn1 = false,
                     selectedIn2 = false;

                int woche = 0;
                int tag;
                int stunde;
                int wocheSelected = 0;
                int tagSelected = 0;
                int stundeSelected = 0;
                foreach (var wochen in Menus)
                {
                    if (wochen.IsSelected == true)
                    {
                        selectedIn0 = true;
                        wocheSelected = woche;
                    }
                    tag = 0;
                    foreach (var tage in wochen.Items)
                    {
                        if (tage.IsSelected == true)
                        {
                            selectedIn1 = true;
                            wocheSelected = woche;
                            tagSelected = tag;
                        }
                        stunde = 0;
                        foreach (var stunden in tage.Items)
                        {
                            if (stunden.IsSelected == true)
                            {
                                selectedIn2 = true;
                                wocheSelected = woche;
                                tagSelected = tag;
                                stundeSelected = stunde;
                            }
                            stunde++;
                        }
                        tag++;
                    }
                    woche++;
                }
                if (selectedIn0)
                {
                    temperatureValues.Clear();
                    humidityValues.Clear();
                    foreach (var item in measuresClean[wocheSelected])
                    {
                        foreach (var item2 in item)
                        {
                            foreach (var item3 in item2)
                            {
                                temperatureValues.Add(new(item3.Date, item3.Temperature));
                                humidityValues.Add(new(item3.Date, item3.Humidity));
                            }
                        }
                    }

                }
                if (selectedIn1)
                {
                    temperatureValues.Clear();
                    humidityValues.Clear();
                    foreach (var item in measuresClean[wocheSelected][tagSelected])
                    {
                        foreach (var item2 in item)
                        {
                            temperatureValues.Add(new(item2.Date, item2.Temperature));
                            humidityValues.Add(new(item2.Date, item2.Humidity));
                        }
                    }
                }
                if (selectedIn2)
                {
                    temperatureValues.Clear();
                    humidityValues.Clear();
                    foreach (var item in measuresClean[wocheSelected][tagSelected][stundeSelected])
                    {
                        temperatureValues.Add(new(item.Date, item.Temperature));
                        humidityValues.Add(new(item.Date, item.Humidity));
                    }
                }
                if (Menus.Count > 0)
                {
                    LastAction = "Gewählte Daten geladen!";
                }
                else
                {
                    LastAction = "Keine Daten vorhanden!";
                }
                CurrentStatsBar();
            }
        }
        void timer_Tick(object? sender, EventArgs e)
        {
            Zeit = DateTime.Now.ToString("HH:mm:ss");
        }
        public void GetMenu()
        {
            measures = sensorDaten.GetMeasurementData();
            measuresClean = sensorDaten.RemoveEmptyLists(measures);
            int cw = 1;
            foreach (var week in measures)
            {
                MenuItem calendarweek = new();
                if (week.Count > 0)
                {
                    calendarweek = new MenuItem() { Title = $"KW {cw}" };
                }
                int weekday = 0;
                foreach (var days in week)
                {
                    EnumWeekdays day = (EnumWeekdays)weekday;
                    MenuItem calendarday = new();

                    if (days.Count > 0)
                    {
                        calendarday = new MenuItem() { Title = $"{day}" };
                        calendarweek.Items.Add(calendarday);
                    }
                    MenuItem hour = new();
                    int stunde = 0;
                    foreach (var hours in days)
                    {
                        if (hours.Count > 0)
                        {
                            hour = new MenuItem() { Title = $"{stunde}:00 bis {stunde + 1}:00" };
                            calendarday.Items.Add(hour);
                        }
                        stunde++;
                    }
                    weekday++;
                }
                if (calendarweek.Title != null)
                {
                    try
                    {
                        Menus.Add(calendarweek);
                    }
                    catch { }
                }
                cw++;
            }
        }
        public void CurrentTempSeries(object obj)
        {
            CurrentTempSeries();
        }
        public void CurrentTempSeries()
        {
            temperatureValues.Clear();
            humidityValues.Clear();
            try
            {
                List<SensorDaten> daten = sensor.AktuellsteDatenAuslesen();
                foreach (var item in daten)
                {
                    double tempTemperature = Math.Round(item.Temperature, 2);
                    double tempHumidity = Math.Round(item.Humidity, 2);

                    temperatureValues.Add(new(item.Date, tempTemperature));
                    humidityValues.Add(new(item.Date, tempHumidity));

                    currentTemperature.Value = tempTemperature;
                    currentHumidity.Value = tempHumidity;
                }
                CurrentStatsBar();
                LastAction = "Heutige Daten geladen!";
            }
            catch
            {
                LastAction = "Keine Logs vorhanden!";
            }

        }
        public void CurrentStatsBar()
        {
            if (Menus.Count > 0)
            {
                DurchschnittTemp = (double)temperatureValues.Select(t => t.Value).Average()!;
                MaxTemp = (double)temperatureValues.Select(t => t.Value).Max()!;
                MinTemp = (double)temperatureValues.Select(t => t.Value).Min()!;
                DurchschnittHumi = (double)humidityValues.Select(t => t.Value).Average()!;
                MaxHumi = (double)humidityValues.Select(t => t.Value).Max()!;
                MinHumi = (double)humidityValues.Select(t => t.Value).Min()!;
            }
        }
        public void SetupPieCharts()
        {
            CurrentHumiditySeries = new GaugeBuilder()
                    .WithOffsetRadius(5)
                    .WithLabelsPosition(PolarLabelsPosition.Start)
                    .AddValue(currentHumidity, "Aktuelle Feuchte")
                    .BuildSeries();

            CurrentTemperatureSeries = new GaugeBuilder()
                    .WithOffsetRadius(5)
                    .WithLabelsPosition(PolarLabelsPosition.Start)
                    .AddValue(currentTemperature, "Aktuelle Temperatur", SKColors.Red)
                    .BuildSeries();
        }
    }
}





