namespace WPCTest.Services;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Polly;
using WPCTest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

/// <summary>
/// Handles communication with the UK Police API to fetch crime data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CrimeDataService class, using an injected HttpClientFactory for HTTP requests.
/// </remarks>
/// <param name="httpClientFactory">A factory for creating instances of HttpClient.</param>
public class CrimeDataService(IHttpClientFactory httpClientFactory) {
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly JsonSerializerSettings _settings = new() {
        ContractResolver = new DefaultContractResolver {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    /// <summary>
		/// Fetches and processes crime data from the UK Police API based on the specified location and date.
		/// </summary>
		/// <param name="latitude">The latitude of the location for which to retrieve crime data.</param>
		/// <param name="longitude">The longitude of the location for which to retrieve crime data.</param>
		/// <param name="date">The date for which to retrieve crime data, in the format YYYY-MM.</param>
		/// <returns>A list of <see cref="CrimeData"/> objects, each representing a summary of crimes by category.</returns>
		/// <exception cref="HttpRequestException">Thrown when the request to the API fails.</exception>
		/// <exception cref="Exception">Thrown when an unexpected error occurs during processing.</exception>
	public async Task<List<CrimeCategorySummary>> GetCrimeDataAsync(string? latitude, string? longitude, string? date) {
        var httpClient = _httpClientFactory.CreateClient();
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        HttpResponseMessage response = await retryPolicy.ExecuteAsync(() =>
            httpClient.GetAsync($"https://data.police.uk/api/crimes-street/all-crime?lat={latitude}&lng={longitude}&date={date}"));

        if (response.IsSuccessStatusCode) {
            string content = await response.Content.ReadAsStringAsync();
            var crimes = JsonConvert.DeserializeObject<List<CrimeData>>(content, settings: _settings) ?? throw new Exception("Failed to deserialize API response.");

            if (crimes == null) return [];

            // Grouping the crimes by category and organizing them into a list of CrimeCategorySummary
            var groupedCrimes = crimes
                .GroupBy(crime => crime.Category)
                .OrderBy(grouped => grouped.Key)
                .Select(ordered => new CrimeCategorySummary {
                    Category = ordered.Key,
                    Crimes = ordered.ToList()
                }).ToList();

            return groupedCrimes;
        } else {
            throw new HttpRequestException($"API call failed with status code {response.StatusCode}");
        }
    }
}