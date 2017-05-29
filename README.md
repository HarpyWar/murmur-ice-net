
# Murmur Ice Wrapper for .NET

This is .NET wrapper of Mumble server [Ice middleware](https://wiki.mumble.info/wiki/Ice).

It is used in [Yulli Mur](http://yulli.cleanvoice.ru) and in production for [Free Mumble Server](http://cleanvoice.ru/free/mumble/) and [CleanVoice Control Panel](https://control.cleanvoice.ru).

## Obtaining

Download an archive from Releases, put all the files into a directory with your program and reference **MurmurAdapter** and **MurmurVersion** to your project.

## Usage Example
```C#

try
{
	// create adapter for Murmur_1.2.9.dll
	var instance = new MurmurAdapter.Adapter("1.2.9").Instance;
	instance.Connect("127.0.0.1", 6502, "secret");

	int users = 0;
	foreach (var s in instance.GetAllServers())
	{
		if (s.Value.IsRunning())
		{
			users += s.GetOnline();
			Console.WriteLine("[0] {1} users online", s.Value.Port, users);
		}
	}
	Console.WriteLine("\nOverall users online: {0}", users);
}
catch (ConnectionRefusedException e)
{
	Console.WriteLine("Could not connect");
}
catch (InvalidSecretException e)
{
	Console.WriteLine("Wrong Ice secret");
}
catch (Exception e)
{
	Console.WriteLine(e.Message);
}
```	

## Projects

### Murmur

The main wrapper implementation. But it should not be referenced to your project. It loaded by **MurmurAdapter** at runtime.

You have to reference only **MurmurAdapter** and **MurmurPlugin**.

### MurmurAdapter

Adapter that used to connect different **Murmur** Ice slice versions. 

You can have several Ice connections with different slice versions inside one project.

### MurmurPlugin

Contain types and interfaces for **Murmur**.

### MurmurVersionCompiler

Compiler for **Murmur**. When start it iterates `MurmurVersionCompiler\Slice\*.cs` files and replace `Murmur.cs` inside **Murmur** project with the subsequent build the project. 

Thus each Ice slice will be compiled into `Murmur\bin\Murmur_{VERSION}`.dll

You have to put the dlls you need into a directory with your project.


## Documentation

[Go to wiki](https://github.com/HarpyWar/murmur-ice-net/wiki)
