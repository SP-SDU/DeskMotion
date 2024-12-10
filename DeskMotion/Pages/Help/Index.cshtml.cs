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

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskMotion.Pages.Help;

public class IndexModel : PageModel
{
    private readonly List<(string Question, string Answer)> _allFAQs =
    [
        ("How do I reset my password?", "Go to settings and click 'Reset Password'."),
        ("How do I contact support?", "Use the 'Report an Issue' button on this page."),
        ("Can I update my account information?", "Yes, visit your account settings to make updates."),
        ("How do I delete my account?", "Please contact support to permanently delete your account."),
    ];

    public IEnumerable<(string Question, string Answer)> FAQs { get; private set; } = new List<(string, string)>();
    public string SearchQuery { get; private set; } = string.Empty;

    public void OnGet(string? search = null)
    {
        SearchQuery = search ?? string.Empty;

        FAQs = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allFAQs
            : _allFAQs
                .Where(faq => faq.Question.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                              faq.Answer.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
    }
}

