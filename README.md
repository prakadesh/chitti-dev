# Clip Enhancer

A Windows desktop application that enhances your clipboard text using Google's Gemini API. The application monitors your clipboard for tagged text, processes it through the Gemini API, and replaces the clipboard content with the enhanced version.

## Features

- **Clipboard Monitoring**: Automatically detects text with specific tags in your clipboard
- **Text Enhancement**: Uses Google's Gemini API to improve grammar, style, and tone
- **Local Storage**: All data is stored locally in your AppData folder
- **History Tracking**: Keeps a record of all processed texts
- **System Tray Integration**: Runs in the background with easy access through the system tray
- **Settings Management**: Configure API key and other preferences

## Installation

1. Download the latest release from the Releases page
2. Extract the ZIP file to a location of your choice
3. Run `ClipEnhancer.exe`

## Usage

1. **Setup**:
   - Launch the application
   - Click the Settings button
   - Enter your Google Gemini API key
   - Enable clipboard monitoring

2. **Processing Text**:
   - Copy any text to your clipboard
   - Add one or more of these tags at the start of the text:

   **Grammar & Language Tags**:
   - `/grammar` - Corrects grammar, spelling, and punctuation
   - `/punctuation` - Fixes punctuation marks and usage
   - `/spelling` - Corrects spelling errors
   - `/syntax` - Improves sentence structure
   - `/tense` - Ensures consistent verb tense
   - `/agreement` - Fixes subject-verb agreement

   **Tone Tags**:
   - `/formal` - Makes text more professional
   - `/casual` - Makes text more relaxed
   - `/friendly` - Adds warmth and approachability
   - `/polite` - Adds courtesy and respect
   - `/assertive` - Makes text more direct
   - `/diplomatic` - Makes text more tactful
   - `/empathetic` - Adds emotional understanding
   - `/enthusiastic` - Adds energy and excitement
   - `/neutral` - Removes emotional bias
   - `/humorous` - Adds appropriate humor
   - `/serious` - Makes text more grave
   - `/academic` - Makes text more scholarly

   **Style Tags**:
   - `/fluent` - Improves flow and readability
   - `/concise` - Makes text more brief
   - `/detailed` - Adds more information
   - `/firstletter` - Capitalizes first letters
   - `/allcaps` - Converts to all caps
   - `/sentencecase` - Converts to sentence case
   - `/titlecase` - Converts to Title Case
   - `/bulletpoints` - Converts to bullet points
   - `/numbered` - Converts to numbered list
   - `/paragraph` - Formats into paragraphs
   - `/simplify` - Uses simpler vocabulary
   - `/expand` - Uses sophisticated vocabulary
   - `/active` - Converts to active voice
   - `/passive` - Converts to passive voice

   **Format Tags**:
   - `/markdown` - Formats in markdown
   - `/html` - Formats in HTML
   - `/json` - Formats as JSON
   - `/xml` - Formats as XML
   - `/csv` - Formats as CSV
   - `/table` - Converts to table
   - `/code` - Formats as code block
   - `/quote` - Formats as quotation
   - `/indent` - Adds indentation
   - `/spacing` - Adjusts line spacing

   **Content Tags**:
   - `/summarize` - Creates a summary
   - `/keypoints` - Extracts key points
   - `/translate` - Translates text
   - `/paraphrase` - Rewrites in different words
   - `/proofread` - Performs proofreading
   - `/factcheck` - Verifies facts
   - `/citations` - Adds citations
   - `/references` - Adds references

   **Audience-Specific Tags**:
   - `/technical` - Makes text more technical
   - `/layman` - Makes text more general
   - `/expert` - Makes text more specialized
   - `/beginner` - Makes text more basic
   - `/children` - Makes text child-friendly
   - `/senior` - Makes text more accessible

   **Purpose Tags**:
   - `/persuasive` - Makes text more convincing
   - `/informative` - Focuses on information
   - `/instructional` - Makes text tutorial-like
   - `/descriptive` - Adds more description
   - `/narrative` - Makes text story-like
   - `/analytical` - Makes text more analytical
   - `/critical` - Adds critical analysis

   **Industry-Specific Tags**:
   - `/legal` - Uses legal terminology
   - `/medical` - Uses medical terminology
   - `/scientific` - Uses scientific language
   - `/business` - Uses business terminology

   **Special Purpose Tags**:
   - `/seo` - Optimizes for search engines
   - `/social` - Optimizes for social media
   - `/email` - Formats as email
   - `/report` - Formats as report
   - `/presentation` - Formats for presentation
   - `/blog` - Formats as blog post
   - `/news` - Formats as news article

   - The processed text will automatically replace the clipboard content

3. **Viewing History**:
   - Click the History button to view all processed texts
   - Filter by status, tags, or date
   - Copy original or processed text back to clipboard

## Data Storage

All application data is stored locally in:
```
%LOCALAPPDATA%\Clip Enhancer\clips.db
```

The database includes:
- Clipboard history
- Application settings
- API key (encrypted)

## Requirements

- Windows 10 or later
- .NET 7.0 or later
- Google Gemini API key

## Privacy

- All data is stored locally on your machine
- The only external communication is with the Google Gemini API
- Your API key is encrypted before storage
- No data is shared with third parties

## Support

For issues and feature requests, please create an issue in the GitHub repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Development

### Prerequisites

- Visual Studio 2022 or later
- .NET 7.0 SDK
- Google Gemini API key

### Building from Source

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution

### Project Structure

- `Models/`: Data models
- `Views/`: WPF windows and user interface
- `Services/`: Business logic and external service integration
- `Data/`: Database context and configuration

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request 