[![Build Status](https://chriss-scratch.visualstudio.com/scratch/_apis/build/status/scratch-ASP.NET%20Core%20(.NET%20Framework)-CI?branchName=master)](https://chriss-scratch.visualstudio.com/scratch/_build/latest?definitionId=6&branchName=master)

# The API

This Web API allows consumers to search for Food Trucks in San Francisco that are within a certain distance of a given longitude and latitude.

## API Usage

```
GET /api/foodtruck
```

### Parameters
| Name | Type | Description |
| --- | --- | --- |
| `distance` | int | Distance in meters from the longitude and latitude to include in the search |
| `longitude` | double | Longitude to search from, in decimal degrees. Valid values are from -180.0 to 180.0 |
| `latitude` | double | Latitude to search from, in decimal degrees. Valid values are from -90.0 to 90.0 |

**Example query**

```
curl https://nearbyfoodtrucks.azurewebsites.net/api/foodtruck?distance=2500&longitude=-122.4329697&latitude=37.72756654
```

# Technical Solution and Approach Considerations
## Data Storage Thoughts
-   The dataset for this specific scenario is quite small and could be easily stored in memory if desired. Arguably, even if we were to consume datasets from many more cities, this is also reasonably cachable in memory. 
-   A database could be used for the dataset, which would allow for nearly infinite scalability, but this could make searches less efficient depending on the platform choice.
    -   Cosmos DB is a db solution that happens to [support native Geospatial indexing and querying](https://docs.microsoft.com/en-us/azure/cosmos-db/geospatial)

## Algorithmic Thoughts
Given that the dataset could be resident in memory, searching for the closest food trucks could be done after "indexing" them on app initialization and searching that index on each request. Some basic approaches considered (ordered from worst to best):
-   Create 2 lists of all items sorted by longitude and by latitude. Given the search coordinates, binary search for where the closest value to the given longitude and slice the surrounding range of locations within the given distance along that dimension. Do the same for the latitude sorted items. Find the intersection between the two lists.
-   Take all the locations and build a 2D index array, based on longitude/latitude values rounded to the nearest 100 meters or so (approximately the nearest thousandths of a degree). Create a dictionary indexing the grid location to the array coordinates containing the given latitude / longitude. Return the items in that bucket and expand to its neighbor coordinates until enough results are found.
-   Research and implement an established world class geospatial indexing scheme.

## Approach Decision
After considering the above, I decided to ingest the data into Cosmos DB and rely on it's geospatial indexing and querying capabilities. The rationale is as follows:
-   If this were a real production scenario, I'd much rather rely on a battle tested, out of the box Azure solution as opposed to my own home-grown search algorithm.
-   This approach is infinitely scalable while being relatively cheap at low scale.
-   Having the data ingested in Cosmos opens up flexibly for various other filtering criteria at global scale.
-   Offloading the search heavy listing to Cosmos allowed me to spend more time on the quality and completeness of the solution.

## Stack, Hosting, and Deployment
-   Asp dot Net Core (2.2) Web Api
-   Azure Web App
-   Cosmos Db
-   Azure KeyVault to host Cosmos database key secret
-   Application Insights for telemetry
-   Azure DevOps CI build pipeline with CI production deployment

# ToDo
Below is a list of features or enhancements to the solution that could be implemented with more time.

### Future Features
-   Create UI to visualize the search results in a friendly format
-   Visualize the result locations on a map
-   Create clickable links for each result that opens it in your phone's mapping app for directions
-   Request the client's current GPS coordinates as the default for the search coordinates
-   Implement search based on food type
-   Implement filters based on hours of operation
-   Secure the API with consumer specific API keys
-   Implement call quotas and metering

### Technical or Infrastructure Enhancements
-   Ingest open API data from the open API source
-   Implement config based extensible data sources to pull additional locations of interest for search
-   Implement an Azure Function to ingest or update data sources periodically
-   Integration testing via `WebApplicationFactory`
-   Swagger documentation of the API


