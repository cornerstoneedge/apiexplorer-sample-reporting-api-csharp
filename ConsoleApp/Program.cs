using System;
using Api;

namespace ConsoleClient
{
    internal class Program
    {
        private const string ApiBaseAddress = "https://[portal].csod.com"; //Enter the Portal for the Service        
        private const string ClientId = ""; //Enter the client id from the portal.        
        private const string ClientSecret = ""; //Enter the client secret from the portal.
        private const string ApiViewsPath = "/services/api/x/odata/api/views";
        private const int PrintLength = 500;

        private static EdgeApiClient _client;

        public static void Main(string[] args)
        {
            try
            {
                _client = new EdgeApiClient(new Uri(ApiBaseAddress), ClientId, ClientSecret);
                ReadApiAction();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to close");
            Console.ReadLine();
        }

        private static void ReadApiAction()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Edge OData API samples");
                Console.WriteLine();
                Console.WriteLine("Select an action:");
                Console.WriteLine();
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Run all");
                Console.WriteLine("2. Get metadata");
                Console.WriteLine("3. Get only count from vw_rpt_user");
                Console.WriteLine("4. Get all data from vw_rpt_user");
                Console.WriteLine("5. Get data from vw_rpt_user by pages");
                Console.WriteLine();
                Console.Write("Input number and press Enter: ");

                var number = int.Parse(Console.ReadLine());
                switch (number)
                {
                    case 0:
                        return;
                    case 1:
                        ExecuteMetadata();
                        ExecuteCount();
                        ExecuteAllData();
                        ExecutePaging();
                        break;
                    case 2:
                        ExecuteMetadata();
                        break;
                    case 3:
                        ExecuteCount();
                        break;
                    case 4:
                        ExecuteAllData();
                        break;
                    case 5:
                        ExecutePaging();
                        break;
                    default:
                        Console.WriteLine("Wrong input");
                        break;
                }
            }
        }

        private static void ExecuteMetadata()
        {
            Console.WriteLine("Getting metadata...");
            var stringContent = _client.GetStringAsync(new Uri($@"{ApiViewsPath}/$metadata", UriKind.Relative)).Result;
            Console.WriteLine(
                $"Response length is {stringContent.Length}. First {PrintLength} characters: {stringContent.Substring(0, PrintLength)}");
        }

        private static void HandleError(EdgeApiODataPayload payload)
        {
            Console.WriteLine(payload.ErrorValue != null
                ? $"Error occurred. Code: {payload.ErrorValue.Error.Code}, message: {payload.ErrorValue.Error.Message}"
                : "Data is retrieved without errors.");
        }

        private static void ExecuteCount()
        {
            Console.WriteLine("Getting only count from vw_rpt_user...");
            var payload = _client
                .GetODataPayloadAsync(new Uri($@"{ApiViewsPath}/vw_rpt_user?$count=true&$top=0", UriKind.Relative))
                .Result;
            HandleError(payload);
            Console.WriteLine($"Got count {payload?.Count}");
        }

        private static void ExecuteAllData()
        {
            Console.WriteLine("Getting all data from vw_rpt_user...");
            var payload = _client
                .GetODataPayloadAsync(new Uri($@"{ApiViewsPath}/vw_rpt_user?$count=true&$top=0", UriKind.Relative))
                .Result;
            payload = _client.GetODataPayloadAsync(new Uri($@"{ApiViewsPath}/vw_rpt_user", UriKind.Relative),
                (int) payload.Count).Result;

            HandleError(payload);
            Console.WriteLine($"Got {payload?.Value?.Count} values");
        }

        private static void ExecutePaging()
        {
            Console.WriteLine("Getting data from vw_rpt_user by pages...");
            var payload = _client.GetODataPayloadAsync(new Uri($@"{ApiViewsPath}/vw_rpt_user", UriKind.Relative), 10)
                .Result;
            HandleError(payload);
            Console.WriteLine($"Got {payload?.Value?.Count} values");
            Console.WriteLine($"Next link is '{payload?.NextLink}'");

            var address = payload.NextLink.Replace(ApiBaseAddress, string.Empty);
            Console.WriteLine("Getting data by next link...");
            payload = _client.GetODataPayloadAsync(new Uri(address, UriKind.Relative), 10).Result;
            HandleError(payload);
            Console.WriteLine($"Got {payload?.Value?.Count} values");
            Console.WriteLine($"Next link is '{payload?.NextLink}'");
            Console.WriteLine("...");
        }
    }
}