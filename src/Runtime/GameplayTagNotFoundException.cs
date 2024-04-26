namespace GameplayTags.Runtime
{
	public class GameplayTagNotFoundException : Exception
	{
		public GameplayTagNotFoundException(TagName tagName) :
			base($"GameplayTag for TagName: '{tagName}' could not be found within the tags tree.")
		{

		}
	}
}
