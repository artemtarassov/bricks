using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CustomLogQueue
{
    private sealed class QueuedLog
    {
        public long Id;
        public string Data;
    }

    private static readonly string SERVER_URL = "https://badmonkees.com/log.php";

    public static CustomLogQueue Instance;

    private static readonly string LocalFileSave = "tmpLogs.txt";
    public static readonly int MaxQueueSize = 100;

    private const int RequestTimeoutSeconds = 10;
    private const int RequestHangGraceSeconds = 5;
    private const int MaxBatchEntries = 30;
    private const int MaxBatchBytes = 32 * 1024;
    private const float BaseRetryDelaySeconds = 5f;
    private const float MaxRetryDelaySeconds = 300f;

    private readonly List<QueuedLog> _queue = new List<QueuedLog>();
    private readonly object _syncRoot = new object();
    private string _savePath;
    private readonly List<QueuedLog> _inFlightBatch = new List<QueuedLog>();

    private UnityWebRequest _activeRequest;
    private float _requestStartedAt;
    private float _nextRetryAt;
    private int _consecutiveFailures;
    private long _nextId = 1;

    public CustomLogQueue Setup()
    {
        _savePath = Path.Combine(Application.persistentDataPath, LocalFileSave);
        LoadFromDisk();
        return this;

    }

    public void Add(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        //Debug.Log($"CustomLogQueue adding log: {data}");

        if (!data.EndsWith("\n", StringComparison.Ordinal))
        {
            data += "\n";
        }

        lock (_syncRoot)
        {
            _queue.Add(new QueuedLog { Id = _nextId++, Data = data });
            TrimQueueLocked();

            SaveToDiskLocked();
        }
    }

    // Called every 10 seconds.
    public void OnUpdate()
    {
        if (_activeRequest != null)
        {
            if (_activeRequest.isDone)
            {
                CompleteActiveRequest();
            }
            else if (Time.realtimeSinceStartup - _requestStartedAt > RequestTimeoutSeconds + RequestHangGraceSeconds)
            {
                FailActiveRequest("Request exceeded watchdog timeout.");
            }

            return;
        }

        if (Time.realtimeSinceStartup < _nextRetryAt)
        {
            return;
        }

        StartNextRequest();
    }

    private void StartNextRequest()
    {
        string payload = BuildNextPayload();
        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("data", payload);
        form.AddField("count", _inFlightBatch.Count.ToString());

        _activeRequest = UnityWebRequest.Post(SERVER_URL, form);
        _activeRequest.timeout = RequestTimeoutSeconds;
        _requestStartedAt = Time.realtimeSinceStartup;
        _activeRequest.SendWebRequest();
    }

    private string BuildNextPayload()
    {
        lock (_syncRoot)
        {
            _inFlightBatch.Clear();

            if (_queue.Count == 0)
            {
                return string.Empty;
            }

            int totalBytes = 0;

            foreach (QueuedLog entry in _queue)
            {
                int entryBytes = Encoding.ASCII.GetByteCount(entry.Data);
                bool wouldExceedEntryLimit = _inFlightBatch.Count >= MaxBatchEntries;
                bool wouldExceedByteLimit = _inFlightBatch.Count > 0 && totalBytes + entryBytes > MaxBatchBytes;

                if (wouldExceedEntryLimit || wouldExceedByteLimit)
                {
                    break;
                }

                _inFlightBatch.Add(entry);
                totalBytes += entryBytes;
            }

            StringBuilder builder = new StringBuilder(totalBytes);
            for (int i = 0; i < _inFlightBatch.Count; i++)
            {
                builder.Append(_inFlightBatch[i].Data);
            }

            return builder.ToString();
        }
    }

    private void CompleteActiveRequest()
    {
        if (_activeRequest == null)
        {
            return;
        }

        bool success = IsSuccessful(_activeRequest);

        if (success)
        {
            lock (_syncRoot)
            {
                HashSet<long> sentIds = new HashSet<long>();
                for (int i = 0; i < _inFlightBatch.Count; i++)
                {
                    sentIds.Add(_inFlightBatch[i].Id);
                }

                _queue.RemoveAll(entry => sentIds.Contains(entry.Id));
                SaveToDiskLocked();
            }

            _consecutiveFailures = 0;
            _nextRetryAt = 0f;
        }
        else
        {
            string error = string.IsNullOrEmpty(_activeRequest.error)
                ? $"HTTP {(long)_activeRequest.responseCode}"
                : _activeRequest.error;

            RegisterFailure(error);
        }

        DisposeActiveRequest();
    }

    private void FailActiveRequest(string reason)
    {
        if (_activeRequest != null)
        {
            _activeRequest.Abort();
        }

        RegisterFailure(reason);
        DisposeActiveRequest();
    }

    private void RegisterFailure(string reason)
    {
        _consecutiveFailures++;
        float delay = Mathf.Min(
            MaxRetryDelaySeconds,
            BaseRetryDelaySeconds * Mathf.Pow(2f, _consecutiveFailures - 1));

        _nextRetryAt = Time.realtimeSinceStartup + delay;
        Debug.LogWarning($"LogQueue send failed ({reason}). Retrying in {delay:0.#} seconds.");
    }

    private void DisposeActiveRequest()
    {
        if (_activeRequest != null)
        {
            _activeRequest.Dispose();
            _activeRequest = null;
        }

        _inFlightBatch.Clear();
        _requestStartedAt = 0f;
    }

    private bool IsSuccessful(UnityWebRequest request)
    {
        if (request == null)
        {
            return false;
        }

        return request.result == UnityWebRequest.Result.Success &&
               request.responseCode >= 200 &&
               request.responseCode < 300;
    }

    private void LoadFromDisk()
    {
        lock (_syncRoot)
        {
            _queue.Clear();

            if (!File.Exists(_savePath))
            {
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(_savePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    try
                    {
                        byte[] bytes = Convert.FromBase64String(line);
                        _queue.Add(new QueuedLog
                        {
                            Id = _nextId++,
                            Data = Encoding.ASCII.GetString(bytes)
                        });
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning("LogQueue skipped a corrupt persisted log entry.");
                    }

                    TrimQueueLocked();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"LogQueue failed to load persisted logs: {ex.Message}");
                _queue.Clear();
            }
        }
    }

    private void SaveToDiskLocked()
    {
        try
        {
            string directory = Path.GetDirectoryName(_savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = _savePath + ".tmp";
            List<string> encodedLines = new List<string>(_queue.Count);
            foreach (QueuedLog entry in _queue)
            {
                encodedLines.Add(Convert.ToBase64String(Encoding.ASCII.GetBytes(entry.Data)));
            }

            File.WriteAllLines(tempPath, encodedLines.ToArray());

            if (File.Exists(_savePath))
            {
                File.Copy(tempPath, _savePath, true);
                File.Delete(tempPath);
            }
            else
            {
                File.Move(tempPath, _savePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LogQueue failed to persist logs: {ex.Message}");
        }
    }

    private void TrimQueueLocked()
    {
        if (_queue.Count <= MaxQueueSize)
        {
            return;
        }

        HashSet<long> protectedIds = new HashSet<long>();
        for (int i = 0; i < _inFlightBatch.Count; i++)
        {
            protectedIds.Add(_inFlightBatch[i].Id);
        }

        while (_queue.Count > MaxQueueSize)
        {
            int removeIndex = -1;
            for (int i = 0; i < _queue.Count; i++)
            {
                if (!protectedIds.Contains(_queue[i].Id))
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex < 0)
            {
                removeIndex = 0;
            }

            _queue.RemoveAt(removeIndex);
        }
    }
}
