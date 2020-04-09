using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace game_one_lambda
{
    public class Function
    {

        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();


        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return CreateResponse("hello", 200);
        }

        private string processRequest(string sURL)
        {
            Uri uUri = new Uri(sURL);
            WebRequest wrGETURL;
            WebProxy wpProxy = new WebProxy();
            Stream objStream;
            string resp = "";

            bool isSSL = (uUri.Scheme == Uri.UriSchemeHttps);

            if (!isSSL)
            {
                Console.WriteLine("Secure Connection Error");
            }
            try
            {
                wrGETURL = WebRequest.Create(uUri);

                wrGETURL.Proxy = HttpWebRequest.DefaultWebProxy;

                wrGETURL.Proxy.Credentials = CredentialCache.DefaultCredentials;

                objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                resp = objReader.ReadToEnd();

            }
            catch (Exception e)
            {
                resp = e.ToString();

            }

            return resp;

        }

        private APIGatewayProxyResponse CreateResponse(string resp, int code)
        {
            int statusCode = (int)HttpStatusCode.OK;
            string responseBody = resp;
            if (code != 200)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = responseBody,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }

                }
            };

            return response;
        }
    }

 
}
