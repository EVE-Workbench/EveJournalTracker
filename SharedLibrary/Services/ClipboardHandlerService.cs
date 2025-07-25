namespace SharedLibrary.Services;

public class ClipboardHandlerService
    {
        public async Task ProcessClipboardContent(string content)
        {
            Console.WriteLine($"Processing clipboard content: {content}");

            if (IsDungeonInfo(content))
            {
                await HandleDungeonInfo(content);
            }
            
            await HandleGeneralText(content);
        }

        private bool IsDungeonInfo(string content)
        {
            
            return false;
        }

        private async Task HandleDungeonInfo(string dungeonInfo)
        {
            Console.WriteLine($"Dungeon info detected: {dungeonInfo}");
            
            // Process dungeon informatie
            // var dungeonData = ParseDungeonInfo(dungeonInfo);
            // await _dungeonService.ProcessDungeonData(dungeonData);
        }

        private async Task HandleGeneralText(string text)
        {
            // Algemene text processing
            Console.WriteLine($"General text processing for: {text.Substring(0, Math.Min(100, text.Length))}...");
            
            // await _historyService.SaveClipboardEntry(text);
        }
    }