namespace GameplayTags.Runtime
{
	public readonly struct TagName : IComparable, IComparable<TagName>, IEquatable<TagName>
	{
		private static readonly Dictionary<string, TagName> _nameTable = new Dictionary<string, TagName>();

		private readonly string _name;

		[Obsolete("Use TagName.FromString() instead.", true)]
		public TagName()
		{
			_name = string.Empty;
		}

		private TagName(string name)
		{
			_name = name;
			_nameTable.Add(name, this);
		}

		public static TagName FromString(string name)
		{
			name = name.ToLower();
			if (_nameTable.TryGetValue(name, out var fName))
			{
				return fName;
			}

			return new TagName(name);
		}

		public static bool operator ==(TagName a, TagName b) { return a.Equals(b); }
		public static bool operator !=(TagName a, TagName b) { return !a.Equals(b); }
		public static bool operator <(TagName a, TagName b) { return a.CompareTo(b) < 0; }
		public static bool operator >(TagName a, TagName b) { return a.CompareTo(b) > 0; }
		public static bool operator <=(TagName a, TagName b) { return a.CompareTo(b) <= 0; }
		public static bool operator >=(TagName a, TagName b) { return a.CompareTo(b) >= 0; }

		public override string ToString()
		{
			return _name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is not TagName tagName)
			{
				throw new ArgumentException("Compared object must be of type TagName.");
			}

			return _name.Equals(tagName._name);
		}

		public bool Equals(TagName other)
		{
			return _name.Equals(other._name);
		}

		public int CompareTo(object? obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is not TagName tagName)
			{
				throw new ArgumentException("Compared object must be of type TagName.");
			}

			return CompareTo(tagName);
		}

		public int CompareTo(TagName other)
		{
			return string.Compare(_name, other._name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
