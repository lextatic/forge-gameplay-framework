namespace GameplayTags.Runtime;

/// <summary>
/// An immutable and memory optimized <see cref="string"/> for representing a <see cref="TagName"/>. The class keeps a
/// list of all previously created <see cref="TagName"/> objects and always returns the same instance when a
/// <see cref="TagName"/> is requested. All instantiations should be handled by the <see langword="static"/> method
/// <see cref="FromString(string)"/>.
/// </summary>
public readonly struct TagName : IComparable<TagName>, IEquatable<TagName>
{
	private static readonly Dictionary<string, TagName> _nameTable = new ();

	private readonly string _name;

	/// <summary>
	/// Gets a default Empty <see cref="TagName"/>.
	/// </summary>
	public static TagName Empty { get; } = new (string.Empty);

	private TagName(string name)
	{
		_name = name;
		_nameTable.Add(name, this);
	}

	/// <summary>
	/// Returns an existing instance of <see cref="TagName"/> based on a <see langword="string"/> or creates a new one
	/// if it doesn't already exist.
	/// </summary>
	/// <param name="name">Name of the desired <see cref="TagName"/>.</param>
	/// <returns>A valid <see cref="TagName"/>.</returns>
	public static TagName FromString(string name)
	{
		name = name.ToLower();
		if (_nameTable.TryGetValue(name, out var fName))
		{
			return fName;
		}

		return new TagName(name);
	}

	/// <summary>
	/// Returns a <see cref="string"/> representation of the <see cref="TagName"/>.
	/// </summary>
	/// <returns>The <see cref="TagName"/>'s name as a <see cref="string"/>.</returns>
	public readonly override string ToString()
	{
		return _name;
	}

	/// <summary>
	/// Determines wether this instance and another specified <see cref="TagName"/> object have the same value.
	/// </summary>
	/// <param name="obj">The other <see cref="object"/> to compare against.</param>
	/// <returns><see langword="true"/> if the value of the <paramref name="obj"/> parameter is the same as the value of
	/// this instance; otherwise, <see langword="false"/>. If <paramref name="obj"/> is <see langword="null"/>, the
	/// method returns <see langword="false"/>.</returns>
	public readonly override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj is not TagName tagName)
		{
			return false;
		}

		return _name.Equals(tagName._name);
	}

	/// <summary>
	/// Determines wether this instance and another specified <see cref="TagName"/> object have the same value.
	/// </summary>
	/// <param name="other">The other <see cref="TagName"/> to compare against.</param>
	/// <returns><see langword="true"/> if the value of the <paramref name="other"/> parameter is the same as the value
	/// of this instance; otherwise, <see langword="false"/>. If <paramref name="other"/> is <see langword="null"/>,
	/// the method returns <see langword="false"/>.</returns>
	public readonly bool Equals(TagName other)
	{
		return _name.Equals(other._name);
	}

	/// <summary>
	/// Returns the hash code for this <see cref="TagName"/>.
	/// </summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public readonly override int GetHashCode()
	{
		return _name.GetHashCode();
	}

	/// <summary>
	/// Compares two specified <see cref="TagName"/> objects using
	/// <see cref="StringComparison.InvariantCultureIgnoreCase"/>, and returns an integer that indicates their relative
	/// position in the sort orther.
	/// </summary>
	/// <param name="other">The other <see cref="TagName"/> to compare against.</param>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.
	/// <para><b>Less than zero</b> - this instance precedes <paramref name="other"/> in the sort order.</para>
	/// <para><b>Zero</b> - this instance is the same position as <paramref name="other"/> in the sort order.</para>
	/// <para><b>Greater than zero</b> - this instance follows <paramref name="other"/> in the sort order.</para>
	/// </returns>
	public readonly int CompareTo(TagName other)
	{
		return string.Compare(_name, other._name, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool operator ==(TagName a, TagName b) { return a.Equals(b); }

	public static bool operator !=(TagName a, TagName b) { return !a.Equals(b); }

	public static bool operator <(TagName a, TagName b) { return a.CompareTo(b) < 0; }

	public static bool operator >(TagName a, TagName b) { return a.CompareTo(b) > 0; }

	public static bool operator <=(TagName a, TagName b) { return a.CompareTo(b) <= 0; }

	public static bool operator >=(TagName a, TagName b) { return a.CompareTo(b) >= 0; }
}
