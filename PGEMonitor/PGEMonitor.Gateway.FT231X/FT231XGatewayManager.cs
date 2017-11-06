using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace PGEMonitor.Gateway.FT231X
{
    public class FT231XGatewayManager : IGatewayManager
    {
        const string GATEWAY_NAME = "FT231X USB UART";

        DataReader _reader = null;
        List<int> _boardIds = new List<int>();
        int? _selected = null;
        int? _default = null;

        bool TryAddBoard(int id)
        {
            if (!_boardIds.Contains(id))
            {
                _boardIds.Add(id);

                // IF: Default board is set.
                if (_default.HasValue)
                {
                    // IF: This is default board.
                    if (id == _default)
                    {
                        _selected = _default;
                    }
                }
                else // ELSE: No default board, use first one that is added.
                {
                    _selected = id;
                }

                return true;
            }

            return false;
        }

        public IEnumerable<int> GetBoards()
        {
            return _boardIds;
        }

        bool TryRemoveBoard(int id)
        {
            if (_boardIds.Contains(id))
            {
                _boardIds.RemoveAll(x => x == id);

                // IF: This is selected board.
                if (_selected == id)
                {
                    // IF: Default board is defined.
                    if (_default.HasValue)
                    {
                        _selected = _default;
                    }
                    else // ELSE: No default board, so just fallback to first one on the list.
                    {
                        _selected = _boardIds.FirstOrDefault();
                    }
                }

                return true;
            }

            return false;
        }

        public int? GetSelectedBoard()
        {
            return _selected;
        }

        public bool TrySelectBoard(int id)
        {
            if (_boardIds.Contains(id))
            {
                _selected = id;
                return true;
            }

            return false;
        }

        public void SetDefaultBoard(int? id)
        {
            _default = id;
        }

        public async Task InitAsync()
        {
            /* Create selector to select all devices with capability to do serial communication. */
            string selector = SerialDevice.GetDeviceSelector();

            /* Query device registry with created selector. */
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            /* Find device that we need by its name. */
            DeviceInformation device = devices
                .Where(x => x.Name == GATEWAY_NAME)
                .FirstOrDefault();

            /* Create wrapper around the port based on device id. */
            SerialDevice serialDevice = await SerialDevice.FromIdAsync(device.Id);

            serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);

            serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(250);

            serialDevice.BaudRate = 115200;

            serialDevice.Parity = SerialParity.None;

            serialDevice.StopBits = SerialStopBitCount.One;

            serialDevice.DataBits = 7;

            serialDevice.Handshake = SerialHandshake.None;

            serialDevice.IsRequestToSendEnabled = true;

            _reader = new DataReader(serialDevice.InputStream);

            _reader.InputStreamOptions = InputStreamOptions.Partial;
        }

        IEnumerable<BoardReading> ProcessMessage(string message)
        {
            List<BoardReading> readings = new List<BoardReading>();

            string[] lines = message.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Reverse()) // Reverse is to give priority to latest value for each sensor.
            {
                Regex iRegex = new Regex(@"i:\d+");
                Match iMatch = iRegex.Match(line);

                Regex hRegex = new Regex(@"h:\d+(\.\d{1,2})?");
                Match hMatch = hRegex.Match(line);

                Regex tRegex = new Regex(@"t:\d+(\.\d{1,2})?");
                Match tMatch = tRegex.Match(line);

                // IF: Expected values are found in the line.
                if (iMatch.Success && hMatch.Success && tMatch.Success)
                {
                    BoardReading reading = new BoardReading
                    {
                        Id = Convert.ToInt32(iMatch.Value.Replace("i:", "")),
                        Humidity = Convert.ToDouble(hMatch.Value.Replace("h:", "")),
                        Temperature = Convert.ToDouble(tMatch.Value.Replace("t:", ""))
                    };

                    // IF: No previous reading is stored for this sensor.
                    if (readings.All(x => x.Id != reading.Id))
                    {
                        // Store the value.
                        readings.Add(reading);
                    }

                    // Try to add sensor id to the list of discovered sensors.
                    TryAddBoard(reading.Id);
                }
            }

            return readings;
        }

        public async Task<IEnumerable<BoardReading>> GetBoardReadingsAsync()
        {
            UInt32 bytesRead = await _reader.LoadAsync(1024);
            Debug.WriteLine(bytesRead);

            // IF: There are some bytes being read.
            if (bytesRead > 0)
            {
                string msg = _reader.ReadString(bytesRead);

                // IF: Read bytes are not just array of nulls.
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    return ProcessMessage(msg);
                }
            }

            return Enumerable.Empty<BoardReading>();
        }
    }
}
