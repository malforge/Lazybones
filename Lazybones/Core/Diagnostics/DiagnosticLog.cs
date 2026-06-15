using System;
using System.IO;
using Lazybones.Core.State;

namespace Lazybones.Core.Diagnostics;

/// <summary>
/// Tiny append-only diagnostic log living next to the state/history files
/// (diagnostics.log in the data dir). Best-effort — it swallows I/O errors and
/// must never affect the running app. It exists to chase hard-to-reproduce
/// lifecycle bugs that can't be explained from history.jsonl alone, currently
/// the day-rollover state machine: a RolloverReset once fired at an hour the
/// code says is impossible (LastRolloverAppliedAt only moves forward), and we
/// couldn't tell whether a stealth restart had reloaded stale state.
///
/// Each line is timestamped and tagged with the process id, so separate app
/// sessions — including a restart we'd otherwise have no record of — are
/// distinguishable when reading the log back.
/// </summary>
public static class DiagnosticLog
{
    private static readonly object _lock = new();
    private static readonly string _filePath =
        Path.Combine(AppState.GetDataDir(), "diagnostics.log");

    public static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [pid:{Environment.ProcessId}] {message}{Environment.NewLine}";
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? string.Empty);
                File.AppendAllText(_filePath, line);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Diagnostics must never take down the app over a transient I/O error.
        }
    }
}
