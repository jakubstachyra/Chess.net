using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logic.Services
{
    public class StockfishEngine : IDisposable
    {
        private readonly Process _process;
        private readonly StringBuilder _output = new StringBuilder();
        private readonly SemaphoreSlim _outputSemaphore = new SemaphoreSlim(1, 1);

        // sygnalizuje, że Stockfish zwrócił "uciok"
        private readonly TaskCompletionSource<bool> _readyTcs = new TaskCompletionSource<bool>();

        public StockfishEngine(string stockfishPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = stockfishPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _process = new Process { StartInfo = startInfo };

            // asynchroniczny odbiór linii z wyjścia Stockfisha
            _process.OutputDataReceived += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    await _outputSemaphore.WaitAsync();
                    try
                    {
                        _output.AppendLine(e.Data);
                        // Gdy nadejdzie "uciok", oznacza to, że silnik jest gotowy
                        if (e.Data.Contains("uciok"))
                            _readyTcs.TrySetResult(true);
                    }
                    finally
                    {
                        _outputSemaphore.Release();
                    }
                }
            };

            _process.Start();
            // startuje asynchroniczny odczyt
            _process.BeginOutputReadLine();

            // wysyłamy Stockfishowi polecenie "uci", po którym odpowie "uciok"
            SendCommand("uci");
        }

        private void SendCommand(string command)
        {
            _process.StandardInput.WriteLine(command);
        }

        /// <summary>
        /// Pobiera najlepszy ruch z aktualnej pozycji (FEN) do zadanej głębokości.
        /// </summary>
        public async Task<string> GetBestMoveAsync(string fen, int depth = 15)
        {
            // Czekamy, aż Stockfish zakończy procedurę "uci" i zwróci "uciok"
            await _readyTcs.Task;

            // Czyścimy bufor, aby nie mieszać starych informacji
            await _outputSemaphore.WaitAsync();
            try
            {
                _output.Clear();
            }
            finally
            {
                _outputSemaphore.Release();
            }

            // Ustawiamy aktualną pozycję wg FEN
            SendCommand($"position fen {fen}");
            // prosimy Stockfish o ruch na zadaną głębokość
            SendCommand($"go depth {depth}");

            // czekamy, aż w buforze pojawi się "bestmove ..."
            while (true)
            {
                // niewielkie opóźnienie na zebranie outputu
                await Task.Delay(50);

                string[] lines;
                await _outputSemaphore.WaitAsync();
                try
                {
                    lines = _output
                        .ToString()
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                finally
                {
                    _outputSemaphore.Release();
                }

                foreach (var line in lines)
                {
                    if (line.StartsWith("bestmove"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                            return parts[1]; // np. "e2e4"
                        else
                            return null;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_process != null && !_process.HasExited)
            {
                SendCommand("quit");
                _process.WaitForExit();
                _process.Dispose();
            }
        }
    }
}
