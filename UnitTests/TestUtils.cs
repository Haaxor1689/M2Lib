using JsonDiffPatchDotNet;
using M2Lib.m2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    public static class TestUtils
    {
        static readonly string TestDataPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
            "TestData"
        );

        public static M2 LoadModel(string name)
        {
            var model = new M2();
            var filePath = Path.Combine(TestDataPath, name);
            using (var reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
                model.Load(reader);
            return model;
        }

        public static void SaveModel(string name, M2 model)
        {
            var filePath = Path.Combine(TestDataPath, name);
            using var writer = new BinaryWriter(new FileStream(filePath, FileMode.Create));
            model.Save(writer);
        }

        public static void SaveJson(string name, M2 model)
        {
            var filePath = Path.Combine(TestDataPath, name);
            var jsonString = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }

        private static string GetFormattedJsonDiff<T>(T obj1, T obj2)
        {
            var json1 = JsonConvert.SerializeObject(obj1, Formatting.Indented);
            var json2 = JsonConvert.SerializeObject(obj2, Formatting.Indented);
            var diff = new JsonDiffPatch().Diff(JToken.Parse(json1), JToken.Parse(json2));
            // var diff = new JsonDiffPatch().Diff(
            //     JToken.Parse(@"{ ""foo"": { ""goo"": { ""aaa"": true }, ""bar"": 2 } }"),
            //     JToken.Parse(@"{ ""foo"": { ""goo"": { ""bbb"": true }, ""bar"": 5 } }")
            // );

            if (diff == null)
                return string.Empty;

            return FormatRecursive(diff);
        }

        private static bool IsRoundErrorFloat(JToken token)
        {
            if (token.Children().Count() != 2)
                return false;

            var from = token.Children().First();
            var to = token.Children().Last();
            if (from.Type == JTokenType.Integer && to.Type == JTokenType.Integer)
            {
                var fromVal = Math.Round(from.Value<float>(), 3);
                var toVal = Math.Round(to.Value<float>(), 3);
                return fromVal == toVal;
            }
            return false;
        }

        private static string FormatRecursive(JToken token, string indent = "")
        {
            var result = string.Empty;
            switch (token.Type)
            {
                case JTokenType.Object:
                    if (
                        token.First is JProperty type
                        && type.Name == "_t"
                        && type.Value.ToString() == "a"
                    )
                    {
                        result += "[\n";
                        foreach (var prop in token.Children<JProperty>().Skip(1))
                        {
                            var removed = prop.Name.StartsWith("_");
                            var added =
                                !removed
                                && prop.Value.Type == JTokenType.Array
                                && prop.Value.Children().Count() == 1;
                            var unchanged = IsRoundErrorFloat(prop.Value);
                            if (unchanged && !removed)
                                continue;

                            result += indent;
                            result +=
                                added ? $" +{prop.Name}"
                                : removed ? $" -{prop.Name.Substring(1)}"
                                : $"  {prop.Name}";
                            result += $": {FormatRecursive(prop.Value, indent + "  ")}\n";
                        }
                        result += $"{indent}]";
                    }
                    else
                    {
                        result += "{\n";
                        foreach (var prop in token.Children<JProperty>())
                            result +=
                                $"{indent}  {prop.Name}: {FormatRecursive(prop.Value, indent + "  ")}\n";
                        result += $"{indent}}}";
                    }
                    break;
                case JTokenType.Array:
                    if (!token.Children().Any())
                        result += "???";
                    else if (token.Children().Count() == 2)
                        result += string.Join(" -> ", token.Children());
                    else
                        result += FormatRecursive(token.Children().First(), indent + "  ");

                    break;
                default:
                    result += token.ToString();
                    break;
            }
            return result;
        }

        public static void AssertDeepEqual<T>(string name, T obj1, T obj2)
        {
            var diff = GetFormattedJsonDiff(obj1, obj2);
            if (diff == string.Empty)
                return;

            File.WriteAllText(Path.Combine(TestDataPath, $"Out.{name}.txt"), diff);
            Assert.Fail($"Objects are not equal. See Out.{name}.md for details.");
        }

        public static void CleanupFiles(string[] model)
        {
            foreach (var name in model)
            {
                var filePath = Path.Combine(TestDataPath, name);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}
