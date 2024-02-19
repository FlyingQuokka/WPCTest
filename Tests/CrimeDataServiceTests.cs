namespace WPCTest.Tests;

using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WPCTest.Services;
using Xunit;

/// <summary>
/// Provides unit tests for the CrimeDataService class to ensure correct API communication and data processing.
/// </summary>
public class CrimeDataServiceTests {
	private readonly CrimeDataService _service;
	private readonly Mock<HttpMessageHandler> _handlerMock;

	public CrimeDataServiceTests() {
		_handlerMock = new Mock<HttpMessageHandler>();

		var client = new HttpClient(_handlerMock.Object) {
			BaseAddress = new System.Uri("https://data.police.uk/api/"),
		};

		var httpClientFactory = new Mock<IHttpClientFactory>();
		httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

		_service = new CrimeDataService(httpClientFactory.Object);
	}

	/// <summary>
	/// Verifies that GetCrimeDataAsync successfully parses and returns data when the API call is successful.
	/// </summary>
	[Fact]
	public async Task GetCrimeDataAsync_SuccessfulCall_ReturnsData() {
		// Arrange
		var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK) {
			Content = new StringContent("[{\"category\": \"anti-social-behaviour\", \"date\": \"2021-01\"}]"),
		};

		_handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(fakeResponse);

		// Act
		var result = await _service.GetCrimeDataAsync("51.509865", "-0.118092", "2021-01");

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.Equal("anti-social-behaviour", result[0].Category);
	}

	/// <summary>
	/// Verifies that GetCrimeDataAsync properly handles and retries on receiving HTTP 429 responses.
	/// </summary>
	[Fact]
	public async Task GetCrimeDataAsync_WithRetryOnHttp429_SuccessAfterRetries() {
		// Arrange
		var attempts = 0;
		_handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(() => {
				attempts++;
				return attempts == 3 ? new HttpResponseMessage(HttpStatusCode.OK) {
					Content = new StringContent("[{\"category\": \"burglary\", \"date\": \"2021-01\"}]"),
				} : new HttpResponseMessage(HttpStatusCode.TooManyRequests);
			});

		// Act
		var result = await _service.GetCrimeDataAsync("51.509865", "-0.118092", "2021-01");

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.Equal("burglary", result[0].Category);
	}

	/// <summary>
	/// Verifies that GetCrimeDataAsync handles invalid latitude and longitude parameters gracefully.
	/// </summary>
	[Theory]
	[InlineData("", "-0.118092", "2021-01")]
	[InlineData("51.509865", "", "2021-01")]
	[InlineData("invalid", "invalid", "2021-01")]
	public async Task GetCrimeDataAsync_InvalidParameters_ThrowsArgumentException(string latitude, string longitude, string date) {
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCrimeDataAsync(latitude, longitude, date));
	}

	/// <summary>
	/// Tests that GetCrimeDataAsync returns an empty list when the API response is empty, indicating no crimes found.
	/// </summary>
	[Fact]
	public async Task GetCrimeDataAsync_NoCrimesFound_ReturnsEmptyList() {
		// Arrange
		var emptyApiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
			Content = new StringContent("[]"),
		};

		_handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(emptyApiResponse);

		// Act
		var result = await _service.GetCrimeDataAsync("51.509865", "-0.118092", "2021-01");

		// Assert
		Assert.Empty(result);
	}

	/// <summary>
	/// Tests that GetCrimeDataAsync properly handles unexpected error codes from the API by throwing an HttpRequestException.
	/// </summary>
	[Fact]
	public async Task GetCrimeDataAsync_ApiReturnsError_ThrowsHttpRequestException() {
		// Arrange
		var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		_handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(errorResponse);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetCrimeDataAsync("51.509865", "-0.118092", "2021-01"));
	}
}
