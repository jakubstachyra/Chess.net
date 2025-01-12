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

        // TaskCompletionSource sygnalizuje, że Stockfish zwrócił "uciok"
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
            _process.OutputDataReceived += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    await _outputSemaphore.WaitAsync();
                    try
                    {
                        _output.AppendLine(e.Data);
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
            _process.BeginOutputReadLine();

            // Wysyłamy do Stockfisha komendę "uci"
            SendCommand("uci");
        }

        private void SendCommand(string command)
        {
            _process.StandardInput.WriteLine(command);
        }

        /// <summary>
        /// Pobiera najlepszy ruch z aktualnej pozycji (FEN) do zadanej głębokości.
        /// </summary>
        public async Task<string> GetBestMoveAsync(string fen, int depth = 1)
        {
            // Poczekaj, aż Stockfish będzie gotowy
            await _readyTcs.Task;

            // Czyścimy bufor z poprzednich linii
            await _outputSemaphore.WaitAsync();
            try
            {
                _output.Clear();
            }
            finally
            {
                _outputSemaphore.Release();
            }

            SendCommand($"position fen {fen}");
            SendCommand($"go depth {depth}");

            while (true)
            {
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
