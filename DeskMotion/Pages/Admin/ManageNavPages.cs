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

#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeskMotion.Pages.Admin;

/// <summary>
/// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
public static class ManageNavPages
{
    public static string Index => "Index";
    public static string Desks => "Desks/Index";
    public static string Metadata => "Metadata/Index";
    public static string Reservations => "Reservations/Index";
    public static string Users => "Users/Index";
    public static string Roles => "Roles/Index";
    public static string ManageReports => "ManageReports/Index";
    public static string OfficesPlan => "OfficesPlan/Index";

    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
    public static string DesksNavClass(ViewContext viewContext) => PageNavClass(viewContext, Desks);
    public static string MetadataNavClass(ViewContext viewContext) => PageNavClass(viewContext, Metadata);
    public static string ReservationsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Reservations);
    public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);
    public static string RolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Roles);
    public static string ManageReportsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ManageReports);
    public static string OfficesPlanNavClass(ViewContext viewContext) => PageNavClass(viewContext, OfficesPlan);

    /// <summary>
    /// Determines the active navigation class for the specified page.
    /// </summary>
    /// <param name="viewContext">The current view context.</param>
    /// <param name="page">The page name to check for active state.</param>
    /// <returns>CSS class name indicating the active state, or null if not active.</returns>
    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
