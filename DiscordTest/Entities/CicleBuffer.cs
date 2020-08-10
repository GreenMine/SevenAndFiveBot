using System;
using System.Linq;

namespace SevenAndFiveBot.Entities
{
    internal class CircleBuffer<T>
    {
		public const int BUFFER_LENGTH = 64;
		public T[] buffer;
		int pos = 0;

		public CircleBuffer(T default_value)
		{
			buffer = Enumerable.Repeat(default_value, BUFFER_LENGTH).ToArray();
		}

		public int Add(T value) {
			if(pos > BUFFER_LENGTH) pos = 0;
			buffer[pos++] = value;
			return pos - 1;
		}

		public int Get(Predicate<T> f) {
			return Array.FindIndex(buffer, f);
		}
    }
}
