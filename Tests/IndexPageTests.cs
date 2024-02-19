using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WPCTest.Models;
using WPCTest.Pages;
using WPCTest.Services;
using Xunit;

public class IndexModelTests {
	private readonly Mock<CrimeDataService> _mockCrimeDataService;
	private readonly IndexModel _indexModel;

	public IndexModelTests() {
		_mockCrimeDataService = new Mock<CrimeDataService>();
		_indexModel = new IndexModel(_mockCrimeDataService.Object);
	}

	[Fact]
	public async Task OnPostAsync_WithMissingLatitude_ReturnsPageWithModelError() {
		// Arrange
		_indexModel.Longitude = "0.1278";
		_indexModel.Date = "2023-01-01";

		// Act
		var result = await _indexModel.OnPostAsync();

		// Assert
		Assert.False(_indexModel.ModelState.IsValid);
		Assert.Contains(_indexModel.ModelState, ms => ms.Key == nameof(_indexModel.Latitude));
	}

	[Fact]
	public async Task OnPostAsync_ServiceThrowsException_AddsModelError() {
		// Arrange
		_mockCrimeDataService.Setup(s => s.GetCrimeDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
							 .ThrowsAsync(new System.Exception("Service error"));
		_indexModel.Latitude = "50.827845";
		_indexModel.Longitude = "-0.143088";
		_indexModel.Date = "2023-01-01";

		// Act
		var result = await _indexModel.OnPostAsync();

		// Assert
		Assert.False(_indexModel.ModelState.IsValid);
		var modelStateEntry = Assert.Contains(string.Empty, _indexModel.ModelState);
		var modelError = Assert.Single(modelStateEntry!.Errors);
		Assert.Equal("Service error", modelError.ErrorMessage);
	}

	[Fact]
	public async Task OnPostAsync_WithValidInput_CallsGetCrimeDataAsyncOnce() {
		// Arrange
		var expectedData = new List<CrimeCategorySummary> { new CrimeCategorySummary { Category = "theft", Crimes = [] } };
		_mockCrimeDataService.Setup(s => s.GetCrimeDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
							 .ReturnsAsync(expectedData);

		_indexModel.Latitude = "50.827845";
		_indexModel.Longitude = "-0.143088";
		_indexModel.Date = "2023-01-01";

		// Act
		await _indexModel.OnPostAsync();

		// Assert
		_mockCrimeDataService.Verify(s => s.GetCrimeDataAsync("50.827845", "-0.143088", "2023-01-01"), Times.Once);
		Assert.Equal(expectedData, _indexModel.CrimeData);
	}
}
