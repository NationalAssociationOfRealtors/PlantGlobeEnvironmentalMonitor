using Autofac;
using PGEMonitor.Api;
using PGEMonitor.Core;
using PGEMonitor.Gateway;
using PGEMonitor.Lifx;
using PGEMonitor.UWP.Calculators;
using PGEMonitor.UWP.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PGEMonitor.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            StartAsync();
        }

        public async void StartAsync()
        {
            /* Values gathered from previous reading cycle. */
            double? prevHumidity = null;
            double? previousTemperature = null;
            int? previousSensorId = null;

            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                ILifxManager lifx = scope.Resolve<ILifxManager>();
                IDewPointCalculator dewPointCalc = scope.Resolve<IDewPointCalculator>();
                IColorCalculator colorCalc = scope.Resolve<IColorCalculator>();
                IGatewayManager gateway = scope.Resolve<IGatewayManager>();
                IConfigurationProvider config = scope.Resolve<IConfigurationProvider>();
                IHttpServer http = scope.Resolve<IHttpServer>();

                await http.ListenAsync(8800, "api");

                gateway.SetDefaultBoard(config.DefaultSensorBoardId);

                try
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                        { "OAUTH2_TOKEN", config.LifxHttpApiOAuth2Token },
                        { "SELECTED_DEVICE", config.DefaultLifxDeviceLabel }
                    };

                    await lifx.InitAsync(parameters);

                    await gateway.InitAsync();

                    while (true)
                    {
                        try
                        {

                            IEnumerable<BoardReading> readings = await gateway.GetBoardReadingsAsync();

                            DataTextBlock.Text = readings.Count().ToString();

                            SensorsTextBlock.Text = string.Join(", ", gateway.GetBoards());

                            int? boardId = gateway.GetSelectedBoard();

                            BoardReading reading = readings
                                .Where(x => x.Id == boardId)
                                .FirstOrDefault();

                            // IF: There is a reading for the selected sensor.
                            if (reading != null)
                            {
                                double dewPoint = dewPointCalc.Calculate(reading.Humidity.Value, reading.Temperature.Value);

                                HumidityTextBlock.Text = reading.Humidity.ToString();
                                TemperatureTextBlock.Text = reading.Temperature.ToString();
                                DewPointTextBlock.Text = dewPoint.ToString();
                                SensorIdTextBlock.Text = boardId.ToString();

                                /* IF: Values have been changed or sensor has been changed. */
                                if (reading.Humidity != prevHumidity || reading.Temperature != previousTemperature || previousSensorId != boardId)
                                {
                                    Color color = colorCalc.CalculateDewPointColor(dewPoint);

                                    await lifx.SetSelectedDeviceRgbColorAsync(color);

                                    IEnumerable<LifxDevice> lifxDevices = await lifx.GetAllDevicesAsync(false);
                                    if (lifxDevices != null)
                                    {
                                        BulbsTextBlock.Text = string.Join(", ", lifxDevices.Select(x => x.Label));
                                    }

                                    LifxDevice selected = await lifx.GetSelectedDeviceAsync();
                                    if (selected != null)
                                    {
                                        SelectedTextBlock.Text = selected.Label;
                                    }

                                    RedTextBlock.Text = color.R.ToString();
                                    GreenTextBlock.Text = color.G.ToString();
                                    BlueTextBlock.Text = color.B.ToString();

                                    HumidityRectangle.Fill = new SolidColorBrush(color);

                                    prevHumidity = reading.Humidity;
                                    previousTemperature = reading.Temperature;
                                    previousSensorId = boardId;
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            ErrorTextBlock.Text = e.Message;
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorTextBlock.Text = e.Message;
                }
            }
        }
    }
}
