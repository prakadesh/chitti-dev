using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ClipEnhancer.Data;
using ClipEnhancer.Models;
using ClipEnhancer.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using System.Linq;

namespace ClipEnhancer.Views;

public partial class HistoryWindow : Window
{
    private readonly ApplicationDbContext _dbContext;

    public HistoryWindow()
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
        if (StatusFilter.SelectedItem != null && StatusFilter.SelectedItem.ToString() != "All")
            query = query.Where(h => h.Status == StatusFilter.SelectedItem.ToString());

        if (TagFilter.SelectedItem != null && TagFilter.SelectedItem.ToString() != "All")
            query = query.Where(h => h.Tags.Contains(TagFilter.SelectedItem.ToString()));

        if (DateFilter.SelectedDate.HasValue)
            query = query.Where(h => h.Timestamp.Date == DateFilter.SelectedDate.Value.Date);

        // Order by most recent first
        query = query.OrderByDescending(h => h.Timestamp);

        HistoryListView.ItemsSource = query.ToList();
    }

    private void InitializeFilters()
    {
        StatusFilter.ItemsSource = new[] { "All", "Success", "Error" };
        StatusFilter.SelectedIndex = 0;

        TagFilter.ItemsSource = new[] { "All", "grammar", "style", "tone" };
        TagFilter.SelectedIndex = 0;
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

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (HistoryListView.SelectedItem is ClipboardHistory item)
        {
            var result = System.Windows.MessageBox.Show(
                "Copy processed text? (No for original text)",
                "Copy Text",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            var textToCopy = result == MessageBoxResult.Yes ? item.ProcessedText : item.OriginalText;
            System.Windows.Forms.Clipboard.SetText(textToCopy);
        }
    }

    private async void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to clear all history?",
            "Clear History",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _dbContext.ClipboardHistory.RemoveRange(_dbContext.ClipboardHistory);
            await _dbContext.SaveChangesAsync();
            LoadHistory();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 