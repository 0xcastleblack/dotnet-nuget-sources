using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;

namespace CastleBlack.NuGetSources
{
    public class Program
    {
        private const string NameRegEx = "[^a-zA-Z0-9]";
        
        [Option(Description = "NuGet Configuration File", ShortName = "f", LongName = "nugetConfigFile")]
        public string ConfigurationFile { get; }

        [Option(Description = "Operation", ShortName = "o", LongName = "operation")]
        public OperationType Operation { get; } = OperationType.List;
        
        [Option(Description = "Name of the NuGet Repository or Credential Key", ShortName = "n", LongName = "name")]
        public string Name { get; }
        
        [Option(Description = "Source of the NuGet Repository", ShortName = "s", LongName = "source")]
        public string Source { get; }
        
        [Option(Description = "Username for accessing the NuGet Repository", ShortName = "u", LongName = "username")]
        public string Username { get; }
        
        [Option(Description = "Password for accessing the NuGet Repository", ShortName = "p", LongName = "password")]
        public string Password { get; }

        [Option(Description = "Protocol Version of the NuGet Repository", ShortName = "v", LongName = "protocolVersion")]
        public int ProtocolVersion { get; } = 3;

        private string GetNuGetConfigurationPath(string configurationFile)
        {
            if (!string.IsNullOrWhiteSpace(configurationFile))
                return (configurationFile);
            switch (Environment.OSVersion.Platform)
            {
                case(PlatformID.MacOSX):
                    return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config"), "NuGet"), "NuGet.Config");
                case(PlatformID.Unix):
                    return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config"), "NuGet"), "NuGet.Config");
                case(PlatformID.Win32NT):
                    return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet"), "NuGet.Config");
                default:
                    throw new PlatformNotSupportedException();
            }
        }
        
