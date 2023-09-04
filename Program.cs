using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var hesCodes = new List<string>();
        while (true)
        {
            Console.Write("HES kodunu girin (başka hes kodu girmeyeceksiniz çıkmak için 'q' tuşuna basın): ");
            var input = Console.ReadLine().Trim();

            if (input.ToLower() == "q")
                break;

            hesCodes.Add(input);
        }

        if (hesCodes.Count == 0)
        {
            Console.WriteLine("Hiç HES kodu girmediniz.");
            return;
        }
        var apiBaseUrl = "https://api.saglikbakanligi.gov.tr/HES/dogrula"; // Gerçek URL olmadığı için kod hata döndürecektir.

        var riskLiHesCodes = new List<string>();
        var riskLessHesCodes = new List<string>();

        using (var httpClient = new HttpClient())
        {
            foreach (var hesCode in hesCodes)
            {
                var requestData = new { hes = hesCode };
                var requestBody = JsonSerializer.Serialize(requestData);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync(apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

                        if (responseData.ContainsKey("status"))
                        {
                            var status = responseData["status"];
                            if (status == "risky")
                            {
                                riskLiHesCodes.Add(hesCode);
                            }
                            else if (status == "riskless")
                            {
                                riskLessHesCodes.Add(hesCode);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"HES kodu {hesCode} sorgulanırken bir hata oluştu. HTTP kodu: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"HES kodu {hesCode} sorgulanırken bir hata oluştu: {ex.Message}");
                }
            }
        }

        Console.WriteLine("Riskli HES Kodları:");
        foreach (var hesCode in riskLiHesCodes)
        {
            Console.WriteLine(hesCode);
        }

        Console.WriteLine("\nRiskli Olmayan HES Kodları:");
        foreach (var hesCode in riskLessHesCodes)
        {
            Console.WriteLine(hesCode);
        }
    }
}
