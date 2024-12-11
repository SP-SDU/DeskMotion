// Copyright 2024 PET Group16
//
// Licensed under the Apache License, Version 2.0 (the "License"):
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;
using System.Text.Json;

namespace DeskMotion.Services;

public class RestRepository<T>(HttpClient httpClient, JsonSerializerOptions serializerOptions = null!)
{
    /// <summary> Retrieves a single entity from the specified endpoint. </summary>
    /// <param name="endpoint">The API endpoint.</param>
    /// <returns>An instance of T.</returns>
    public virtual async Task<T> GetAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint must not be null or whitespace.", nameof(endpoint));

        using var response = await httpClient.GetAsync(endpoint);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(content, serializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return result;
    }

    /// <summary> Sends a POST request to the specified endpoint with the provided data. </summary>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="data">The data to post.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task PostAsync(string endpoint, object data)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint must not be null or whitespace.", nameof(endpoint));

        var json = JsonSerializer.Serialize(data, serializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync(endpoint, content);
        _ = response.EnsureSuccessStatusCode();
    }

    /// <summary> Sends a PUT request to the specified endpoint with the provided data. </summary>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="data">The data to put.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task PutAsync(string endpoint, object data)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint must not be null or whitespace.", nameof(endpoint));

        var json = JsonSerializer.Serialize(data, serializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync(endpoint, content);
        _ = response.EnsureSuccessStatusCode();
    }

    /// <summary> Sends a DELETE request to the specified endpoint. </summary>
    /// <param name="endpoint">The API endpoint.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint must not be null or whitespace.", nameof(endpoint));

        using var response = await httpClient.DeleteAsync(endpoint);
        _ = response.EnsureSuccessStatusCode();
    }
}