        private void OnExecute()
        {
            string nuGetConfigurationFilePath = GetNuGetConfigurationPath(ConfigurationFile);
            FileInfo nuGetConfigurationFile = new FileInfo(nuGetConfigurationFilePath);
            Console.WriteLine($"NuGet Configuration File: {nuGetConfigurationFile.FullName}");
            switch (Operation)
            {
                case(OperationType.Add):
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Source))
                    {
                        Console.Error.WriteLine("Source cannot be an empty string");
                    }
                    AddNuGetSource(nuGetConfigurationFile, Name, Source, ProtocolVersion);
                    break;
                case OperationType.List:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    ListNuGetSources(nuGetConfigurationFile);
                    break;
                case OperationType.Remove:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    RemoveNuGetSource(nuGetConfigurationFile, Name);
                    break;
                case OperationType.Enable:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    EnableNuGetSource(nuGetConfigurationFile, Name);
                    break;
                case OperationType.Disable:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    DisableNuGetSource(nuGetConfigurationFile, Name);
                    break;
                case OperationType.Update:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Source))
                    {
                        Console.Error.WriteLine("Source cannot be an empty string");
                    }
                    UpdateNuGetSource(nuGetConfigurationFile, Name, Source);
                    break;
                case OperationType.Create:
                    if(nuGetConfigurationFile.Exists)
                        if(nuGetConfigurationFile.IsReadOnly)
                        {
                            Console.Error.WriteLine("NuGet configuration file is read only");
                            return;
                        }
                        else
                            nuGetConfigurationFile.Delete();
                    CreateEmptyNuGetConfiguration(nuGetConfigurationFile);
                    break;
                case OperationType.UpdateCredentials:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Username))
                    {
                        Console.Error.WriteLine("Username cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        Console.Error.WriteLine("Password cannot be an empty string");
                    }
                    UpdateNuGetSourceCredentials(nuGetConfigurationFile, Name, Username, Password);
                    break;
                case OperationType.AddCredentials:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Username))
                    {
                        Console.Error.WriteLine("Username cannot be an empty string");
                    }
                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        Console.Error.WriteLine("Password cannot be an empty string");
                    }
                    AddNuGetSourceCredentials(nuGetConfigurationFile, Name, Username, Password);
                    break;
                case OperationType.RemoveCredentials:
                    if(!nuGetConfigurationFile.Exists)
                    {
                        Console.Error.WriteLine("NuGet configuration file does not exist");
                        return;
                    }
                    if(nuGetConfigurationFile.IsReadOnly)
                    {
                        Console.Error.WriteLine("NuGet configuration file is read only");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Console.Error.WriteLine("Name cannot be an empty string");
                    }
                    RemoveNuGetSourceCredentials(nuGetConfigurationFile, Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void UpdateNuGetSourceCredentials(FileInfo nuGetConfigurationFile, string name,
            string username, string password)
        {
            name = name.Replace(" ", "_x0020_");
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode checkNode = rootNode.SelectSingleNode($"/configuration/packageSourceCredentials/{name}");
            if (checkNode == null)
            {
                Console.Error.WriteLine("Credentials do not exist");
                return;
            }
            checkNode.ParentNode.RemoveChild(checkNode);
            
            XmlNode credentialNode = document.CreateNode(XmlNodeType.Element, name, null);
            
            XmlNode usernameNode = document.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute keyAttribute = document.CreateAttribute("key");
            keyAttribute.Value = "Username";
            usernameNode.Attributes.Append(keyAttribute);

            XmlAttribute valueAttribute = document.CreateAttribute("value");
            valueAttribute.Value = username;
            usernameNode.Attributes.Append(valueAttribute);

            credentialNode.AppendChild(usernameNode);
            
            XmlNode passwordNode = document.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute keyAttribute1 = document.CreateAttribute("key");
            keyAttribute1.Value = "ClearTextPassword";
            passwordNode.Attributes.Append(keyAttribute1);

            XmlAttribute valueAttribute1 = document.CreateAttribute("value");
            valueAttribute1.Value = password;
            passwordNode.Attributes.Append(valueAttribute1);

            credentialNode.AppendChild(passwordNode);

            XmlNode packageSourceCredentials = rootNode.SelectSingleNode("/configuration/packageSourceCredentials");
            packageSourceCredentials.AppendChild(credentialNode);
            
            Console.WriteLine("Credentials have been updated");
            document.Save(nuGetConfigurationFile.FullName);
        }

        private void RemoveNuGetSourceCredentials(FileInfo nuGetConfigurationFile, string name)
        {
            name = name.Replace(" ", "_x0020_");
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode checkNode = rootNode.SelectSingleNode($"/configuration/packageSourceCredentials/{name}");
            if (checkNode == null)
            {
                Console.Error.WriteLine("Credentials do not exist");
                return;
            }
            checkNode.ParentNode.RemoveChild(checkNode);
            Console.WriteLine("Credentials have been removed");
            document.Save(nuGetConfigurationFile.FullName);
        }

        private void AddNuGetSourceCredentials(FileInfo nuGetConfigurationFile, string name, string username, string password)
        {
            name = name.Replace(" ", "_x0020_");
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode checkNode = rootNode.SelectSingleNode($"/configuration/packageSourceCredentials/{name}");
            if (checkNode != null)
            {
                Console.Error.WriteLine("Credentials already exist");
                return;
            }
            
            XmlNode packageSourceCredentials = rootNode.SelectSingleNode($"/configuration/packageSourceCredentials");
            if (packageSourceCredentials == null)
            {
                XmlNode newPackageSourceCredentials = document.CreateNode(XmlNodeType.Element, "packageSourceCredentials", null);
                rootNode.SelectSingleNode("/configuration").AppendChild(newPackageSourceCredentials);
                packageSourceCredentials = rootNode.SelectSingleNode($"/configuration/packageSourceCredentials");
            }
            
            XmlNode credentialNode = document.CreateNode(XmlNodeType.Element, name, null);
            
            XmlNode usernameNode = document.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute keyAttribute = document.CreateAttribute("key");
            keyAttribute.Value = "Username";
            usernameNode.Attributes.Append(keyAttribute);

            XmlAttribute valueAttribute = document.CreateAttribute("value");
            valueAttribute.Value = username;
            usernameNode.Attributes.Append(valueAttribute);

            credentialNode.AppendChild(usernameNode);
            
            XmlNode passwordNode = document.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute keyAttribute1 = document.CreateAttribute("key");
            keyAttribute1.Value = "ClearTextPassword";
            passwordNode.Attributes.Append(keyAttribute1);

            XmlAttribute valueAttribute1 = document.CreateAttribute("value");
            valueAttribute1.Value = password;
            passwordNode.Attributes.Append(valueAttribute1);

            credentialNode.AppendChild(passwordNode);
            
            packageSourceCredentials.AppendChild(credentialNode);
            
            Console.WriteLine("Credentials have been added");
            document.Save(nuGetConfigurationFile.FullName);

        }

        private void CreateEmptyNuGetConfiguration(FileInfo nuGetConfigurationFile)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            using (Stream resource = assembly.GetManifestResourceStream("CastleBlack.NuGetSources.DefaultNuGet.Config.xml"))
            {
                using (StreamWriter writer = File.CreateText(nuGetConfigurationFile.FullName))
                {
                    resource.CopyTo(writer.BaseStream);
                    writer.Close();
                }
            }
            Console.WriteLine("New NuGet.Config file created");
        }

        private void AddNuGetSource(FileInfo nuGetConfigurationFile, string name, string source, int protocolVersion)
        {
            name = Regex.Replace(name, NameRegEx, "");
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode checkNode = rootNode.SelectSingleNode($"/configuration/packageSources/add[@key='{name}']");
            if (checkNode != null)
            {
                Console.Error.WriteLine("Source already exists");
                return;
            }
            
            XmlNode addPackageSource = document.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute keyAttribute = document.CreateAttribute("key");
            keyAttribute.Value = name;
            addPackageSource.Attributes.Append(keyAttribute);

            XmlAttribute valueAttribute = document.CreateAttribute("value");
            valueAttribute.Value = source;
            addPackageSource.Attributes.Append(valueAttribute);

            XmlAttribute protocolVersionAttribute = document.CreateAttribute("protocolVersion");
            protocolVersionAttribute.Value = $"{protocolVersion}";
            addPackageSource.Attributes.Append(protocolVersionAttribute);

            XmlNode packageSources = rootNode.SelectSingleNode("/configuration/packageSources");
            packageSources.AppendChild(addPackageSource);
            
            Console.WriteLine("Source has been added");
            document.Save(nuGetConfigurationFile.FullName);
        }
        
        private void UpdateNuGetSource(FileInfo nuGetConfigurationFile, string name, string source)
        {
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode checkNode = rootNode.SelectSingleNode($"/configuration/packageSources/add[@key='{name}']");
            if (checkNode == null)
            {
                Console.Error.WriteLine("Source does not exist");
                return;
            }

            bool foundValue = false;
            foreach (XmlAttribute attribute in checkNode.Attributes)
            {
                if (attribute.Name != "value")
                    continue;
                attribute.Value = source;
                foundValue = true;
                break;
            }

            if (!foundValue)
            {
                XmlAttribute valueAttribute = document.CreateAttribute("value");
                valueAttribute.Value = source;
                checkNode.Attributes.Append(valueAttribute);
            }
            
            Console.WriteLine("Source has been updated");
            document.Save(nuGetConfigurationFile.FullName);
        }

        private void ListNuGetSources(FileInfo nuGetConfigurationFile)
        {
            Console.WriteLine("Listing NuGet Sources");
            Console.WriteLine();
            ConsoleTable table = new ConsoleTable("Name", "Source", "Protocol Version");
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            foreach (XmlNode packageSourceNode in rootNode.SelectNodes("/configuration/packageSources/add"))
            {
                if(packageSourceNode.Attributes == null)
                    continue;
                table.AddRow(
                    packageSourceNode.Attributes["key"]?.Value,
                    packageSourceNode.Attributes["value"]?.Value,
                    packageSourceNode.Attributes["protocolVersion"]?.Value
                );
            }
            table.Write(Format.MarkDown);
            Console.WriteLine();
        }

        private void RemoveNuGetSource(FileInfo nuGetConfigurationFile, string name)
        {
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode removeNode = rootNode.SelectSingleNode($"/configuration/packageSources/add[@key='{name}']");
            if (removeNode == null)
            {
                Console.Error.WriteLine("Unable to find the source to remove");
                return;
            }
            removeNode.ParentNode.RemoveChild(removeNode);
            document.Save(nuGetConfigurationFile.FullName);
        }
        
        private void EnableNuGetSource(FileInfo nuGetConfigurationFile, string name)
        {
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode enableNode = rootNode.SelectSingleNode($"/configuration/packageSources/add[@key='{name}']");
            if (enableNode == null)
            {
                Console.Error.WriteLine("Unable to find the source to enable");
                return;
            }
            
            XmlNode disabledPackageSources = rootNode.SelectSingleNode($"/configuration/disabledPackageSources/add[@key='{name}']");
            if (disabledPackageSources != null)
            {
                disabledPackageSources.ParentNode.RemoveChild(disabledPackageSources);
            }
            else
            {
                Console.WriteLine("Source is not disabled");
                return;
            }
            Console.WriteLine("Source is now enabled");
            document.Save(nuGetConfigurationFile.FullName);
        }
        
        private void DisableNuGetSource(FileInfo nuGetConfigurationFile, string name)
        {
            XmlDocument document = new XmlDocument();  
            document.Load(nuGetConfigurationFile.FullName);  
            XmlNode rootNode = document.DocumentElement;
            XmlNode disableNode = rootNode.SelectSingleNode($"/configuration/packageSources/add[@key='{name}']");
            if (disableNode == null)
            {
                Console.Error.WriteLine("Unable to find the source to disable");
                return;
            }
            
            XmlNode disabledPackageSource = rootNode.SelectSingleNode($"/configuration/disabledPackageSources/add[@key='{name}']");
            if (disabledPackageSource != null)
            {
                bool addAttribute = true;
                foreach (XmlAttribute attribute in disabledPackageSource.Attributes)
                {
                    if (attribute.Name != "value")
                        continue;
                    attribute.Value = "true";
                    addAttribute = false;
                    break;
                }

                if (addAttribute)
                {
                    XmlAttribute newAttribute = document.CreateAttribute("value");
                    newAttribute.Value = "true";
                    disabledPackageSource.Attributes.Append(newAttribute);
                }
            }
            else
            {
                XmlNode newDisabledPackageSource = document.CreateNode(XmlNodeType.Element, "add", null);
                XmlAttribute keyAttribute = document.CreateAttribute("key");
                keyAttribute.Value = name;
                newDisabledPackageSource.Attributes.Append(keyAttribute);

                XmlAttribute valueAttribute = document.CreateAttribute("value");
                valueAttribute.Value = "true";
                newDisabledPackageSource.Attributes.Append(valueAttribute);

                XmlNode disabledPackageSources = rootNode.SelectSingleNode("/configuration/disabledPackageSources");
                if (disabledPackageSources == null)
                {
                    XmlNode newDisabledPackageSources = document.CreateNode(XmlNodeType.Element, "disabledPackageSources", null);
                    rootNode.SelectSingleNode("/configuration").AppendChild(newDisabledPackageSources);
                    disabledPackageSources = rootNode.SelectSingleNode("/configuration/disabledPackageSources");
                }
                disabledPackageSources.AppendChild(newDisabledPackageSource);
            }
            Console.Error.WriteLine("Source is now disabled");
            document.Save(nuGetConfigurationFile.FullName);
        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
    }
}