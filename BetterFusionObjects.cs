using Newtonsoft.Json;
using System.Collections.Generic;

namespace BetterFusion
{
    public static class BetterFusionObjects
    {
        public struct AppObject
        {
            [JsonProperty("activeapis")] public string ActiveApi { get; set; }
            [JsonProperty("apicount")] public string ApiCount { get; set; }
            [JsonProperty("usercount")] public string UserCount { get; set; }
            [JsonProperty("label")] public string Label { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
        }

        public struct VarObject
        {
            [JsonProperty("vars")] public string Var { get; set; }
        }

        public struct AuthObject
        {
            [JsonProperty("using2fa")] public string UsingTwoFactor { get; set; }
            [JsonProperty("2fa-code")] public string TwoFactorCode { get; set; }
            [JsonProperty("level")] public string Level { get; set; }
            [JsonProperty("uid")] public string UserId { get; set; }
            [JsonProperty("expiry")] public string Expiry { get; set; }
        }

        public struct IpObject
        {
            [JsonProperty("ip")] public string IpAddress { get; set; }
        }

        public struct BetterFusionResponse
        {
            public bool Error { get; set; }
            public string Session { get; set; }
            public string Message { get; set; }
            public string Response { get; set; }
            public string Status { get; set; }
            public string Blob { get; set; }
        }
        public struct App
        {
            public static string ActiveApis;
            public static string ApiCount;
            public static string UserCount;
            public static string AppName;
            public static string AppDescription;
        }
        public struct User
        {
            public static bool ValidateMfa = false;
            public static string MfaCode;
            public static string Ip;
            public static string HardwareId;
            public static string Level;
            public static string Expiry;
            public static string Username;
            public static string UserId;
        }
        public struct ChatMessage
        {
            public string Author;
            public string Content;
            public int Timestamp;
        }
        public struct ChatResponse
        {
            public List<ChatMessage> Chat;
            public bool Error;
            public string Message;
        }
    }
}
