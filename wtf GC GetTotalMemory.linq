<Query Kind="Program" />

void Main()
{
	long availableMemory = GC.GetTotalMemory(false);
	Console.WriteLine($"Available memory 1: {String.Format("{0:#,##0}", availableMemory)} bytes", availableMemory);

	availableMemory = GC.GetTotalMemory(false);
	Console.WriteLine($"Available memory 2: {String.Format("{0:#,##0}", availableMemory)} bytes", availableMemory);

	var t = new List<Guid>();
	for (int i = 0; i < 10000; i++)
	{
		t.Add(Guid.NewGuid());
	}

	var c = t;

}

