using System.Text.Json;
using System.Net.Http;

namespace CadastroProducaoCRE.Services;

public class UpdateService
{
    private readonly Action<string> _log;
    private readonly string _githubRepo = "geovanecarvalho/CadastroProducaoCREOI"; // ALTERE PARA SEU REPOSITÓRIO
    private readonly string _currentVersion = "1.0.0";
    
    public event Action<UpdateInfo>? OnUpdateAvailable;
    public event Action<string, int>? OnDownloadProgress;
    public event Action? OnUpdateCompleted;
    public event Action<string>? OnError;
    
    public UpdateService(Action<string> logCallback)
    {
        _log = logCallback;
    }
    
    public string CurrentVersion => _currentVersion;
    
    private void Log(string message)
    {
        _log?.Invoke(message);
    }
    
    public async Task<bool> VerificarAtualizacaoAsync(bool notificarSeAtualizado = false)
    {
        try
        {
            Log("🔍 Verificando atualizações...");
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "CadastroProducaoCRE-App");
            client.Timeout = TimeSpan.FromSeconds(15);
            
            var url = $"https://api.github.com/repos/{_githubRepo}/releases/latest";
            var response = await client.GetStringAsync(url);
            var release = JsonSerializer.Deserialize<GitHubRelease>(response);
            
            if (release == null) 
            {
                Log("Resposta vazia da API");
                return false;
            }
            
            var remoteVersion = release.tag_name.TrimStart('v');
            Log($"📊 Versão local: {_currentVersion}, Versão remota: {remoteVersion}");
            
            if (remoteVersion != _currentVersion)
            {
                Log($"✨ Nova versão disponível: {remoteVersion}");
                
                var asset = release.assets?.FirstOrDefault(a => a.name?.EndsWith(".exe") == true);
                
                var updateInfo = new UpdateInfo
                {
                    VersaoAtual = _currentVersion,
                    VersaoNova = remoteVersion,
                    Descricao = release.body ?? "Melhorias e correções de bugs",
                    DataLancamento = release.published_at ?? "",
                    DownloadUrl = asset?.browser_download_url ?? "",
                    Tamanho = asset?.size ?? 0
                };
                
                OnUpdateAvailable?.Invoke(updateInfo);
                return true;
            }
            
            if (notificarSeAtualizado)
            {
                Log("✅ Você está usando a versão mais recente!");
            }
            else
            {
                Log("✅ Nenhuma atualização disponível");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Log($"⚠️ Erro ao verificar atualização: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> BaixarAtualizacaoAsync(string downloadUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "CadastroProducaoCRE-App");
            
            var tempFile = Path.Combine(Path.GetTempPath(), $"CadastroProducaoCRE_Update_{DateTime.Now:yyyyMMdd_HHmmss}.exe");
            
            using (var downloadStream = await client.GetStreamAsync(downloadUrl))
            using (var fileStream = File.Create(tempFile))
            {
                var buffer = new byte[8192];
                int bytesRead;
                long totalBytes = 0;
                
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    totalBytes = response.Content.Headers.ContentLength ?? 0;
                }
                
                long bytesReceived = 0;
                
                while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    bytesReceived += bytesRead;
                    
                    var percent = totalBytes > 0 ? (int)(bytesReceived * 100 / totalBytes) : 0;
                    OnDownloadProgress?.Invoke($"Baixando... {percent}%", percent);
                }
            }
            
            OnDownloadProgress?.Invoke("Download concluído!", 100);
            await Task.Delay(500);
            
            return await AplicarAtualizacao(tempFile);
        }
        catch (Exception ex)
        {
            Log($"❌ Erro ao baixar atualização: {ex.Message}");
            OnError?.Invoke($"Erro ao baixar: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> AplicarAtualizacao(string novoExecutavel)
    {
        try
        {
            var currentExe = Application.ExecutablePath;
            var backupExe = currentExe + ".old";
            var scriptPath = Path.Combine(Path.GetTempPath(), $"update_script_{DateTime.Now:yyyyMMdd_HHmmss}.bat");
            
            // Script batch
            var script = $@"
@echo off
timeout /t 2 /nobreak > nul
del ""{backupExe}"" 2>nul
move ""{currentExe}"" ""{backupExe}""
move ""{novoExecutavel}"" ""{currentExe}""
start """" ""{currentExe}""
del ""%~f0""
";
            
            await File.WriteAllTextAsync(scriptPath, script);
            
            // Executar batch em modo oculto
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start /min \"\" \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            
            System.Diagnostics.Process.Start(psi);
            
            Log("✅ Atualização concluída! O aplicativo será reiniciado...");
            
            await Task.Delay(500);
            Application.Exit();
            
            return true;
        }
        catch (Exception ex)
        {
            Log($"❌ Erro ao aplicar atualização: {ex.Message}");
            OnError?.Invoke($"Erro ao aplicar: {ex.Message}");
            return false;
        }
    }
    
    public string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class UpdateInfo
{
    public string VersaoAtual { get; set; } = string.Empty;
    public string VersaoNova { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string DataLancamento { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long Tamanho { get; set; }
}

public class GitHubRelease
{
    public string tag_name { get; set; } = string.Empty;
    public string body { get; set; } = string.Empty;
    public string published_at { get; set; } = string.Empty;
    public List<GitHubAsset> assets { get; set; } = new();
}

public class GitHubAsset
{
    public string name { get; set; } = string.Empty;
    public string browser_download_url { get; set; } = string.Empty;
    public long size { get; set; }
}