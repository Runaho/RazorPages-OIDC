using System;
namespace RazorPages_OIDC.Models.Configuration
{
    public struct OpenId
    {

        public string APIUrl { get; set; }
        public string APIClientID { get; set; }
        public string ClientRedirection { get; set; }
        public string CallBack { get; set; }
    }
}

