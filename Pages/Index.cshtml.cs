namespace WPCTest.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using WPCTest.Models;
using WPCTest.Services;

/// <summary>
/// The PageModel for the Index page, handling user inputs and displaying crime data.
/// </summary>
public class IndexModel(CrimeDataService crimeDataService) : PageModel {
    private readonly CrimeDataService _crimeDataService = crimeDataService;

    [BindProperty]
    [Required(ErrorMessage = "Latitude is required.")]
    public string? Latitude { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Longitude is required.")]
    public string? Longitude { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Date is required.")]
    public string? Date { get; set; }

    public string? ErrorMessage { get; set; }

    public List<CrimeCategorySummary> CrimeData { get; set; } = [];

    /// <summary>
    /// Handles the POST request for the Index page to fetch and display crime data.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of updating the page with crime data.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) {
            ErrorMessage = "Please correct the errors and try again.";
            return Page();
        }

        try {
            // Fetching grouped crime data based on user input
            CrimeData = await _crimeDataService.GetCrimeDataAsync(Latitude, Longitude, Date);
        } catch (Exception ex) {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return Page();
    }
}
