namespace WPCTest.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Provides integration tests for the Index Razor Page, ensuring the page correctly handles user input and displays data.
/// </summary>
public class IndexPageTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>> {
	private readonly HttpClient _client = factory.CreateClient();

	/// <summary>
	/// Verifies that the form submission with valid data correctly displays crime data.
	/// </summary>
	[Fact]
	public async Task Post_ValidData_ReturnsCrimeSummary() {
		// Arrange
		var formData = new FormUrlEncodedContent(new[] {
			new KeyValuePair<string, string>("Latitude", "51.509865"),
			new KeyValuePair<string, string>("Longitude", "-0.118092"),
			new KeyValuePair<string, string>("Date", "2021-01"),
		});

		// Act
		var response = await _client.PostAsync("/", formData);
		var responseString = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Contains("anti-social-behaviour", responseString);
	}

	/// <summary>
	/// Tests the behavior of the Index page when the API returns an error or no data.
	/// </summary>
	[Fact]
	public async Task Post_ApiFailureOrEmptyData_ShowsNoDataMessage() {
		// Arrange
		// Simulate API failure or no data scenario. This might involve setting up a mock server,
		// adjusting the application's startup configuration for testing to use a mock service,
		// or using predefined conditions in your application that you can trigger for testing.

		var formData = new FormUrlEncodedContent(new[] {
			new KeyValuePair<string, string>("Latitude", "51.509865"),
			new KeyValuePair<string, string>("Longitude", "-0.118092"),
			new KeyValuePair<string, string>("Date", "2021-01"),
		});

		// Act
		var response = await _client.PostAsync("/", formData);
		var responseString = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Contains("No crime data available", responseString);
	}

	/// <summary>
	/// Tests form submission with missing or invalid data and verifies that appropriate validation messages are displayed.
	/// </summary>
	[Theory]
	[InlineData("", "-0.118092", "2021-01", "Latitude is required.")]
	[InlineData("51.509865", "", "2021-01", "Longitude is required.")]
	[InlineData("51.509865", "-0.118092", "", "Date is required.")]
	public async Task Post_InvalidData_ReturnsValidationMessage(string latitude, string longitude, string date, string expectedMessage) {
		// Arrange
		var formData = new FormUrlEncodedContent(new[] {
			new KeyValuePair<string, string>("Latitude", latitude),
			new KeyValuePair<string, string>("Longitude", longitude),
			new KeyValuePair<string, string>("Date", date),
		});

		// Act
		var response = await _client.PostAsync("/", formData);
		var responseString = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Contains(expectedMessage, responseString);
	}

	/// <summary>
	/// Tests the behavior of the Index page when an unexpected error occurs, ensuring that a user-friendly error message is displayed.
	/// </summary>
	[Fact]
	public async Task Post_UnexpectedError_DisplaysGenericErrorMessage() {
		// Arrange
		// To simulate an unexpected error, you may need to configure a scenario within your application
		// that triggers an error, such as providing input that causes an exception or temporarily modifying
		// a service to throw an exception. This setup would be specific to how your application is architected.

		var formData = new FormUrlEncodedContent(new[] {
			new KeyValuePair<string, string>("Latitude", "invalid"), // Provide data that might lead to an unexpected error
            new KeyValuePair<string, string>("Longitude", "invalid"),
			new KeyValuePair<string, string>("Date", "invalid"),
		});

		// Act
		var response = await _client.PostAsync("/", formData);
		var responseString = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Contains("An unexpected error occurred", responseString);
	}
}
