# HConfigs
A flexible, Reflection-based configuration system

HConfigs is built around the Reflection system, and uses attributes to turn a plain class into a set of options that can be automatically and easily saved, loaded, and read. Config classes are saved to the disk as human-readable text files, and by default support string, int, float, and bool parameters. HConfigs also supports custom parsers for any type you might want.

## Using HConfigs
### Creating a Config Class
To make a class into a configuration, simply mark it with the Config attribute. The Config attribute takes a string argument that specifies the file path for this config, relative to the execution path of the program. In order to specify config options, any property or field can be marked with the Config.Option property. Config options by default will use the name of the field or property in the config file, or a custom name can be specified in the attribute. A comment describing the option and a region for sorting purposes can also be specified. When in the same region, options that are fields come before options that are properties in the file. 

### Saving and Loading
To save or load a configuration class, use the static Config.Save<T>() and Config.Load<T>() methods. Both methods can be passed an optional array of custom parsers that inherit from the OptionParserBase abstract class. Parsers in these arrays will be used in addition to the default parsers when serializing and interpreting the config files.
