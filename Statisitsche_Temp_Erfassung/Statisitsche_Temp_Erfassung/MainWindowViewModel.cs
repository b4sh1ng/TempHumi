using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.ComponentModel;
using System.Windows.Threading;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Drawing;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace Statisitsche_Temp_Erfassung
{
    [ObservableObject]
    public partial class MainWindowViewModel
    {
        private SensorDaten sensor = new();

        private readonly ObservableCollection<ObservableValue> humidityValues = new ObservableCollection<ObservableValue>();
        private readonly ObservableCollection<ObservableValue> temperatureValues = new ObservableCollection<ObservableValue>();

        private readonly ObservableValue currentHumidity;
        private readonly ObservableValue currentTemperature;

        private string? zeit;
        public ObservableCollection<ISeries> Series { get; set; }
        public IEnumerable<ISeries> CurrentHumiditySeries { get; set; }
        public IEnumerable<ISeries> CurrentTemperatureSeries { get; set; }

        public string Zeit
        {
            get { return zeit!; }
            set
            {
                SetProperty(ref zeit, value);
            }
        }
        DispatcherTimer LiveTime = new();

        public MainWindowViewModel()
        {
            var xAxis = new Axis
            {
                MaxLimit = 0,
                MinLimit = 96
            };

            ;
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservableValue>
                {
                    Values = humidityValues,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness= 1,

                },
                new LineSeries<ObservableValue>
                {
                    Values = temperatureValues,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness= 1
                }

            };

            currentHumidity = new ObservableValue { Value = 0 };
            currentTemperature = new ObservableValue { Value = 0 };

            CurrentHumiditySeries = new GaugeBuilder()
                .WithOffsetRadius(5)
                .WithLabelsPosition(LiveChartsCore.Measure.PolarLabelsPosition.Start)
                .AddValue(currentHumidity, "Luftfeuchte")
                .BuildSeries();

            CurrentTemperatureSeries = new GaugeBuilder()
                .WithOffsetRadius(5)
                .WithLabelsPosition(LiveChartsCore.Measure.PolarLabelsPosition.Start)
                .AddValue(currentTemperature, "Temperatur", SKColors.Red)
                .BuildSeries();

            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
            AddTempSeries();
        }
        void timer_Tick(object? sender, EventArgs e)
        {
            Zeit = DateTime.Now.ToString("HH:mm:ss");
            AddTempSeriesLast();
        }

        public void AddTempSeries()
        {
            double tempTemperaturwert, tempHumiditywert;
            List<SensorDaten> daten = sensor.AktuellsteDatenAuslesen();
            foreach (var item in daten)
            {
                tempTemperaturwert = Math.Round(item.Temperature, 1);
                tempHumiditywert = Math.Round(item.Humidity, 1);
                temperatureValues.Add(new(tempTemperaturwert));
                humidityValues.Add(new(tempHumiditywert));
            }
        }
        public void AddTempSeriesLast()
        {
            double tempTemperaturwert, tempHumiditywert;
            List<SensorDaten> daten = sensor.AktuellsteDatenAuslesen();
            List<double> TempValues = new List<double>();
            List<double> HumValues = new List<double>();
            foreach (var item in daten)
            {
                tempTemperaturwert = Math.Round(item.Temperature, 1);
                tempHumiditywert = Math.Round(item.Humidity, 1);
                TempValues.Add(tempTemperaturwert);
                HumValues.Add(tempHumiditywert);
            }
            temperatureValues.Add(new(TempValues.Last()));
            currentTemperature.Value = TempValues.Last();
            currentHumidity.Value = HumValues.Last();
        }
    }
}
