# Crime Data Summary Application

## Overview
This application allows users to enter a latitude, longitude and month to display a summary of crimes that occurred, grouped by crime category. It utilizes the "all-crime" API endpoint from the UK Police Data API to fetch and display the data.

## Features
- User input for latitude, longitude and month.
- Display of crime data grouped by category.
- Detailed information for each crime, including category, month, location type, street name, outcome status and outcome date.

## Technologies Used
- .NET 8
- Razor Pages
- ASP.NET Core MVC
- HttpClient for API requests

## Getting Started

### Prerequisites
- .NET 8 SDK

### Installation
1. Clone the repository:
   git clone https://github.com/FlyingQuokka/WPCTest

2. Navigate to the project directory:
   cd crime-data-summary

3. Restore the project dependencies:
   dotnet restore

4. Build the project:
   dotnet build

### Running the Application
Run the application with the following command:

dotnet run

The application will start and be accessible at `http://localhost:5023` by default.

## Usage
1. Open the application in your web browser.
2. Enter the latitude, longitude and month for which you want to view crime data.
3. Submit the form to view the summary of crimes grouped by category.

## Project Structure
- `Pages/`: Contains the Razor Pages for the application's UI.
- `Services/`: Includes the `CrimeDataService` responsible for fetching and processing crime data from the UK Police API.
- `Models/`: Defines the data models, such as `CrimeData` for handling crime information and `CrimeCategorySummary` for grouping crimes by category.
- `wwwroot/`: Houses static files, including CSS for styling.
- `Program.cs`: The entry point for the application, configuring services and middleware.
