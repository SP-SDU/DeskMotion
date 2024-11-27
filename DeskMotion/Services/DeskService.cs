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

using DeskMotion.Models;
using System.Text;
using System.Text.Json;

namespace DeskMotion.Services;

public class DeskService(HttpClient httpClient)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<string>> GetDeskIdsAsync()
    {
        var response = await httpClient.GetAsync("desks");
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<string>>(content, _jsonOptions) ?? [];
    }

    public async Task<Desk> GetDeskAsync(string deskId)
    {
        var response = await httpClient.GetAsync($"desks/{deskId}");
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var desk = JsonSerializer.Deserialize<Desk>(content, _jsonOptions);
        desk!.MacAddress = deskId;
        desk.LastErrors ??= [];
        return desk;
    }

    public async Task UpdateDeskPositionAsync(string deskId, int newPositionMm)
    {

        var content = new StringContent(
            JsonSerializer.Serialize(new { position_mm = newPositionMm }, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PutAsync($"desks/{deskId}/state", content);
        _ = response.EnsureSuccessStatusCode();
    }
}
