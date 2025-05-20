using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Chitti.Data;
using Chitti.Models;
using Chitti.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chitti.Services;

public class ClipboardMonitorService
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern byte VkKeyScan(char ch);

    private const byte VK_SHIFT = 0x10;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_V = 0x56;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const int TYPING_DELAY_MS = 20; // Adjust this value to control typing speed

    private readonly ApplicationDbContext _dbContext;
    private readonly GeminiService _geminiService;
    private bool _isMonitoring;
    private System.Windows.Forms.Timer _clipboardTimer;
    private string _lastClipboardText = string.Empty;

    public event EventHandler<string>? StatusChanged;

    private bool ContainsCodeBlock(string text)
    {
        // Check for common code block indicators
        return text.Contains("```") || 
               text.Contains("    ") || // 4 spaces for code blocks
               text.Contains("\t") ||   // Tab for code blocks
               Regex.IsMatch(text, @"^\s*[a-zA-Z0-9_]+\(.*\)\s*{", RegexOptions.Multiline) || // Function definitions
               Regex.IsMatch(text, @"^\s*[a-zA-Z0-9_]+:\s*[a-zA-Z0-9_]+", RegexOptions.Multiline); // Type definitions
    }

    private bool ContainsFormattedText(string text)
    {
        // Check for common formatting indicators
        return text.Contains("*") || // Bold/Italic
               text.Contains("_") || // Underline
               text.Contains("#") || // Headers
               text.Contains(">") || // Blockquotes
               text.Contains("- ") || // Lists
               text.Contains("1. ");  // Numbered lists
    }

    private async Task SimulateTyping(string text)
    {
        try
        {
            // If the text contains code blocks or complex formatting, use paste instead
            if (ContainsCodeBlock(text) || ContainsFormattedText(text))
            {
                SimulatePaste();
                return;
            }

            foreach (char c in text)
            {
                // Skip Enter key
                if (c == '\r' || c == '\n')
                {
                    continue;
                }

                // Get the virtual key code for the character
                byte vk = VkKeyScan(c);
                bool needsShift = (vk & 0x100) != 0;
                vk &= 0xFF;

                // Handle uppercase letters
                if (char.IsUpper(c))
                {
                    needsShift = true;
                }

                if (needsShift)
                {
                    // Press Shift
                    keybd_event(VK_SHIFT, 0, 0, 0);
                }

                // Press the key
                keybd_event(vk, 0, 0, 0);
                // Release the key
                keybd_event(vk, 0, KEYEVENTF_KEYUP, 0);

                if (needsShift)
                {
                    // Release Shift
                    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
                }

                // Add a small delay between keystrokes
                await Task.Delay(TYPING_DELAY_MS);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error simulating typing: {ex}");
            // Fallback to paste if typing fails
            SimulatePaste();
        }
    }

    private void SimulatePaste()
    {
        try
        {
            // Press Ctrl
            keybd_event(VK_CONTROL, 0, 0, 0);
            // Press V
            keybd_event(VK_V, 0, 0, 0);
            // Release V
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error simulating paste: {ex}");
        }
    }

    public ClipboardMonitorService(GeminiService geminiService)
    {
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);
        _geminiService = geminiService;
        _clipboardTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _clipboardTimer.Tick += ClipboardTimer_Tick;
    }

    public void StartMonitoring()
    {
        if (!_isMonitoring)
        {
            _isMonitoring = true;
            _clipboardTimer.Start();
            StatusChanged?.Invoke(this, "Monitoring clipboard...");
        }
    }

    public void StopMonitoring()
    {
        if (_isMonitoring)
        {
            _isMonitoring = false;
            _clipboardTimer.Stop();
            StatusChanged?.Invoke(this, "Clipboard monitoring stopped");
        }
    }

    private async void ClipboardTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isMonitoring) return;

        try
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
            {
                var text = System.Windows.Forms.Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text) || text == _lastClipboardText)
                {
                    StatusChanged?.Invoke(this, "Clipboard empty or unchanged.");
                    Logger.Log("Clipboard empty or unchanged.");
                    return;
                }
                _lastClipboardText = text;

                var tags = ExtractTags(text);
                Logger.Log($"Clipboard text: '{text}'. Tags detected: [{string.Join(", ", tags)}]");
                if (!tags.Any())
                {
                    StatusChanged?.Invoke(this, "No tags found in clipboard text.");
                    Logger.Log("No tags found in clipboard text.");
                    return;
                }

                StatusChanged?.Invoke(this, "Processing text...");
                Logger.Log("Processing text with Gemini API...");

                var processedText = await _geminiService.ProcessText(text, tags);
                if (string.IsNullOrEmpty(processedText))
                {
                    Logger.Log("Gemini API returned empty result.");
                    return;
                }

                System.Windows.Forms.Clipboard.SetText(processedText);
                Logger.Log("Clipboard updated with processed text.");

                // Simulate typing the processed text
                await SimulateTyping(processedText);

                var history = new ClipboardHistory
                {
                    OriginalText = text,
                    ProcessedText = processedText,
                    Tags = string.Join(",", tags),
                    Timestamp = DateTime.UtcNow,
                    Status = "Success"
                };

                _dbContext.ClipboardHistory.Add(history);
                await _dbContext.SaveChangesAsync();

                StatusChanged?.Invoke(this, "Text processed and typed automatically");
                Logger.Log("Text processed, typed, and saved to history.");
            }
            else
            {
                StatusChanged?.Invoke(this, "Clipboard does not contain text.");
                Logger.Log("Clipboard does not contain text.");
            }
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            Logger.Log($"Error: {ex}");

            var history = new ClipboardHistory
            {
                OriginalText = System.Windows.Forms.Clipboard.GetText(),
                ProcessedText = string.Empty,
                Tags = string.Empty,
                Timestamp = DateTime.UtcNow,
                Status = "Error",
                ErrorMessage = ex.Message
            };

            _dbContext.ClipboardHistory.Add(history);
            await _dbContext.SaveChangesAsync();
        }
    }

    private List<string> ExtractTags(string text)
    {
        var tags = new List<string>();
        var lowered = text.ToLowerInvariant();
        
        // Special Purpose Tags
        if (lowered.Contains("@chitti"))
        {
            // Extract the custom prompt after @chitti
            var chittiIndex = lowered.IndexOf("@chitti");
            if (chittiIndex >= 0)
            {
                var prompt = text.Substring(chittiIndex + "@chitti".Length).Trim();
                if (!string.IsNullOrEmpty(prompt))
                {
                    tags.Add($"@chitti {prompt}");
                }
            }
        }

        // Grammar & Language Tags
        if (lowered.Contains("/grammar")) tags.Add("grammar");
        if (lowered.Contains("/punctuation")) tags.Add("punctuation");
        if (lowered.Contains("/spelling")) tags.Add("spelling");
        if (lowered.Contains("/syntax")) tags.Add("syntax");
        if (lowered.Contains("/tense")) tags.Add("tense");
        if (lowered.Contains("/agreement")) tags.Add("agreement");

        // Tone Tags
        if (lowered.Contains("/formal")) tags.Add("formal");
        if (lowered.Contains("/casual")) tags.Add("casual");
        if (lowered.Contains("/friendly")) tags.Add("friendly");
        if (lowered.Contains("/polite")) tags.Add("polite");
        if (lowered.Contains("/assertive")) tags.Add("assertive");
        if (lowered.Contains("/diplomatic")) tags.Add("diplomatic");
        if (lowered.Contains("/empathetic")) tags.Add("empathetic");
        if (lowered.Contains("/enthusiastic")) tags.Add("enthusiastic");
        if (lowered.Contains("/neutral")) tags.Add("neutral");
        if (lowered.Contains("/humorous")) tags.Add("humorous");
        if (lowered.Contains("/serious")) tags.Add("serious");
        if (lowered.Contains("/academic")) tags.Add("academic");

        // Style Tags
        if (lowered.Contains("/fluent")) tags.Add("fluent");
        if (lowered.Contains("/concise")) tags.Add("concise");
        if (lowered.Contains("/detailed")) tags.Add("detailed");
        if (lowered.Contains("/firstletter")) tags.Add("firstletter");
        if (lowered.Contains("/allcaps")) tags.Add("allcaps");
        if (lowered.Contains("/sentencecase")) tags.Add("sentencecase");
        if (lowered.Contains("/titlecase")) tags.Add("titlecase");
        if (lowered.Contains("/bulletpoints")) tags.Add("bulletpoints");
        if (lowered.Contains("/numbered")) tags.Add("numbered");
        if (lowered.Contains("/paragraph")) tags.Add("paragraph");
        if (lowered.Contains("/simplify")) tags.Add("simplify");
        if (lowered.Contains("/expand")) tags.Add("expand");
        if (lowered.Contains("/active")) tags.Add("active");
        if (lowered.Contains("/passive")) tags.Add("passive");

        // Format Tags
        if (lowered.Contains("/markdown")) tags.Add("markdown");
        if (lowered.Contains("/html")) tags.Add("html");
        if (lowered.Contains("/json")) tags.Add("json");
        if (lowered.Contains("/xml")) tags.Add("xml");
        if (lowered.Contains("/csv")) tags.Add("csv");
        if (lowered.Contains("/table")) tags.Add("table");
        if (lowered.Contains("/code")) tags.Add("code");
        if (lowered.Contains("/quote")) tags.Add("quote");
        if (lowered.Contains("/indent")) tags.Add("indent");
        if (lowered.Contains("/spacing")) tags.Add("spacing");

        // Content Tags
        if (lowered.Contains("/summarize")) tags.Add("summarize");
        if (lowered.Contains("/keypoints")) tags.Add("keypoints");
        if (lowered.Contains("/translate")) tags.Add("translate");
        if (lowered.Contains("/paraphrase")) tags.Add("paraphrase");
        if (lowered.Contains("/proofread")) tags.Add("proofread");
        if (lowered.Contains("/factcheck")) tags.Add("factcheck");
        if (lowered.Contains("/citations")) tags.Add("citations");
        if (lowered.Contains("/references")) tags.Add("references");

        // Audience-Specific Tags
        if (lowered.Contains("/technical")) tags.Add("technical");
        if (lowered.Contains("/layman")) tags.Add("layman");
        if (lowered.Contains("/expert")) tags.Add("expert");
        if (lowered.Contains("/beginner")) tags.Add("beginner");
        if (lowered.Contains("/children")) tags.Add("children");
        if (lowered.Contains("/senior")) tags.Add("senior");

        // Purpose Tags
        if (lowered.Contains("/persuasive")) tags.Add("persuasive");
        if (lowered.Contains("/informative")) tags.Add("informative");
        if (lowered.Contains("/instructional")) tags.Add("instructional");
        if (lowered.Contains("/descriptive")) tags.Add("descriptive");
        if (lowered.Contains("/narrative")) tags.Add("narrative");
        if (lowered.Contains("/analytical")) tags.Add("analytical");
        if (lowered.Contains("/critical")) tags.Add("critical");

        // Industry-Specific Tags
        if (lowered.Contains("/legal")) tags.Add("legal");
        if (lowered.Contains("/medical")) tags.Add("medical");
        if (lowered.Contains("/scientific")) tags.Add("scientific");
        if (lowered.Contains("/business")) tags.Add("business");

        // Special Purpose Tags
        if (lowered.Contains("/seo")) tags.Add("seo");
        if (lowered.Contains("/social")) tags.Add("social");
        if (lowered.Contains("/email")) tags.Add("email");
        if (lowered.Contains("/report")) tags.Add("report");
        if (lowered.Contains("/presentation")) tags.Add("presentation");
        if (lowered.Contains("/blog")) tags.Add("blog");
        if (lowered.Contains("/news")) tags.Add("news");

        return tags;
    }
} 