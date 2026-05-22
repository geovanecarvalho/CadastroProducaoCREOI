using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace CadastroProducaoCRE.Services
{
    public class ConfigManager
    {
        private static readonly string CONFIG_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cadastro_producao_cre");
        private static readonly string CONFIG_FILE = Path.Combine(CONFIG_DIR, "config.json");
        private static readonly string KEY_FILE = Path.Combine(CONFIG_DIR, "secret.key");
        
        private Dictionary<string, object> _configData;
        private byte[] _key;
        private bool _debug;

        public ConfigManager(bool debug = false)
        {
            _debug = debug;
            _configData = new Dictionary<string, object>();
            Initialize();
        }

        private void Log(string message)
        {
            if (_debug)
            {
                Console.WriteLine($"[ConfigManager] {message}");
            }
        }

        private void Initialize()
        {
            // Cria diretório se não existir
            Directory.CreateDirectory(CONFIG_DIR);
            Log($"Diretório de config: {CONFIG_DIR}");

            // Carrega configurações existentes
            LoadConfig();
            
            // Só configura a criptografia se o arquivo de configuração existir ou quando for salvar
            if (File.Exists(CONFIG_FILE))
            {
                SetupEncryption();
            }
            else
            {
                Log("Arquivo de configuração não existe, criptografia será configurada no primeiro salvamento");
            }
        }

        private void SetupEncryption()
        {
            if (File.Exists(KEY_FILE))
            {
                // Carrega chave existente
                _key = File.ReadAllBytes(KEY_FILE);
                Log("Chave de criptografia carregada");
            }
            else
            {
                // Cria nova chave
                _key = GenerateKey();
                File.WriteAllBytes(KEY_FILE, _key);
                Log("Nova chave de criptografia criada");
            }
        }

        private byte[] GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return aes.Key;
            }
        }

        private string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(password);
                    }
                    var encrypted = Convert.ToBase64String(ms.ToArray());
                    Log("Senha criptografada");
                    return encrypted;
                }
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return "";

            try
            {
                var fullCipher = Convert.FromBase64String(encryptedPassword);
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    var iv = new byte[aes.BlockSize / 8];
                    var cipher = new byte[fullCipher.Length - iv.Length];
                    Array.Copy(fullCipher, iv, iv.Length);
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                    using (var decryptor = aes.CreateDecryptor(_key, iv))
                    using (var ms = new MemoryStream(cipher))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        var decrypted = sr.ReadToEnd();
                        Log("Senha descriptografada");
                        return decrypted;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Erro ao descriptografar: {ex.Message}");
                return "";
            }
        }

        public bool SaveConfig(Dictionary<string, object> config)
        {
            try
            {
                // Se ainda não tem chave de criptografia, configura agora
                if (_key == null)
                {
                    SetupEncryption();
                }
                
                Log($"Salvando configuração - last_file: {config.GetValueOrDefault("last_file", "")}");
                
                var dataToSave = new Dictionary<string, object>
                {
                    ["username"] = config.GetValueOrDefault("username", "") ?? "",
                    ["password"] = EncryptPassword(config.GetValueOrDefault("password", "")?.ToString() ?? ""),
                    ["headless"] = config.GetValueOrDefault("headless", false),
                    ["last_file"] = config.GetValueOrDefault("last_file", "") ?? "",
                    ["window_geometry"] = config.GetValueOrDefault("window_geometry", new Dictionary<string, int>()),
                    ["last_updated"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ["version"] = "1.0"
                };

                var json = JsonSerializer.Serialize(dataToSave, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json, Encoding.UTF8);
                
                Log($"Configurações salvas em: {CONFIG_FILE}");
                Log($"last_file salvo: {dataToSave["last_file"]}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Erro ao salvar configuração: {ex.Message}");
                return false;
            }
        }

        public Dictionary<string, object> LoadConfig()
        {
            if (!File.Exists(CONFIG_FILE))
            {
                Log("Arquivo de config não existe, usando padrão");
                return GetDefaultConfig();
            }

            try
            {
                var json = File.ReadAllText(CONFIG_FILE, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                Log($"Configurações carregadas de: {CONFIG_FILE}");

                // Descriptografa a senha
                if (data.ContainsKey("password") && data["password"] != null)
                {
                    var encryptedPassword = data["password"].ToString();
                    if (!string.IsNullOrEmpty(encryptedPassword))
                    {
                        data["password"] = DecryptPassword(encryptedPassword);
                    }
                }

                return data ?? GetDefaultConfig();
            }
            catch (Exception ex)
            {
                Log($"Erro ao carregar configuração: {ex.Message}");
                return GetDefaultConfig();
            }
        }

        private Dictionary<string, object> GetDefaultConfig()
        {
            return new Dictionary<string, object>
            {
                ["username"] = "",
                ["password"] = "",
                ["headless"] = false,
                ["last_file"] = "",
                ["window_geometry"] = new Dictionary<string, int>
                {
                    ["x"] = 100,
                    ["y"] = 100,
                    ["width"] = 800,
                    ["height"] = 700
                },
                ["last_updated"] = "",
                ["version"] = "1.0"
            };
        }

        public bool ClearConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    File.Delete(CONFIG_FILE);
                    Log("Arquivo de configuração removido");
                }
                if (File.Exists(KEY_FILE))
                {
                    File.Delete(KEY_FILE);
                    Log("Chave de criptografia removida");
                }

                _configData = GetDefaultConfig();
                return true;
            }
            catch (Exception ex)
            {
                Log($"Erro ao limpar configurações: {ex.Message}");
                return false;
            }
        }

        public void SaveLastFile(string filePath)
        {
            var config = LoadConfig();
            config["last_file"] = filePath;
            SaveConfig(config);
            Log($"Último arquivo salvo: {filePath}");
        }

        public string GetLastFile()
        {
            var config = LoadConfig();
            var lastFile = config.GetValueOrDefault("last_file", "")?.ToString() ?? "";
            Log($"Último arquivo recuperado: {lastFile}");
            return lastFile;
        }
    }
}