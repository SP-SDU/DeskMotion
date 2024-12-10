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

using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Models;

[Owned]
public class State
{
    public int Position_mm { get; set; }
    public int Speed_mms { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPositionLost { get; set; }
    public bool IsOverloadProtectionUp { get; set; }
    public bool IsOverloadProtectionDown { get; set; }
    public bool IsAntiCollision { get; set; }
}
