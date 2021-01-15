using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BetterFusion.BetterFusionObjects;

namespace BetterFusion
{
    // Better Fusion made by Khrysus
    // I made this because I saw it wasn't async, I improved it by splitting the classes and also turning them into structs so they are faster
    public struct BetterFusionApp
    {
        private readonly HttpClient client = null;
        public static string baseurl = null;
        public static string executeurl = null;
        private static string session;
        private static readonly string url = "https://fusionapi.dev/";

        public BetterFusionApp(string App)
        {
            if (client == null)
            {
                var handler = new HttpClientHandler()
                {
                    Proxy = null
                };
                client = new HttpClient(handler);
                baseurl = $"{url}app/{App}/api";
            }
        }

        private async static Task Check42FA(string username)
        {
            var g2faDictionary = new Dictionary<string, string>
            {
                { "action", "has2fa" },
                { "username", username }
            };
            var g2fasc = new FormUrlEncodedContent(g2faDictionary);
            var g2faResponse = await client.PostAsync(baseurl, g2fasc);
            var g2faContent = await g2faResponse.Content.ReadAsStringAsync();
            var g2faResp = JsonConvert.DeserializeObject<BetterFusionResponse>(g2faContent);

            if (g2faResp.Session == "true")
                User.ValidateMfa = true;
            else
                User.ValidateMfa = false;
        }
        public async static Task RefreshApp()
        {
            var blobDictionary2 = new Dictionary<string, string>
            {
                { "action", "appblob" },
                { "session", session }
            };
            var blobsc2 = new FormUrlEncodedContent(blobDictionary2);
            var blobResponse2 = await client.PostAsync(baseurl, blobsc2);
            var blobContent2 = await blobResponse2.Content.ReadAsStringAsync();
            var obj2 = JsonConvert.DeserializeObject<BetterFusionObjects.AppObject>(blobContent2);
            App.ActiveApis = obj2.ActiveApi;
            App.ApiCount = obj2.ApiCount;
            App.UserCount = obj2.UserCount;
            App.AppName = obj2.Label;
            App.AppDescription = obj2.Description;
        }
        public static async Task<string> GetIp()
        {
            var webResponse = await client.GetAsync("https://api.ipify.org/?format=json");
            var responsePayload = await webResponse.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<BetterFusionObjects.IpObject>(responsePayload);
            return obj.IpAddress;
        }
        public static async Task<string> GetHwid()
        {
            return await Task.Run(() => WindowsIdentity.GetCurrent().User.Value);
        }
        public async Task<BetterFusionResponse> Login(string username, string password, string g2fa = null, bool checkhwid = false, bool checkip = false)
        {
            await Check42FA(username);
            var loginDictionary = new Dictionary<string, string>();

            if (User.ValidateMfa == true)
            {
                loginDictionary.Add("action", "login");
                loginDictionary.Add("username", username);
                loginDictionary.Add("password", password);
                loginDictionary.Add("2fa", g2fa);
            }
            else
            {
                loginDictionary.Add("action", "login");
                loginDictionary.Add("username", username);
                loginDictionary.Add("password", password);
            }

            var sc = new FormUrlEncodedContent(loginDictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);
                if (!resp.Error) session = resp.Session;

                try
                {
                    var blobDictionary = new Dictionary<string, string>
                    {
                        { "action", "myblob" },
                        { "session", resp.Session }
                    };
                    var blobsc = new FormUrlEncodedContent(blobDictionary);
                    var blobResponse = await client.PostAsync(baseurl, blobsc);
                    var blobContent = await blobResponse.Content.ReadAsStringAsync();

                    var blobDictionary2 = new Dictionary<string, string>
                    {
                        { "action", "appblob" },
                        { "session", resp.Session }
                    };
                    var blobsc2 = new FormUrlEncodedContent(blobDictionary2);
                    var blobResponse2 = await client.PostAsync(baseurl, blobsc2);
                    var blobContent2 = await blobResponse2.Content.ReadAsStringAsync();

                    var obj1 = JsonConvert.DeserializeObject<BetterFusionObjects.AuthObject>(blobContent);

                    string expdate;
                    try { expdate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(obj1.Expiry)).DateTime.ToString(); } catch { expdate = "N/a"; }

                    User.MfaCode = obj1.TwoFactorCode;
                    User.Ip = await GetIp();
                    User.HardwareId = await GetHwid();
                    User.Level = obj1.Level;
                    User.Expiry = expdate;
                    User.UserId = obj1.UserId;
                    User.Username = username;

                    var obj2 = JsonConvert.DeserializeObject<BetterFusionObjects.AppObject>(blobContent2);
                    App.ActiveApis = obj2.ActiveApi;
                    App.ApiCount = obj2.ApiCount;
                    App.UserCount = obj2.UserCount;
                    App.AppName = obj2.Label;
                    App.AppDescription = obj2.Description;

                    if (checkhwid == true)
                    {
                        try
                        {
                            if (!string.Equals(GetUserVar("hwid"), User.HardwareId))
                            {
                                MessageBox.Show("Hardware ID's do not match, closing the app", "Modded FusionAPI by Big man Khrysus (ASYNC!)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Environment.Exit(0);
                            }
                        }
                        catch
                        {
                            await SetUserVar("hwid", User.HardwareId);
                        }
                    }
                    if (checkip == true)
                    {
                        try
                        {
                            if (!string.Equals(GetUserVar("ip"), User.Ip))
                            {
                                MessageBox.Show("IP Addresses do not match, closing the app", "Modded FusionAPI by Big man Khrysus (ASYNC!)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Environment.Exit(0);
                            }
                        }
                        catch
                        {
                            // Set user var ip if it dont exist
                            await SetUserVar("ip", User.Ip);
                        }
                    }
                }
                catch
                {

                }

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public async Task<BetterFusionResponse> Register(string username, string password, string token)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "register" },
                { "token", token },
                { "username", username },
                { "password", password }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);
                if (!resp.Error) session = resp.Session;

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task<BetterFusionResponse> ResetPassword(string oldpassword, string newpassword)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "register" },
                { "session", session },
                { "oldpassword", oldpassword },
                { "newpassword", newpassword }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);
                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task<string> GetUserVar(string var)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "myvars" },
                { "session", session }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var obj1 = JsonConvert.DeserializeObject<BetterFusionObjects.VarObject>(content);
                return obj1.Var;
            }
            catch (JsonReaderException)
            {
                return "Response Was Not Valid";
            }
        }
        public static async Task SetUserVar(string key, string value)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "set-user-vars" },
                { "session", session },
                { "key", key },
                { "value", value }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetAppVar(string var)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "get-app-vars" },
                { "session", session }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var obj1 = JsonConvert.DeserializeObject<BetterFusionObjects.VarObject>(content);
                return obj1.Var;
            }
            catch (JsonReaderException)
            {
                return "Response Was Not Valid";
            }
        }
        public static async Task<BetterFusionResponse> ExecuteAPI(string id, string data)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "data", data }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(executeurl + id, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task<BetterFusionResponse> ExecuteTimeAPI(string id, string data, int time)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "data", data },
                { "time", time.ToString() }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync($"{url}executeapi/" + id, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task<BetterFusionResponse> ExecuteAuthAPI(string id, string data)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "data", data },
                { "session", session }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync($"{url}executeapi/" + id, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task<BetterFusionResponse> ExecuteFullAPI(string id, string data, int time)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "data", data },
                { "time", time.ToString() },
                { "session", session }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync($"{url}executeapi/" + id, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<BetterFusionResponse>(content);

                return resp;
            }
            catch (JsonReaderException)
            {
                return new BetterFusionResponse { Error = true, Message = "Response Was Not Valid" };
            }
        }
        public static async Task SendMessage(string message)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "sendmsg" },
                { "session", session },
                { "message", message }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetChat()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "action", "getchat" },
                { "session", session }
            };
            var sc = new FormUrlEncodedContent(dictionary);
            var response = await client.PostAsync(baseurl, sc);
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var obj1 = JsonConvert.DeserializeObject<dynamic>(content);
                return content;
            }
            catch (JsonReaderException)
            {
                return "Response Was Not Valid";
            }
        }
    }
}
