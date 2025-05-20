using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClipEnhancer.Data;
using ClipEnhancer.Models;
using Microsoft.EntityFrameworkCore;
using ClipEnhancer.Helpers;
using System.Collections.Generic;

namespace ClipEnhancer.Views;

public partial class HistoryPage : UserControl
{
    private readonly ApplicationDbContext _dbContext;

    public HistoryPage()
    {
        InitializeComponent();
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);

        InitializeFilters();
        LoadHistory();
    }

    private void LoadHistory()
    {
        var query = _dbContext.ClipboardHistory.AsQueryable();

        // Apply filters
        var selectedStatus = StatusFilter.SelectedItem?.ToString();
        if (selectedStatus != null && selectedStatus != "All")
            query = query.Where(h => h.Status == selectedStatus);

        var selectedTag = TagFilter.SelectedItem?.ToString();
        if (selectedTag != null && selectedTag != "All")
            query = query.Where(h => h.Tags.Contains(selectedTag));

        if (DateFilter.SelectedDate.HasValue)
            query = query.Where(h => h.Timestamp.Date == DateFilter.SelectedDate.Value.Date);

        // Order by most recent first
        query = query.OrderByDescending(h => h.Timestamp);

        HistoryListView.ItemsSource = query.ToList();
    }

    private void InitializeFilters()
    {
        StatusFilter.ItemsSource = new[] { "All", "Success", "Error" };
        StatusFilter.SelectedItem = "All";

        var allTags = new List<string> { "All" };
        
        // Grammar & Language Tags
        allTags.AddRange(new[] { "grammar", "punctuation", "spelling", "syntax", "tense", "agreement" });
        
        // Tone Tags
        allTags.AddRange(new[] { "formal", "casual", "friendly", "polite", "assertive", "diplomatic", 
            "empathetic", "enthusiastic", "neutral", "humorous", "serious", "academic" });
        
        // Style Tags
        allTags.AddRange(new[] { "fluent", "concise", "detailed", "firstletter", "allcaps", "sentencecase", 
            "titlecase", "bulletpoints", "numbered", "paragraph", "simplify", "expand", "active", "passive" });
        
        // Format Tags
        allTags.AddRange(new[] { "markdown", "html", "json", "xml", "csv", "table", "code", 
            "quote", "indent", "spacing" });
        
        // Content Tags
        allTags.AddRange(new[] { "summarize", "keypoints", "translate", "paraphrase", "proofread", 
            "factcheck", "citations", "references" });
        
        // Audience-Specific Tags
        allTags.AddRange(new[] { "technical", "layman", "expert", "beginner", "children", "senior" });
        
        // Purpose Tags
        allTags.AddRange(new[] { "persuasive", "informative", "instructional", "descriptive", 
            "narrative", "analytical", "critical" });
        
        // Industry-Specific Tags
        allTags.AddRange(new[] { "legal", "medical", "scientific", "business" });
        
        // Special Purpose Tags
        allTags.AddRange(new[] { "seo", "social", "email", "report", "presentation", "blog", "news" });

        TagFilter.ItemsSource = allTags;
        TagFilter.SelectedItem = "All";
    }

    private void ApplyFilters_Click(object sender, RoutedEventArgs e)
    {
        LoadHistory();
    }

    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        StatusFilter.SelectedIndex = 0;
        TagFilter.SelectedIndex = 0;
        DateFilter.SelectedDate = null;
        LoadHistory();
    }

    public void RefreshHistory()
    {
        LoadHistory();
    }
} 