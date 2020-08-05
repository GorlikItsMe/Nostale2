using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using Newtonsoft.Json.Linq; // install this using NuGet
using SimpleJSON;
using System.Net;

using UnityEngine;// debug log

class ntAuth
{
    string locale, gfLang, installation_id;
    string token = null;

    public ntAuth(string _locale="pl_PL", string _gfLang="pl", string _installation_id = "5ea61643-b22a-4ad6-89dd-175b0be2c9d9") {
        locale = _locale;
        gfLang = _gfLang;
        installation_id = _installation_id;
    }

    public bool auth(string _username, string _password) {
        string username = _username;
        string password = _password;
        string URL = "https://spark.gameforge.com/api/v1/auth/sessions";
        try
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(URL);
            if (webRequest != null)
            {
                string reqString = "{\"email\": \"{username}\", \"locale\": \"{locale}\", \"password\": \"{password}\"}";
                reqString = reqString.Replace("{gfLang}", gfLang);
                reqString = reqString.Replace("{username}", username);
                reqString = reqString.Replace("{locale}", locale);
                reqString = reqString.Replace("{password}", password);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36";
                webRequest.Headers.Set("TNT-Installation-Id", installation_id);
                webRequest.Headers.Set("Origin", "spark://www.gameforge.com");

                byte[] requestData = Encoding.UTF8.GetBytes(reqString);
                webRequest.ContentLength = requestData.Length;
                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }

                System.Net.WebResponse response;
                try
                {
                    response = webRequest.GetResponse();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                    return false;
                }
                
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var N = JSON.Parse(responseString);
                token = N["token"].Value;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }
        return false;
    }

    public List<string> getAccounts()
    // return id:nickname
    {
        if (token == null) {
            throw new System.ArgumentException("First use auth", "original");
        }
        string URL = "https://spark.gameforge.com/api/v1/user/accounts";
        try
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(URL);
            if (webRequest != null)
            {
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36";
                webRequest.Headers.Add("TNT-Installation-Id", installation_id);
                webRequest.Headers.Add("Origin", "spark://www.gameforge.com");
                webRequest.Headers.Add("Authorization", "Bearer " + token);
                //webRequest.Connection = "Keep-Alive";

                System.Net.WebResponse response;
                response = webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var N = JSON.Parse(responseString);

                List<string> acc = new List<string>(new string[] { });
                foreach (string key in N.Keys)
                {                    
                    acc.Add(key+":"+N[key]["displayName"].Value);
                }
                return acc;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }
        List<string> accx = new List<string>(new string[] { });
        return accx;
    }

    private string _convertToken(string code)
    {
        byte[] ba = Encoding.Default.GetBytes(code);
        var hexString = BitConverter.ToString(ba);
        hexString = hexString.Replace("-", "");
        return hexString;
    }
    public string getToken(string account)
    {
        if (token == null)
        {
            throw new System.ArgumentException("First use auth", "original");
        }
        string URL = "https://spark.gameforge.com/api/v1/auth/thin/codes";
        try
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(URL);
            if (webRequest != null)
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "GameforgeClient/2.0.48";
                webRequest.Headers.Add("TNT-Installation-Id", installation_id);
                webRequest.Headers.Add("Origin", "spark://www.gameforge.com");
                webRequest.Headers.Add("Authorization", "Bearer " + token);
                //webRequest.Headers.Add("Connection", "Keep-Alive");

                string reqString = "{\"platformGameAccountId\": \"{account}\"}";
                reqString = reqString.Replace("{account}", account);
                byte[] requestData = Encoding.UTF8.GetBytes(reqString);
                webRequest.ContentLength = requestData.Length;
                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }

                System.Net.WebResponse response = webRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //dynamic stuff = JObject.Parse(responseString);
                var N = JSON.Parse(responseString);
                //string code = stuff.code;
                string code = N["code"].Value;
                return _convertToken(code);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }
        return null;
    }
}
