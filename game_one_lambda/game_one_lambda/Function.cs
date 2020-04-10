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
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace game_one_lambda
{
    public class Function
    {

        private static AmazonDynamoDBClient dynamoClient = new AmazonDynamoDBClient();
        private static string tableName = "game_one_connections";

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Dictionary<string, string> requestData = ParseRequest(request);

            // Create a new entry in the database
            try
            {
                Table connections = Table.LoadTable(dynamoClient, tableName);
                string id = request.RequestContext.ConnectionId;

                if (id == null)
                {
                    return CreateResponse("Connection Id cannot be null", 400);
                }

                CreateConnection(connections, id);
            }
            catch (AmazonDynamoDBException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return CreateResponse("Ok", 200);
        }

        private string ProcessRequest(string sURL)
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

        private async void CreateConnection(Table T, string ID)
        {
            Document connection = new Document();
            connection["connectionId"] = ID;
            await T.PutItemAsync(connection);
        }

        private APIGatewayProxyResponse CreateResponse(string resp, int code)
        {
            string responseBody = resp;

            var response = new APIGatewayProxyResponse
            {
                StatusCode = code,
                Body = responseBody,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }

                }
            };

            return response;
        }

        private Dictionary<string, string> ParseRequest(APIGatewayProxyRequest request)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            //"{\"test\":\"body\"}" -> "test:body"
            string bodyText = request.Body.Replace("{", "").Replace("\\", "").Replace("}", "").Replace("\"", "");

            foreach (string KeyValue in bodyText.Split(","))
            {
                var tmp = KeyValue.Split(":");
                output[tmp[0]] = tmp[1];
            }

            return output;
        }
    }

 
}
