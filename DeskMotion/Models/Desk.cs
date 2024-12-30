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

using System.Text.Json.Serialization;

namespace DeskMotion.Models;

public class Desk
{
    [JsonIgnore]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public bool IsLatest { get; set; } = true;

    [JsonIgnore]
    public string MacAddress { get; set; } = string.Empty;

    // Navigation Properties
    public Config Config { get; set; } = default!;
    public State State { get; set; } = default!;
    public Usage Usage { get; set; } = default!;
    public List<LastError> LastErrors { get; set; } = default!;
}
