using SharedLibrary.Models;

namespace SharedLibrary.Events;

public record IskUpdate(int TotalBounty, int LastBounty, Character Character, int CharacterBounty);
