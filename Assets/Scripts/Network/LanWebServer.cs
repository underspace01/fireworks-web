using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Fireworks
{
    public sealed class LanWebServer : MonoBehaviour
    {
        [SerializeField] private FireworkSettings _settings;
        [SerializeField, Min(1)] private int _port = 8080;
        [SerializeField, Min(1)] private int _maximumMessageBytes = 4096;

        private readonly ConcurrentQueue<WebFireworkCommand> _commandQueue = new ConcurrentQueue<WebFireworkCommand>();
        private CancellationTokenSource _cancellationTokenSource;
        private HttpListener _listener;
        private string _accessToken;
        private string _configJson;
        private string _webRootPath;

        public bool IsRunning { get; private set; }
        public string AccessUrl { get; private set; }

        private void Awake()
        {
            if (_settings == null) return;

            _accessToken = Guid.NewGuid().ToString("N");
            _configJson = JsonUtility.ToJson(CreateConfig());
            _webRootPath = Path.Combine(Application.streamingAssetsPath, "WebController");
            StartServer();
        }

        private void OnDestroy() => StopServer();

        public bool TryDequeueCommand(out WebFireworkCommand command) => _commandQueue.TryDequeue(out command);

        private void StartServer()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{_port}/");
                _listener.Start();
                IsRunning = true;
                AccessUrl = $"http://{GetLocalAddress()}:{_port}/";
                _ = ListenAsync(_cancellationTokenSource.Token);
            }
            catch (HttpListenerException)
            {
                StopServer();
            }
        }

        private void StopServer()
        {
            IsRunning = false;
            AccessUrl = null;
            _cancellationTokenSource?.Cancel();
            _listener?.Close();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _listener = null;
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener != null)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            if (!IsPrivateNetworkAddress(context.Request.RemoteEndPoint?.Address))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.Close();
                return;
            }

            string path = context.Request.Url?.AbsolutePath ?? "/";
            if (path == "/api/config")
            {
                await WriteJsonAsync(context.Response, _configJson);
                return;
            }

            if (path == "/api/status")
            {
                await WriteJsonAsync(context.Response, JsonUtility.ToJson(new WebStatus { isRunning = IsRunning }));
                return;
            }

            if (path == "/api/launch" && context.Request.HttpMethod == "POST")
            {
                await ProcessLaunchRequestAsync(context);
                return;
            }

            await WriteStaticFileAsync(context.Response, path);
        }

        private async Task ProcessLaunchRequestAsync(HttpListenerContext context)
        {
            if (context.Request.QueryString["token"] != _accessToken)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Close();
                return;
            }

            if (context.Request.ContentLength64 < 1 || context.Request.ContentLength64 > _maximumMessageBytes)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await WriteJsonAsync(context.Response, JsonUtility.ToJson(new WebResponse
                {
                    type = "error", message = "メッセージが長すぎます"
                }));
                return;
            }

            string message;
            // HttpListenerのContentEncodingはcharset未指定時にUTF-8以外になることがある。
            using (StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8, true))
            {
                message = await reader.ReadToEndAsync();
            }

            await WriteJsonAsync(context.Response, ProcessCommand(message));
        }

        private string ProcessCommand(string message)
        {
            WebFireworkCommand command = JsonUtility.FromJson<WebFireworkCommand>(message);
            if (command == null || command.type != "launch")
            {
                return JsonUtility.ToJson(new WebResponse { type = "error", message = "不正なコマンドです" });
            }

            if (IsTextFireworkCommand(command) && !TryNormalizeText(command))
            {
                return JsonUtility.ToJson(new WebResponse { type = "error", message = "文字列は1〜6文字で入力してください" });
            }

            _commandQueue.Enqueue(command);
            return JsonUtility.ToJson(new WebResponse { type = "accepted", requestId = command.requestId });
        }

        private bool IsTextFireworkCommand(WebFireworkCommand command)
        {
            return _settings != null && _settings.TryGetEffectIndex(command.structure, out int effectIndex) &&
                _settings.GetEffect(effectIndex).IsTextFirework;
        }

        private bool TryNormalizeText(WebFireworkCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.textValue))
            {
                // 更新前のスマホ画面がキャッシュされている場合も受け付ける。
                command.textValue = command.text;
            }

            if (string.IsNullOrWhiteSpace(command.textValue))
            {
                return false;
            }

            command.textValue = command.textValue.Trim();
            if (command.textValue.Length == 0 || command.textValue.Length > 6)
            {
                return false;
            }

            foreach (char character in command.textValue)
            {
                if (char.IsControl(character))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task WriteStaticFileAsync(HttpListenerResponse response, string path)
        {
            string relativePath = path == "/" ? "index.html" : path.TrimStart('/');
            if (relativePath.Contains(".."))
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Close();
                return;
            }

            string filePath = Path.Combine(_webRootPath, relativePath);
            if (!File.Exists(filePath))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(filePath);
            response.ContentType = GetContentType(Path.GetExtension(filePath));
            response.Headers[HttpResponseHeader.CacheControl] = "no-store";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.Close();
        }

        private async Task WriteJsonAsync(HttpListenerResponse response, string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.Close();
        }

        private WebConfig CreateConfig()
        {
            WebConfig config = new WebConfig
            {
                token = _accessToken,
                sizes = new WebOption[_settings.SizeCount],
                structures = new WebOption[_settings.EffectCount],
                colors = new WebOption[_settings.ColorCount]
            };
            for (int index = 0; index < _settings.SizeCount; index++)
            {
                FireworkSizeDefinition size = _settings.GetSize(index);
                config.sizes[index] = new WebOption { id = size.Id, displayName = size.DisplayName, visualScale = size.VisualScale };
            }
            for (int index = 0; index < _settings.EffectCount; index++)
            {
                FireworkTypeDefinition structure = _settings.GetEffect(index);
                config.structures[index] = new WebOption
                {
                    id = structure.Id,
                    displayName = structure.DisplayName,
                    allowsChangeColor = structure.AllowsChangeColor,
                    isTextFirework = structure.IsTextFirework
                };
            }
            for (int index = 0; index < _settings.ColorCount; index++)
            {
                FireworkColorDefinition color = _settings.GetColor(index);
                config.colors[index] = new WebOption { id = color.Id, displayName = color.DisplayName, colorHex = "#" + ColorUtility.ToHtmlStringRGB(color.Value) };
            }
            return config;
        }

        private bool IsPrivateNetworkAddress(IPAddress address)
        {
            if (address == null || IPAddress.IsLoopback(address)) return true;
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
            byte[] bytes = address.GetAddressBytes();
            return bytes[0] == 10 || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168);
        }

        private string GetLocalAddress()
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach (IPAddress address in addresses)
                {
                    if (IsPrivateNetworkAddress(address) && !IPAddress.IsLoopback(address))
                    {
                        return address.ToString();
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                // LANアドレスを取得できない環境では、ローカル確認用アドレスを表示する。
            }

            return IPAddress.Loopback.ToString();
        }

        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".css": return "text/css; charset=utf-8";
                case ".js": return "application/javascript; charset=utf-8";
                default: return "text/html; charset=utf-8";
            }
        }
    }

    [Serializable] public sealed class WebFireworkCommand { public string type; public string requestId; public string sizeId; public string structure; public string colorId; public string changeColorId; public string textValue; public string text; }
    [Serializable] public sealed class WebResponse { public string type; public string requestId; public string message; }
    [Serializable] public sealed class WebStatus { public bool isRunning; }
    [Serializable] public sealed class WebConfig { public string token; public WebOption[] sizes; public WebOption[] structures; public WebOption[] colors; }
    [Serializable] public sealed class WebOption { public string id; public string displayName; public bool allowsChangeColor; public bool isTextFirework; public float visualScale; public string colorHex; }
}
