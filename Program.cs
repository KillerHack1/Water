using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdvancedWaterUtilitySystem
{
    public class EnhancedPumpMonitor
    {
        private List<PumpReading> _sensorData = new List<PumpReading>();
        private const string DataDirectory = "PumpData";
        private const string DataFilePattern = "pump_*.csv";
        
        public void LoadHistoricalData()
        {
            _sensorData.Clear(); // Очищаем предыдущие данные
            
            if (!Directory.Exists(DataDirectory))
            {
                Console.WriteLine($"Директория {DataDirectory} не найдена. Создаю новую...");
                Directory.CreateDirectory(DataDirectory);
                return;
            }

            try 
            {
                var files = Directory.GetFiles(DataDirectory, DataFilePattern);
                if (files.Length == 0)
                {
                    Console.WriteLine("Файлы данных не найдены");
                    return;
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"Обработка файла: {file}");
                    var lines = File.ReadAllLines(file);
                    
                    if (lines.Length <= 1) // Только заголовок или пустой файл
                    {
                        Console.WriteLine($"Файл {file} не содержит данных");
                        continue;
                    }

                    foreach (var line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        try
                        {
                            _sensorData.Add(ParsePumpData(line));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка обработки строки: {line}\n{ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"Успешно загружено {_sensorData.Count} записей");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка загрузки: {ex.Message}");
            }
        }

        private PumpReading ParsePumpData(string dataLine)
        {
            var values = dataLine.Split(';');
            if (values.Length != 5)
                throw new FormatException("Некорректный формат строки данных");

            return new PumpReading {
                MeasurementTime = DateTime.Parse(values[0].Trim()),
                Pressure = double.Parse(values[1].Trim()),
                Temperature = double.Parse(values[2].Trim()),
                VibrationLevel = double.Parse(values[3].Trim()),
                Status = (EquipmentStatus)Enum.Parse(typeof(EquipmentStatus), values[4].Trim())
            };
        }

        public void PerformAdvancedAnalysis()
        {
            if (!_sensorData.Any())
            {
                Console.WriteLine("Нет данных для анализа. Загрузите данные сначала.");
                return;
            }

            try
            {
                var latest = GetLatestReading();
                var stats = new PumpStatistics(_sensorData);

                Console.WriteLine("\n=== Расширенная аналитика ===");
                Console.WriteLine($"Последнее измерение: {latest.MeasurementTime}");
                Console.WriteLine($"Температура: {latest.Temperature}°C");
                Console.WriteLine($"Вибрация: {latest.VibrationLevel} dB");
                
                Console.WriteLine("\nСтатистика за 24 часа:");
                Console.WriteLine($"Макс. давление: {stats.MaxPressure} бар");
                Console.WriteLine($"Макс. температура: {stats.MaxTemperature}°C");
                Console.WriteLine($"Средняя температура: {stats.AvgTemperature:F1}°C");
                Console.WriteLine($"Аварийных событий: {stats.CriticalEventsCount}");

                GenerateMaintenanceRecommendation(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при анализе данных: {ex.Message}");
            }
        }

        private void GenerateMaintenanceRecommendation(PumpStatistics stats)
        {
            Console.WriteLine("\nРекомендации:");
            
            if (stats.AvgVibration > 4.5)
                Console.WriteLine("⚠ Требуется проверка подшипников насоса");
            
            if (stats.MaxTemperature > 85)
                Console.WriteLine("⚠ Охлаждение работает неэффективно");
            
            if (stats.CriticalEventsCount == 0)
                Console.WriteLine("✅ Все системы работают нормально");
            else
                Console.WriteLine($"⛔ Критических событий: {stats.CriticalEventsCount}");
        }

        public PumpReading GetLatestReading()
        {
            if (!_sensorData.Any())
                throw new InvalidOperationException("Нет данных для анализа");

            return _sensorData.OrderByDescending(r => r.MeasurementTime).First();
        }
    }

    public class PumpReading
    {
        public DateTime MeasurementTime { get; set; }
        public double Pressure { get; set; }
        public double Temperature { get; set; }
        public double VibrationLevel { get; set; }
        public EquipmentStatus Status { get; set; }
        
        public override string ToString() => 
            $"{MeasurementTime:T} - Давление: {Pressure} бар, Темп: {Temperature}°C";
    }

    public class PumpStatistics
    {
        public double MaxPressure { get; } = 0;
        public double MaxTemperature { get; } = 0;
        public double AvgTemperature { get; } = 0;
        public double AvgVibration { get; } = 0;
        public int CriticalEventsCount { get; } = 0;

        public PumpStatistics(IEnumerable<PumpReading> data)
        {
            var last24h = data?
                .Where(r => r.MeasurementTime > DateTime.Now.AddHours(-24))
                .ToList();

            if (last24h?.Any() != true)
                return;

            MaxPressure = last24h.Max(r => r.Pressure);
            MaxTemperature = last24h.Max(r => r.Temperature);
            AvgTemperature = last24h.Average(r => r.Temperature);
            AvgVibration = last24h.Average(r => r.VibrationLevel);
            CriticalEventsCount = last24h.Count(r => r.Status == EquipmentStatus.Critical);
        }
    }

    public enum EquipmentStatus 
    { 
        Normal, 
        Warning, 
        Critical,
        MaintenanceRequired 
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            var monitor = new EnhancedPumpMonitor();
            monitor.LoadHistoricalData();
            monitor.PerformAdvancedAnalysis();
            
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}