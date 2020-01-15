using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BirdieLib
{
    public class Request
    {
        public RestClient RestClient;

        public Request()
        {
            RestClient = new RestClient("http://api.kriscraig.com");
        }

        public RestRequest Prepare(string url, Method method, List<Parameter> parameters, List<FileParameter> files, string contentType = "application/x-www-form-urlencoded")
        {
            RestRequest restRequest = new RestRequest(url, method);

            if (restRequest.Method == Method.POST || restRequest.Method == Method.PUT)
            {
                restRequest.AddHeader("Content-Type", contentType);
            }

            foreach (Parameter param in parameters)
            {
                if (!param.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                    && !param.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    restRequest.AddParameter(param);
                }
            }

            foreach (FileParameter file in files)
            {
                restRequest.Files.Add(file);
            }

            return restRequest;
        }

        public RestRequest Prepare(string url, Method method = Method.GET, string contentType = "application/x-www-form-urlencoded")
        {
            return Prepare(url, method, new List<Parameter>(), new List<FileParameter>(), contentType);
        }

        public string ExecuteRequest(RestRequest restRequest)
        {
            IRestResponse res = RestClient.Execute(restRequest);

            int retry = 5;
            do
            {
                if (res != null && res.IsSuccessful)
                {
                    return res.Content;
                }
                else if (res.StatusCode == 0)
                {
                    retry--;
                    if (retry == 0)
                    {
                        Exception ex = new Exception("Birdie API returned empty (status code == 0) response after multiple retries.");

                        ex.Data.Add("res", res);

                        throw ex;
                    }

                    Thread.Sleep(3000);

                    res = RestClient.Execute(restRequest);
                }
                else
                {
                    Exception ex = new Exception("Birdie API returned non-success response.");

                    ex.Data.Add("res", res);

                    throw ex;
                }
            } while (!res.IsSuccessful && retry > 0);

            return res.Content;
        }

        public T GetJSON<T>(string url, Method method, List<Parameter> parameters, List<FileParameter> files, string contentType = "application/x-www-form-urlencoded")
        {
            return GetJSON<T>(Prepare(url, method, parameters, files, contentType));
        }

        public T GetJSON<T>(RestRequest restRequest)
        {
            return JsonConvert.DeserializeObject<T>(ExecuteRequest(restRequest));
        }

        public void AddParamIfNotNull(string name, object value, ref RestRequest restRequest)
        {
            if (value != null)
            {
                restRequest.AddParameter(name, value);
            }
        }

        public void AddParamIfNotNull(string name, DateTime? value, ref RestRequest restRequest)
        {
            if (value.HasValue)
            {
                restRequest.AddParameter(name, value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }
}
